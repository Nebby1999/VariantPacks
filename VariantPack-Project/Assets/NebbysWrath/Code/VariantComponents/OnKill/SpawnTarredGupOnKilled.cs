using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using VAPI;

namespace NW.Components.OnKill
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public class SpawnTarredGupOnKilled : VariantComponent, IOnKilledServerReceiver
    {
        public CharacterBody characterBody { get; private set; }

        private static GameObject _gupMaster;
        private static GameObject _geepMaster;
        private static GameObject _gipMaster;

        private static BodyIndex _moffClaymanBody;

        [SystemInitializer]
        private static void Init()
        {
            MasterCatalog.availability.CallWhenAvailable(() =>
            {
                _gupMaster = MasterCatalog.FindMasterPrefab("GupMaster");
                _geepMaster = MasterCatalog.FindMasterPrefab("GeepMaster");
                _gipMaster = MasterCatalog.FindMasterPrefab("GipMaster");
            });

            BodyCatalog.availability.CallWhenAvailable(() =>
            {
                _moffClaymanBody = BodyCatalog.FindBodyIndex("MoffeinClayManBody");
            });
        }

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
        }

        public void OnKilledServer(DamageReport damageReport)
        {
            if (VariantSpawnManager.instance == null)
                return;

            GameObject masterToUse = null;
            CharacterVariantDef[] variantDefs = null;
            if (_moffClaymanBody != BodyIndex.None && characterBody.bodyIndex == _moffClaymanBody)
            {
                masterToUse = _gipMaster;
                variantDefs = new CharacterVariantDef[] { NWContent.CharacterVariants.TarredGip };
            }
            else if (characterBody.bodyIndex == RoR2Content.BodyPrefabs.ClayBruiserBody.bodyIndex)
            {
                masterToUse = _geepMaster;
                variantDefs = new CharacterVariantDef[] { NWContent.CharacterVariants.TarredGeep };
            }
            else if (characterBody.bodyIndex == DLC1Content.BodyPrefabs.ClayGrenadierBody.bodyIndex)
            {
                masterToUse = _gupMaster;
                variantDefs = new CharacterVariantDef[] { NWContent.CharacterVariants.TarredGup };
            }
            else if (characterBody.bodyIndex == RoR2Content.BodyPrefabs.ClayBossBody.bodyIndex)
            {
                masterToUse = _gupMaster;
                variantDefs = new CharacterVariantDef[] { NWContent.CharacterVariants.TarredGup, NWContent.CharacterVariants.Goop };
            }
            else
            {
                return;
            }

            MasterSummon masterSummon = new MasterSummon()
            {
                ignoreTeamMemberLimit = true,
                position = characterBody.corePosition,
                teamIndexOverride = characterBody.teamComponent.teamIndex,
                preSpawnSetupCallback = (master) => Util.CopyEliteStatus(characterBody.masterObject, master.gameObject, false),
                masterPrefab = masterToUse
            };

            VariantMasterSummon variantMasterSummon = new VariantMasterSummon(masterSummon)
            {
                noGoldExpModifications = true,
                noItemReward = true,
                variantDefs = variantDefs
            };

            var splitter = new BodySplitter
            {
                body = characterBody,
                moneyMultiplier = 0.5f,
                count = 2
            };

            variantMasterSummon.onSummonPerformed += (rprt) =>
            {
                var body = rprt.masterSummonReport.summonBodyInstance;
                if (body)
                {
                    body.gameObject.layer = LayerIndex.debris.intVal;
                    body.StartCoroutine(SwapBack(body));
                }
            };

            splitter.masterSummon = masterSummon;
            splitter.Perform();
        }


        private IEnumerator SwapBack(CharacterBody obj)
        {
            while (characterBody)
                yield return null;

            if (!obj)
                yield break;
            obj.gameObject.layer = LayerIndex.defaultLayer.intVal;
        }

    }
}