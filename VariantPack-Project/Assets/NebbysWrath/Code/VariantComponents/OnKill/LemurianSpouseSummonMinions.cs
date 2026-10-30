using RoR2;
using System;
using UnityEngine;
using VAPI;
using Random = UnityEngine.Random;

namespace NW.Components.OnKill
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody)]
    public sealed class LemurianSpouseSummonMinions : VariantComponent, IOnKilledServerReceiver
    {
        public CharacterBody characterBody { get; private set; }

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            if(!VariantSpawnManager.instance)
            {
                return;
            }

            DeathRewards deathRewards = GetComponent<DeathRewards>();
            for(int i = 0; i < 3; i++)
            {
                MasterSummon masterSummon = new MasterSummon()
                {
                    masterPrefab = MasterCatalog.FindMasterPrefab("LemurianMaster"),
                    ignoreTeamMemberLimit = true,
                    position = characterBody.corePosition + (5 * Random.insideUnitSphere),
                    rotation = transform.rotation,
                    summonerBodyObject = null,
                    teamIndexOverride = characterBody.teamComponent.teamIndex,
                    preSpawnSetupCallback = (mstr) =>
                    {
                        Util.CopyEliteStatus(characterBody.masterObject, mstr.gameObject, true);
                        mstr.inventory.GiveItemPermanent(RoR2Content.Items.Ghost);
                    }
                };

                VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
                {
                    variantDefs = Array.Empty<CharacterVariantDef>(),
                    deathRewardsCoefficient = 1,
                    summonerDeathRewards = deathRewards,
                    noItemReward = true,
                    noGoldExpModifications = true,
                };
            }
        }
    }
}