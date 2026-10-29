using RoR2;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    //TODO: Maybe just modify the xi construct body itself to have these instead of using a component.
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterModel, useOnClient = true, useOnServer = true)]
    public class CreateBarrageChildLocatorEntries : VariantComponent
    {
        public CharacterModel characterModel { get; set; }

        private void Awake()
        {
            characterModel = GetComponent<CharacterModel>();
        }
        private void Start()
        {
            ChildLocator locator = characterModel.childLocator;

            GameObject barragePoint = null;
            Transform t = null;
            var ROOT = characterModel.transform.Find("MegaConstructArmature/ROOT");
            if (locator.FindChildIndex("BarragePoint0") == -1)
            {
                barragePoint = new GameObject("BarragePoint0");
                t = barragePoint.transform;
                t.SetParent(ROOT);
                t.localPosition = new Vector3(-4.78003979f, -0.0156774241f, 0.254189521f);
                t.localRotation = Quaternion.Euler(new Vector3(1.56115079f, 268.144562f, 359.842438f));
                HG.ArrayUtils.ArrayAppend(ref locator.transformPairs, new ChildLocator.NameTransformPair
                {
                    name = "BarragePoint0",
                    transform = t
                });
            }

            if(locator.FindChildIndex("BarragePoint1") == -1)
            {
                barragePoint = new GameObject("BarragePoint1");
                t = barragePoint.transform;
                t.SetParent(ROOT);
                t.localPosition = new Vector3(5.05441856f, -0.0676202178f, 0.178184763f);
                t.localRotation = Quaternion.Euler(new Vector3(0.210776329f, 91.5235596f, 359.295166f));
                HG.ArrayUtils.ArrayAppend(ref locator.transformPairs, new ChildLocator.NameTransformPair
                {
                    name = "BarragePoint1",
                    transform = t
                });
            }

            if(locator.FindChildIndex("BarragePoint2") == -1)
            {
                barragePoint = new GameObject("BarragePoint2");
                t = barragePoint.transform;
                t.SetParent(ROOT);
                t.localPosition = new Vector3(-0.0125829512f, 5.17679119f, 0.251294643f);
                t.localRotation = Quaternion.Euler(new Vector3(270.913635f, 167.473419f, 191.620148f));
                HG.ArrayUtils.ArrayAppend(ref locator.transformPairs, new ChildLocator.NameTransformPair
                {
                    name = "BarragePoint2",
                    transform = t
                });
            }

            if(locator.FindChildIndex("BarragePoint3") == -1)
            {
                barragePoint = new GameObject("BarragePoint3");
                t = barragePoint.transform;
                t.SetParent(ROOT);
                t.localPosition = new Vector3(-0.0500000007f, -5.19000006f, 0.409999996f);
                t.localRotation = Quaternion.Euler(new Vector3(89.0863571f, 347.477081f, 168.38353f));
                HG.ArrayUtils.ArrayAppend(ref locator.transformPairs, new ChildLocator.NameTransformPair
                {
                    name = "BarragePoint3",
                    transform = t
                });
            }
        }
    }
}