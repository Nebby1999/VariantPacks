using EntityStates.JellyfishMonster.MOAJ;
using RoR2;
using System.Collections.Generic;
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
        private CharacterVariantProvider chosenMonsterVariantDefProvider;
        private DeathRewards deathRewards;
        public override void OnEnter()
        {
            enterEffectPrefab = DeathState.enterEffectPrefab;
            base.OnEnter();
            deathRewards = GetComponent<DeathRewards>();
            DestroyModel();
            if (NetworkServer.active)
            {
                chosenMonster = MasterCatalog.GetMasterPrefab(validMasters[Random.Range(0, validMasters.Count)]);
                var monsterMaster = chosenMonster.GetComponent<CharacterMaster>();
                if (!monsterMaster.bodyPrefab)
                {
                    DestroyBodyAsapServer();
                    return;
                }
                chosenMonsterVariantDefProvider = CharacterVariantCatalog.GetCharacterVariantProvider(monsterMaster.bodyPrefab.GetComponent<CharacterBody>().bodyIndex);
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
                master.inventory.SetEquipmentIndex(characterBody.inventory.currentEquipmentIndex, false);
        }

        private void SpawnEnemyServer()
        {
            Vector3 position = characterBody.corePosition + (SpawnNova.jellyDropRadius * Random.insideUnitSphere);

            MasterSummon masterSummon = new MasterSummon
            {
                useAmbientLevel = false,
                ignoreTeamMemberLimit = true,
                teamIndexOverride = characterBody.teamComponent.teamIndex,
                position = position,
                masterPrefab = chosenMonster,
                preSpawnSetupCallback = GiveEquipmentIndex,
                summonerBodyObject = null
            };

            var variant = chosenMonsterVariantDefProvider.allVariants[Random.Range(0, chosenMonsterVariantDefProvider.totalVariantCount)];
            VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
            {
                deathRewardsCoefficient = 1f,
                summonerDeathRewards = deathRewards,
                variantDefs = new CharacterVariantDef[] { variant }
            };

            variantSummon.Perform();
        }
        public override void CreateDeathEffects()
        {
            base.CreateDeathEffects();
            if ((bool)enterEffectPrefab)
            {
                EffectManager.SimpleEffect(enterEffectPrefab, transform.position, transform.rotation, transmit: false);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
