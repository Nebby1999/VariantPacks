using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAPI.Components;
using Moonstorm;
using RoR2;

namespace NW.Components
{
    public class SetupBasicPrintController : VariantComponent
    {
        public static AnimationCurveAsset basicCurve = NWAssets.LoadAsset<AnimationCurveAsset>("ArchaicStoneWispPrintCurve");
        public void Start()
        {
            var printController = CharacterModel.gameObject.EnsureComponent<PrintController>();
            printController.maxPrintHeight = 10;
            printController.printTime = 1;
            printController.startingPrintHeight = -2f;
            printController.printCurve = basicCurve.value;
            Destroy(this);
        }
    }
}
