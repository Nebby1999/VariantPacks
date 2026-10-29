//Clayman: Head- pos=(0,4,0)scale=(0.2,0.2,0.2)
//Clay bruiser: Head-pos=(0,0.5, 0) scale=(0.5, 0.5, 0.5)
//Apothecary: Head-pos=0,-1,-3.5) scale = 0.75, 0.75 0.75
//Dunestrider Muzzle-pos(0, 0, 0) scale = 1.5, 1.5 1.5

using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using VAPI;

namespace NW.Components
{
    //TODO: This can be turned into VisualModifiers for each variant alongside a proper
    //OnDeath component spawn, similar to SpectralJellyfish
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnServer = true, useOnClient = true)]
    public class AttachClayBubbles : VariantComponent, ILifeBehavior
    {
        private static GameObject _effectPrefab;
        private static GameObject _gupMaster;
        private static GameObject _geepMaster;
        private static GameObject _gipMaster;

        private static BodyIndex _moffclaymanBody;
        private static BodyIndex _clayBruiserBody;
        private static BodyIndex _clayApothecaryBody;
        private static BodyIndex _dunestriderBody;

        public CharacterBody characterBody { get; private set; }
        private GameObject _effectInstance;
        private Transform _chosenTransform;
        private BodyIndex _myIndex;

        [AsyncAssetLoad]
        private static IEnumerator Load()
        {
            var effectRequest = NWAssets.LoadAssetAsync<GameObject>("ClayOverflow");

            while (!effectRequest.isComplete)
                yield return null;

            _effectPrefab = effectRequest.asset;

            BodyCatalog.availability.CallWhenAvailable(() =>
            {
                _moffclaymanBody = BodyCatalog.FindBodyIndex("MoffeinClayManBody");
                _clayBruiserBody = BodyCatalog.FindBodyIndex("ClayBruiserBody");
                _clayApothecaryBody = BodyCatalog.FindBodyIndex("ClayGrenadierBody");
                _dunestriderBody = BodyCatalog.FindBodyIndex("ClayBossBody");
            });

            RoR2Application.onLoad += () =>
            {
                _gupMaster = MasterCatalog.FindMasterPrefab("GupMaster");
                _geepMaster = MasterCatalog.FindMasterPrefab("GeepMaster");
                _gipMaster = MasterCatalog.FindMasterPrefab("GipMaster");
            };
        }

        public void OnDeathStart()
        {
            if(_effectInstance)
                Destroy(_effectInstance);

            if(NetworkServer.active)
                DoSpawnServer();
        }

        private void DoSpawnServer()
        {
            /*
            VariantSummon variantSummon = new VariantSummon()
            {
                applyOnStart = true,
                ignoreTeamMemberLimit = true,
                supressRewards = true,
                position = _chosenTransform.position,
                teamIndexOverride = characterBody.teamComponent.teamIndex,
                inventoryToCopy = characterMaster.inventory,
            };

            if (_myIndex == _moffclaymanBody)
            {
                variantSummon.masterPrefab = _gipMaster;
                variantSummon.variantDefs = new VariantDef[] { _tarredGip };
            }
            else if (_myIndex == _clayBruiserBody)
            {
                variantSummon.masterPrefab = _geepMaster;
                variantSummon.variantDefs = new VariantDef[] { _tarredGeep };
            }
            else if (_myIndex == _clayApothecaryBody)
            {
                variantSummon.masterPrefab = _gupMaster;
                variantSummon.variantDefs = new VariantDef[] { _tarredGup };
            }
            else if (_myIndex == _dunestriderBody)
            {
                variantSummon.masterPrefab = _gupMaster;
                variantSummon.variantDefs = new VariantDef[] { _tarredGup, _goop };
            }
            else
            {
                return;
            }

            var splitter = new BodySplitter
            {
                body = characterBody,
                moneyMultiplier = 0.5f,
                count = 2
            };

            variantSummon.onSummonCompleted += (mstr) =>
            {
                var body = mstr.GetBody();
                body.gameObject.layer = LayerIndex.debris.intVal;
                body.StartCoroutine(SwapBack(body));
            };
            splitter.masterSummon = variantSummon;
            splitter.Perform();
            */
        }

        private IEnumerator SwapBack(CharacterBody obj)
        {
            while (characterBody)
                yield return null;

            if (!obj)
                yield break;
            obj.gameObject.layer = LayerIndex.defaultLayer.intVal;
        }

        private void Start()
        {
            /*_effectInstance = Instantiate(_effectPrefab);

            _myIndex = characterBody.bodyIndex;
            if (_myIndex == BodyIndex.None)
                return;

            if(_myIndex == _moffclaymanBody)
            {
                _chosenTransform = characterModel.childLocator.FindChild("Head");
                _effectInstance.transform.SetParent(_chosenTransform);
                _effectInstance.transform.localPosition = Vector3.up * 0.5f;
                _effectInstance.transform.GetChild(0).localScale = Vector3.one * 0.2f;
            }
            else if(_myIndex == _clayBruiserBody)
            {
                _chosenTransform = characterModel.childLocator.FindChild("Head");
                _effectInstance.transform.SetParent(_chosenTransform);
                _effectInstance.transform.localPosition = Vector3.up * 0.5f;;
                _effectInstance.transform.GetChild(0).localScale = Vector3.one * 0.5f;
            }
            else if(_myIndex == _clayApothecaryBody)
            {
                _chosenTransform = characterModel.childLocator.FindChild("Head");
                _effectInstance.transform.SetParent(_chosenTransform);
                _effectInstance.transform.localPosition = Vector3.up * 0.1f;
                _effectInstance.transform.GetChild(0).localScale = Vector3.one * 0.75f;
            }
            else if(_myIndex == _dunestriderBody)
            {
                _chosenTransform = characterModel.childLocator.FindChild("Muzzle");
                _effectInstance.transform.SetParent(_chosenTransform);
                _effectInstance.transform.localPosition = Vector3.zero;
                _effectInstance.transform.GetChild(0).localScale = Vector3.one * 2f;
            }
            else
            {
                _chosenTransform = characterBody.transform;
                Destroy(_effectInstance);
            }*/
        }
    }
}