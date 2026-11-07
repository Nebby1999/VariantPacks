using RoR2;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterModel, useOnClient = true)]
    public class ModifyPrintController : VariantComponent
    {
        private CharacterModel _characterModel;
        private void Awake()
        {
            _characterModel = GetComponent<CharacterModel>();
        }
        private void Start()
        {
            var printController = _characterModel.GetComponent<PrintController>();
            if (printController)
            {
                printController.printTime = 25f;
                printController.maxPrintHeight = 100;
            }
        }
    }
}