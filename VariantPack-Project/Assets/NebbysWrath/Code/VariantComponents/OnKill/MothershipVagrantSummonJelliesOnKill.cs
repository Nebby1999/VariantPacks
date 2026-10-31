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
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnServer = true, useOnClient = false)]
    public sealed class MothershipVagrantSummonJelliesOnKill : VariantComponent, IOnKilledServerReceiver
    {
        public CharacterBody characterBody { get; private set; }

        public int count = 2;
        private CharacterVariantDef moajDef;


        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        private void Start()
        {
            moajDef = CharacterVariantCatalog.GetCharacterVariantDef(CharacterVariantCatalog.FindCharacterVariantIndex("MOAJ"));
            count = moajDef ? 3 : 10;
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
                MasterSummon masterSummon = new MasterSummon()
                {
                    position = characterBody.corePosition,
                    masterPrefab = MasterCatalog.FindMasterPrefab("JellyfishMaster"),
                    summonerBodyObject = gameObject,
                    teamIndexOverride = characterBody.teamComponent.teamIndex,
                    preSpawnSetupCallback = (mstr) => Util.CopyEliteStatus(characterBody.masterObject, mstr.gameObject, false),
                    ignoreTeamMemberLimit = true,
                };

                VariantMasterSummon variantMasterSummon = new VariantMasterSummon(masterSummon)
                {
                    summonerDeathRewards = characterBody.GetComponent<DeathRewards>(),
                    deathRewardsCoefficient = 1 / count,
                    variantDefs = moajDef ? new CharacterVariantDef[] { moajDef } : Array.Empty<CharacterVariantDef>(),
                };

                masterSummon.Perform();
            }
        }
    }
}