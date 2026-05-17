using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI.Legacy.Components;
using RoR2;
using MSU;
using System.Collections;

namespace NW.Components
{
    public class SetupBasicPrintController : VariantComponent
    {
        public static AnimationCurveAsset basicCurve;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var request = NWAssets.LoadAssetAsync<AnimationCurveAsset>("ArchaicStoneWispPrintCurve");

            while (!request.isDone)
                yield return null;

            basicCurve = request.asset;
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
