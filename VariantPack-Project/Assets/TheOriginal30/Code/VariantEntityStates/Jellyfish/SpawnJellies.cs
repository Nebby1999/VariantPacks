using EntityStates;
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
                    Vector3 position = base.characterBody.corePosition + (SpawnNova.jellyDropRadius * UnityEngine.Random.insideUnitSphere);

                    VariantSummon summon = new VariantSummon
                    {
                        applyOnStart = false,
                        variantDefs = Array.Empty<VariantDef>(),
                        useAmbientLevel = false,
                        teamIndexOverride = characterBody.teamComponent.teamIndex,
                        ignoreTeamMemberLimit = true,
                        position = position,
                        masterPrefab = masterPrefab,
                        preSpawnSetupCallback = GiveEquipmentIndex,
                        summonerBodyObject = null
                    };
                    if(deathRewards)
                    {
                        summon.deathRewardsCoefficient = 0.2f;
                        summon.summonerDeathRewards = deathRewards;
                    }

                    summon.Perform();
                }
                DestroyBodyAsapServer();
            }
        }

        private void GiveEquipmentIndex(CharacterMaster master)
        {
            if (characterBody.inventory)
                master.inventory.SetEquipmentIndex(characterBody.inventory.currentEquipmentIndex);
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
