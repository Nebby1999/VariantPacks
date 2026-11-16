using HG;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using VAPI;
using Random = UnityEngine.Random;

namespace TO30.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public sealed class SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath : VariantComponent, IOnKilledServerReceiver
    {
        public static void AddWeightedMaster(MasterCatalog.MasterIndex master, float weight = 1)
        {
            if (master == MasterCatalog.MasterIndex.none)
                return;

            _allMastersThatMaySpawn.Add(new WeightedMaster { masterIndex = master, weight = weight });
        }
        private struct WeightedMaster
        {
            private struct WeightedMasterComparer : IEqualityComparer<WeightedMaster>
            {
                public bool Equals(WeightedMaster x, WeightedMaster y)
                {
                    return x.masterIndex == y.masterIndex;
                }

                public int GetHashCode(WeightedMaster obj)
                {
                    return obj.masterIndex.GetHashCode();
                }
            }
            public static IEqualityComparer<WeightedMaster> GetEqualityComparer()
            {
                return new WeightedMasterComparer();
            }

            public MasterCatalog.MasterIndex masterIndex;
            public float weight;

            public bool IsAvailable()
            {
                if (!Run.instance)
                    return false;

                var masterPrefab = MasterCatalog.GetMasterPrefab(masterIndex);
                if (!masterPrefab || !masterPrefab.TryGetComponent<CharacterMaster>(out var characterMaster))
                    return false;

                //Ensure the master's expansion requirement is met
                if(masterPrefab.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequirementComponent))
                {
                    //If the master has an expansion requirement, and the expansion is not enabled, its not available.
                    if(expansionRequirementComponent.requiredExpansion && Run.instance.IsExpansionEnabled(expansionRequirementComponent.requiredExpansion) == false)
                    {
                        return false;
                    }
                }

                if (!characterMaster.bodyPrefab)
                    return false;

                if(characterMaster.bodyPrefab.TryGetComponent<ExpansionRequirementComponent>(out expansionRequirementComponent))
                {
                    if (expansionRequirementComponent.requiredExpansion && Run.instance.IsExpansionEnabled(expansionRequirementComponent.requiredExpansion) == false)
                        return false;
                }

                return true;
            }
        }
        private static readonly HashSet<WeightedMaster> _allMastersThatMaySpawn = new HashSet<WeightedMaster>(WeightedMaster.GetEqualityComparer());

        public int enemyCount = 1;
        public CharacterBody characterBody { get; private set; }
        public DeathRewards deathRewards { get; private set; }

        private WeightedSelection<MasterCatalog.MasterIndex> _masterSelection = new WeightedSelection<MasterCatalog.MasterIndex>();
        private GameObject _chosenMasterPrefab;
        private CharacterVariantProvider _chosenMonsterVariantDefProvider;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            deathRewards = GetComponent<DeathRewards>();
            _masterSelection = new WeightedSelection<MasterCatalog.MasterIndex>(_allMastersThatMaySpawn.Count);
            foreach(var weightedMaster in _allMastersThatMaySpawn)
            {
                if(weightedMaster.IsAvailable())
                {
                    _masterSelection.AddChoice(weightedMaster.masterIndex, weightedMaster.weight);
                }
            }
        }

        private void Start()
        {
            if(VariantSpawnManager.instance)
            {
                _chosenMasterPrefab = GetMasterPrefab(VariantSpawnManager.instance.variantSpawnRng.nextNormalizedFloat);
                if (!_chosenMasterPrefab)
                    return;

                var chosenMaster = _chosenMasterPrefab.GetComponent<CharacterMaster>();
                if(!chosenMaster.bodyPrefab)
                {
                    return;
                }

                _chosenMonsterVariantDefProvider = CharacterVariantCatalog.FindCharacterVariantProvider(chosenMaster.bodyPrefab);
                if(_chosenMonsterVariantDefProvider == null)
                {
                    TO30Log.Error($"{this} rolled a {_chosenMasterPrefab} to spawn but said master's body has no variant def provider!");
                    return;
                }
            }
        }

        private GameObject GetMasterPrefab(float normalizedIndex)
        {
            return MasterCatalog.GetMasterPrefab(_masterSelection.Evaluate(normalizedIndex));
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            if(_chosenMasterPrefab == null || _chosenMonsterVariantDefProvider == null || !VariantSpawnManager.instance)
            {
                return;
            }

            Vector3 position = characterBody.corePosition + (5 * Random.insideUnitSphere);

            MasterSummon masterSummon = new MasterSummon
            {
                useAmbientLevel = false,
                ignoreTeamMemberLimit = true,
                teamIndexOverride = characterBody.teamComponent.teamIndex,
                position = position,
                masterPrefab = _chosenMasterPrefab,
                preSpawnSetupCallback = PreSpawnSetupCallback,
                summonerBodyObject = null
            };

            var variant = _chosenMonsterVariantDefProvider.filteredVariants[VariantSpawnManager.instance.variantSpawnRng.RangeInt(0, _chosenMonsterVariantDefProvider.filteredVariants.Count)];
            VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
            {
                deathRewardsCoefficient = 1f,
                summonerDeathRewards = deathRewards,
                preassignedVariantDefs = new CharacterVariantDef[] { variant },
                supressArrivalMessage = true,
                noGoldExpModifications = true,
                noItemReward = true
            };

            masterSummon.Perform();
        }

        private void PreSpawnSetupCallback(CharacterMaster master)
        {
            if(characterBody.master)
                Util.CopyEliteStatus(characterBody.master.gameObject, master.gameObject, false);
        }
    }
}