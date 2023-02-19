using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.LemurianBruiserMonster.GhostBrother
{
    public class BrotherDeath : GenericCharacterDeath
    {
        public static GameObject lemurianMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/LemurianMaster.prefab").WaitForCompletion();
        public static Material ghostMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matGhostEffect.mat").WaitForCompletion();

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                for (int i = 0; i < 3; i++)
                {
                    VariantSummon variantSummon = new VariantSummon();
                    variantSummon.masterPrefab = lemurianMaster;
                    variantSummon.applyOnStart = false;
                    variantSummon.variantDefs = Array.Empty<VariantDef>();
                    variantSummon.ignoreTeamMemberLimit = true;
                    variantSummon.position = characterBody.corePosition + (5 * UnityEngine.Random.insideUnitSphere);
                    variantSummon.rotation = transform.rotation;
                    variantSummon.summonerBodyObject = null;
                    variantSummon.teamIndexOverride = teamComponent.teamIndex;
                    variantSummon.supressRewards = true;

                    var lemmyMaster = variantSummon.Perform();
                    if(lemmyMaster)
                    {
                        var lemmyBodyObjectt = lemmyMaster.bodyInstanceObject;
                        var lemmyInventory = lemmyMaster.inventory;
                        if(lemmyInventory)
                        {
                            lemmyInventory.SetEquipmentIndex(characterBody.inventory ? characterBody.inventory.currentEquipmentIndex : EquipmentIndex.None);
                            lemmyInventory.GiveItem(RoR2Content.Items.BoostHp, Mathf.RoundToInt(6 - 1f) * 10);
                            lemmyInventory.GiveItem(RoR2Content.Items.BoostDamage, Mathf.RoundToInt(3 - 1 * 10));
                        }

                        var lemmyBody = lemmyBodyObjectt.GetComponent<CharacterBody>();
                        lemmyBody.modelLocator.modelTransform.GetComponent<CharacterModel>().baseRendererInfos[0].defaultMaterial = ghostMaterial;
                        lemmyBody.AddTimedBuff(RoR2Content.Buffs.Immune, 1);
                    }
                }
                DestroyBodyAsapServer();
            }
        }
    }
}