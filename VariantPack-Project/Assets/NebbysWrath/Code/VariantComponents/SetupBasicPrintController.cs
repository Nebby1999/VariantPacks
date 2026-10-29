using VAPI;
using RoR2;
using MSU;
using System.Collections;
using UnityEngine;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterModel, useOnServer = false, useOnClient = true)]
    public class SetupBasicPrintController : VariantComponent
    {
        public static AnimationCurveAsset basicCurve;

        public CharacterModel characterModel { get; private set; }

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var request = NWAssets.LoadAssetAsync<AnimationCurveAsset>("ArchaicStoneWispPrintCurve");

            while (!request.isComplete)
                yield return null;

            basicCurve = request.asset;
        }

        private void Awake()
        {
            characterModel = GetComponent<CharacterModel>();
        }

        public void Start()
        {
            var printController = characterModel.gameObject.EnsureComponent<PrintController>();
            printController.maxPrintHeight = 10;
            printController.printTime = 1;
            printController.startingPrintHeight = -2f;
            printController.printCurve = basicCurve.value;
            Destroy(this);
        }
    }
}
