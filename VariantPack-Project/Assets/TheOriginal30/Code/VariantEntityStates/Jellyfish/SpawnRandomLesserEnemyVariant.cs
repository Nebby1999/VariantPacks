using EntityStates.JellyfishMonster.MOAJ;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.JellyfishMonster.Spectral
{
    public class SpawnRandomLesserEnemyVariant : GenericCharacterDeath
    {
        public static GameObject enterEffectPrefab;
        public static List<MasterCatalog.MasterIndex> validMasters = new List<MasterCatalog.MasterIndex>();
        public static int monsterCount = 1;

        private GameObject chosenMonster;
        private BodyVariantDefProvider chosenMonsterVariantDefProvider;
        private DeathRewards deathRewards;
        public override void OnEnter()
        {
            enterEffectPrefab = DeathState.enterEffectPrefab;
            base.OnEnter();
            deathRewards = GetComponent<DeathRewards>();
            DestroyModel();
            if (NetworkServer.active)
            {
                chosenMonster = MasterCatalog.GetMasterPrefab(validMasters[UnityEngine.Random.Range(0, validMasters.Count)]);
                var monsterMaster = chosenMonster.GetComponent<CharacterMaster>();
                if(!monsterMaster.bodyPrefab)
                {
                    DestroyBodyAsapServer();
                    return;
                }
                chosenMonsterVariantDefProvider = BodyVariantDefProvider.FindProvider(monsterMaster.bodyPrefab.GetComponent<CharacterBody>().bodyIndex);
                for (int i = 0; i < monsterCount; i++)
                {
                    SpawnEnemyServer();
                }
                DestroyBodyAsapServer();
            }
        }

        private void GiveEquipmentIndex(CharacterMaster master)
        {
            if (characterBody.inventory)
                master.inventory.SetEquipmentIndex(characterBody.inventory.currentEquipmentIndex);
        }

        private void SpawnEnemyServer()
        {
            Vector3 position = base.characterBody.corePosition + (SpawnNova.jellyDropRadius * UnityEngine.Random.insideUnitSphere);
            VariantSummon summon = new VariantSummon
            {
                applyOnStart = true,
                useAmbientLevel = false,
                ignoreTeamMemberLimit = true,
                teamIndexOverride = characterBody.teamComponent.teamIndex,
                position = position,
                variantDefs = Array.Empty<VariantDef>(),
                masterPrefab = chosenMonster,
                preSpawnSetupCallback = GiveEquipmentIndex,
                summonerBodyObject = null
            };

            if (deathRewards)
            {
                summon.summonerDeathRewards = deathRewards;
                summon.deathRewardsCoefficient = 1f;
            }

            HG.ArrayUtils.ArrayAppend(ref summon.variantDefs, chosenMonsterVariantDefProvider.GetVariantDef(UnityEngine.Random.Range(0, chosenMonsterVariantDefProvider.TotalVariantCount)));

            summon.Perform();
        }
        public override void CreateDeathEffects()
        {
            base.CreateDeathEffects();
            if ((bool)enterEffectPrefab)
            {
                EffectManager.SimpleEffect(enterEffectPrefab, base.transform.position, base.transform.rotation, transmit: false);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
