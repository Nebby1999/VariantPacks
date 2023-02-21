using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.VagrantMonster.Mothership
{
    public class DeployJellies : DeathState
    {
        public static GameObject masterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Jellyfish/JellyfishMaster.prefab").WaitForCompletion();
        public static VariantDef moajDef;
        public static EquipmentIndex index;
        private Vector3 spawnPossition;
        private DeathRewards rewards;

        private static void LoadDef() => moajDef = VariantCatalog.GetVariantDef(VariantCatalog.FindVariantIndex("MOAJ"));
        public override void OnEnter()
        {
            base.OnEnter();
            spawnPossition = characterBody.corePosition;
            index = characterBody.equipmentSlot.equipmentIndex;
            rewards = GetComponent<DeathRewards>();

            if (NetworkServer.active)
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Nebby.TO30") && moajDef)
                {
                    SpawnJellies(3, new VariantDef[] {moajDef});
                }
                else
                {
                    SpawnJellies(10, Array.Empty<VariantDef>());
                }
            }
        }

        private void SpawnJellies(int count, VariantDef[] variantDefs)
        {
            for (int i = 0; i < count; i++)
            {
                var summon = new VariantSummon();
                summon.position = spawnPossition;
                summon.masterPrefab = masterPrefab;
                summon.summonerBodyObject = this.gameObject;
                summon.teamIndexOverride = teamComponent.teamIndex;
                summon.variantDefs = variantDefs;
                summon.applyOnStart = true;
                if(rewards)
                {
                    summon.summonerDeathRewards = rewards;
                    summon.deathRewardsCoefficient = count / 10;
                }
                var jellyMaster = summon.Perform();
                if (jellyMaster)
                {
                    var jelly = jellyMaster.GetBody();
                    jelly.AddTimedBuff(RoR2Content.Buffs.Immune, 1);

                    jelly.inventory.SetEquipmentIndex(index);
                }
            }
        }
    }
}