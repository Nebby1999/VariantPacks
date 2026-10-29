using HG;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using VAPI;

namespace TO30.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public sealed class SpectralJellyfishSpawnRandomLesserEnemyVariantOnDeath : VariantComponent, IOnKilledServerReceiver
    {
        public static void AddMaster(MasterCatalog.MasterIndex master)
        {
            if(!_availableMastersToSpawn.Contains(master))
            {
                _availableMastersToSpawn.Add(master);
            }
        }
        private static readonly List<MasterCatalog.MasterIndex> _availableMastersToSpawn = new List<MasterCatalog.MasterIndex>();

        public int enemyCount = 1;
        public CharacterBody characterBody { get; private set; }
        public DeathRewards deathRewards { get; private set; }

        private GameObject chosenMasterPrefab;
        private CharacterVariantProvider chosenMonsterVariantDefProvider;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            deathRewards = GetComponent<DeathRewards>();
        }

        private void Start()
        {
            if(VariantSpawnManager.instance)
            {
                chosenMasterPrefab = MasterCatalog.GetMasterPrefab(VariantSpawnManager.instance.variantSpawnRng.NextElementUniform(_availableMastersToSpawn));
                var chosenMaster = chosenMasterPrefab.GetComponent<CharacterMaster>();
                if(!chosenMaster.bodyPrefab)
                {
                    return;
                }

                chosenMonsterVariantDefProvider = CharacterVariantCatalog.FindCharacterVariantProvider(chosenMaster.bodyPrefab);
                if(chosenMonsterVariantDefProvider == null)
                {
                    TO30Log.Error($"{this} rolled a {chosenMasterPrefab} to spawn but said master's body has no variant def provider!");
                }
            }
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            if(chosenMasterPrefab == null || chosenMonsterVariantDefProvider == null || !VariantSpawnManager.instance)
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
                masterPrefab = chosenMasterPrefab,
                preSpawnSetupCallback = PreSpawnSetupCallback,
                summonerBodyObject = null
            };

            var variant = chosenMonsterVariantDefProvider.allVariants[VariantSpawnManager.instance.variantSpawnRng.RangeInt(0, chosenMonsterVariantDefProvider.totalVariantCount)];
            VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
            {
                deathRewardsCoefficient = 1f,
                summonerDeathRewards = deathRewards,
                variantDefs = new CharacterVariantDef[] { variant },
                supressArrivalMessage = true,
                noGoldExpModifications = true,
                noItemReward = true
            };

            variantSummon.Perform();
        }

        private void PreSpawnSetupCallback(CharacterMaster master)
        {
            if(characterBody.master)
                Util.CopyEliteStatus(characterBody.master.gameObject, master.gameObject, false);
        }
    }
}