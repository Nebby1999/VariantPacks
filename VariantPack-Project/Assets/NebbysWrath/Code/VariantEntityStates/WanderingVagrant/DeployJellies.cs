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

        private static void LoadDef() => moajDef = VariantCatalog.GetVariantDef(VariantCatalog.FindVariantIndex("MOAJ"));
        public override void OnEnter()
        {
            spawnPossition = characterBody.corePosition;
            index = characterBody.equipmentSlot.equipmentIndex;

            base.OnEnter();

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
            for (int i = 0; i < 10; i++)
            {
                var summon = new VariantSummon();
                summon.position = spawnPossition;
                summon.masterPrefab = masterPrefab;
                summon.summonerBodyObject = this.gameObject;
                summon.variantDefs = variantDefs;
                summon.supressRewards = true;
                summon.applyOnStart = true;
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