using MSU;
using RoR2;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI;

namespace NW.Components.OnKill
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody)]
    public sealed class ChildishParentSummonParentsOnKill : VariantComponent, IOnKilledServerReceiver
    {
        private static CharacterSpawnCard parentCard;
        public CharacterBody characterBody { get; private set; }

        public int count = 2;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var spawnCardHandle = Addressables.LoadAssetAsync<CharacterSpawnCard>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Parent.cscParent_asset);

            while (spawnCardHandle.Task.IsCompleted == false)
            {
                yield return null;
            }

            parentCard = spawnCardHandle.Result;
        }

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            if (!VariantSpawnManager.instance)
            {
                return;
            }

            DeathRewards deathRewards = GetComponent<DeathRewards>();
            for (int i = 0; i < count; i++)
            {
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(parentCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 3,
                    maxDistance = 20f,
                    position = characterBody.corePosition
                }, VariantSpawnManager.instance.variantSpawnRng)
                {
                    teamIndexOverride = characterBody.teamComponent.teamIndex,
                    onSpawnedServer = (rslt) => Util.CopyEliteStatus(characterBody.masterObject, rslt.spawnedInstance, false),
                    ignoreTeamMemberLimit = true
                };

                VariantDirectorSpawnRequest variantSpawnRequest = new VariantDirectorSpawnRequest(directorSpawnRequest)
                {
                    deathRewardsCoefficient = 0.5f,
                    requesterDeathRewards = characterBody.GetComponent<DeathRewards>(),
                    noGoldExpModifications = true,
                    noItemReward = true,
                    variantDefs = Array.Empty<CharacterVariantDef>()
                };

                var parentMaster = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                if(parentMaster && parentMaster.TryGetComponent<CharacterMaster>(out var master) && master.TryGetBody(out var body))
                {
                    body.AddBuff(RoR2Content.Buffs.WarCryBuff);
                }
            }
        }
    }
}