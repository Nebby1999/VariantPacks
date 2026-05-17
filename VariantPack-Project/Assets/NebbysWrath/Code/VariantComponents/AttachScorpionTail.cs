using MSU;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI.Legacy.Components;

namespace NW.Components
{
    public class AttachScorpionTail : VariantComponent
    {
        private static GameObject _scorpionDisplay;
        private static CharacterBody _verminBody;
        private static CharacterBody _pestBody;

        public bool isPest => characterBody && characterBody.bodyIndex == _pestBody.bodyIndex;
        private GameObject _scorpionInstance;
        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var scorpionRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/PermanentDebuffOnHit/DisplayScorpion.prefab");
            var verminRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Vermin/VerminBody.prefab");
            var pestRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/FlyingVermin/FlyingVerminBody.prefab");

            var routine = new ParallelCoroutine();
            routine.Add(scorpionRequest);
            routine.Add(verminRequest);
            routine.Add(pestRequest);

            while (!routine.IsDone())
                yield return null;

            _scorpionDisplay = scorpionRequest.Result;
            _verminBody = verminRequest.Result.GetComponent<CharacterBody>();
            _pestBody = pestRequest.Result.GetComponent<CharacterBody>();
        }

        private void Start()
        {
            var index = characterBody.bodyIndex;
            if (!(index == _verminBody.bodyIndex || index == _pestBody.bodyIndex))
            {
                return;
            }

            if (!characterModel || !characterModel.childLocator)
                return;

            var targetTransform = isPest ? characterModel.childLocator.FindChild("Body") : characterModel.transform.Find("VerminArmature/ROOT/base/Spine1");

            _scorpionInstance = Instantiate(_scorpionDisplay, targetTransform);
            _scorpionInstance.transform.localPosition = Vector3.zero;
            _scorpionInstance.transform.localScale = Vector3.one * 5;
        }
    }
}