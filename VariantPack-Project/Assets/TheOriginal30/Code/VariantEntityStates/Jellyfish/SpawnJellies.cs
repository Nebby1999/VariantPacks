using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.JellyfishMonster.MOAJ
{
    public class SpawnJellies : GenericCharacterDeath
    {
        public static GameObject enterEffectPrefab;
        public static GameObject masterPrefab;
        public static int jellies = 5;

        private DeathRewards deathRewards;
        public override void OnEnter()
        {
            enterEffectPrefab = DeathState.enterEffectPrefab;
            masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(characterBody.bodyIndex));
            base.OnEnter();
            deathRewards = GetComponent<DeathRewards>();
            DestroyModel();
            if (NetworkServer.active)
            {
                for (int i = 0; i < SpawnNova.jellyCount; i++)
                {
                    Vector3 position = characterBody.corePosition + (SpawnNova.jellyDropRadius * UnityEngine.Random.insideUnitSphere);

                    MasterSummon masterSummon = new MasterSummon
                    {
                        useAmbientLevel = false,
                        teamIndexOverride = characterBody.teamComponent.teamIndex,
                        ignoreTeamMemberLimit = true,
                        position = position,
                        masterPrefab = masterPrefab,
                        preSpawnSetupCallback = GiveEquipmentIndex,
                        summonerBodyObject = null,
                    };

                    VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
                    {
                        deathRewardsCoefficient = deathRewards ? 0.2f : 1,
                        summonerDeathRewards = deathRewards,
                        variantDefs = Array.Empty<CharacterVariantDef>()
                    };

                    variantSummon.Perform();
                }
                DestroyBodyAsapServer();
            }
        }

        private void GiveEquipmentIndex(CharacterMaster master)
        {
            if (characterBody.inventory)
                master.inventory.SetEquipmentIndex(characterBody.inventory.currentEquipmentIndex, false);
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
