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
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public sealed class AmalgamatedSummonMinions : VariantComponent, IOnKilledServerReceiver
    {
        public CharacterBody characterBody { get; private set; }

        public int count = 5;

        private bool killed = false;
        private bool didSpawns = false;
        private Run.FixedTimeStamp timeUntilSpawns;

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        private void FixedUpdate()
        {
            if (killed == false)
                return;

            if(timeUntilSpawns.hasPassed && didSpawns == false)
            {
                DoSpawns();
            }
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            killed = true;
            timeUntilSpawns = Run.FixedTimeStamp.now + 1.9f;
        }

        private void DoSpawns()
        {
            if (!VariantSpawnManager.instance || didSpawns == true)
            {
                return;
            }
            didSpawns = true;

            DeathRewards deathRewards = GetComponent<DeathRewards>();
            for (int i = 0; i < count; i++)
            {
                Vector3 position = characterBody.corePosition + (5 * UnityEngine.Random.insideUnitSphere);
                MasterSummon masterSummon = new MasterSummon()
                {
                    masterPrefab = MasterCatalog.FindMasterPrefab("WispMaster"),
                    useAmbientLevel = false,
                    ignoreTeamMemberLimit = true,
                    position = position,
                    preSpawnSetupCallback = (mstr) => Util.CopyEliteStatus(characterBody.masterObject, mstr.gameObject, false),
                    teamIndexOverride = characterBody.teamComponent.teamIndex
                };

                VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
                {
                    deathRewardsCoefficient = 0.2f,
                    summonerDeathRewards = characterBody.GetComponent<DeathRewards>(),
                    noGoldExpModifications = true,
                    noItemReward = true,
                    variantDefs = Array.Empty<CharacterVariantDef>()
                };

                variantSummon.Perform();
            }
        }
    }
}