using HG;
using RoR2;
using UnityEngine;
using VAPI;

namespace TO30.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public class MOAJSpawnJelliesOnDeath : VariantComponent, IOnKilledServerReceiver
    {
        public int jellyCount = 5;
        public CharacterBody characterBody { get; private set; }
        public DeathRewards deathRewards { get; private set; }
        public CharacterBodyVariantController variantBodyController { get; private set; }

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            deathRewards = GetComponent<DeathRewards>();
            variantBodyController = GetComponent<CharacterBodyVariantController>();
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            GameObject masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(characterBody.bodyIndex));
            if (!masterPrefab)
                return;

            CharacterVariantDef[] variantsToUse = RetrieveVariantDefsExceptMOAJ();
            for(int i = 0; i < jellyCount; i++)
            {
                Vector3 position = characterBody.corePosition + (5 * Random.insideUnitSphere);

                MasterSummon masterSummon = new MasterSummon
                {
                    useAmbientLevel = false,
                    teamIndexOverride = characterBody.teamComponent.teamIndex,
                    ignoreTeamMemberLimit = true,
                    position = position,
                    masterPrefab = masterPrefab,
                    preSpawnSetupCallback = PreSpawnSetupCallback,
                    summonerBodyObject = null,
                };

                VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
                {
                    deathRewardsCoefficient = deathRewards ? 0.2f : 1,
                    summonerDeathRewards = deathRewards,
                    supressArrivalMessage = true,
                    variantDefs = RetrieveVariantDefsExceptMOAJ()
                };

                masterSummon.Perform();
            }
        }

        private CharacterVariantDef[] RetrieveVariantDefsExceptMOAJ()
        {
            using var _ = ListPool<CharacterVariantDef>.RentCollection(out var variantsToUse);
            if (!variantBodyController)
                return variantsToUse.ToArray();

            ReadOnlyArray<CharacterVariantDef> appliedVariantDefs = variantBodyController.GetCharacterVariantDefs();
            foreach(var potentialDef in appliedVariantDefs)
            {
                if (potentialDef == TO30Content.CharacterVariants.MOAJ)
                    continue;

                variantsToUse.Add(potentialDef);
            }

            return variantsToUse.ToArray();
        }

        private void PreSpawnSetupCallback(CharacterMaster master)
        {
            if(characterBody.master)
                Util.CopyEliteStatus(characterBody.master.gameObject, master.gameObject, false);
        }
    }
}