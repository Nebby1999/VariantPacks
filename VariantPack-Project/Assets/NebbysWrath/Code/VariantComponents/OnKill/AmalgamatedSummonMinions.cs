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
    public sealed class AmalgamatedSummonMinions : VariantComponent, IOnKilledServerReceiver
    {
        private static CharacterSpawnCard wispSpawnCard;
        public CharacterBody characterBody { get; private set; }

        public int count = 5;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var spawnCardHandle = Addressables.LoadAssetAsync<CharacterSpawnCard>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Wisp.cscLesserWisp_asset);

            while(spawnCardHandle.Task.IsCompleted == false)
            {
                yield return null;
            }

            wispSpawnCard = spawnCardHandle.Result;
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
                DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(wispSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 0,
                    maxDistance = 5f,
                    position = characterBody.corePosition
                }, VariantSpawnManager.instance.variantSpawnRng)
                {
                    teamIndexOverride = characterBody.teamComponent.teamIndex,
                    onSpawnedServer = (rslt) => Util.CopyEliteStatus(characterBody.masterObject, rslt.spawnedInstance, false),
                    ignoreTeamMemberLimit = true
                };

                VariantDirectorSpawnRequest variantSpawnRequest = new VariantDirectorSpawnRequest(directorSpawnRequest)
                {
                    deathRewardsCoefficient = 0.2f,
                    requesterDeathRewards = characterBody.GetComponent<DeathRewards>(),
                    noGoldExpModifications = true,
                    noItemReward = true,
                    variantDefs = Array.Empty<CharacterVariantDef>()
                };

                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }
    }
}