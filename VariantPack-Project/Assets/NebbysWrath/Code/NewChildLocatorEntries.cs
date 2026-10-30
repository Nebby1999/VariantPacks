using RoR2;
using UnityEngine;

namespace NW
{
    //Class used to add new child locator entries to vanilla bodies
    internal static class NewChildLocatorEntries
    {
        [SystemInitializer]
        private static void Init()
        {
            RoR2Application.onLoad += () =>
            {
                AddBlindVerminChildLocatorEntries();
                AddXiConstructChildLocatorEntries();
            };
        }

        private static void AddBlindVerminChildLocatorEntries()
        {
            var blindVerminBody = DLC1Content.BodyPrefabs.VerminBody;
            var childLocator = blindVerminBody.GetComponentInChildren<ChildLocator>();

            childLocator.AddChild("Spine1", childLocator.FindChild("Spine2").parent);
        }

        private static void AddXiConstructChildLocatorEntries()
        {
            var majorConstructBody = DLC1Content.BodyPrefabs.MajorConstructBody;
            var childLocator = majorConstructBody.GetComponentInChildren<ChildLocator>();

            var ROOT = childLocator.transform.Find("MegaConstructArmature/ROOT");
            GameObject barragePoint = null;
            Transform t = null;

            barragePoint = new GameObject("BarragePoint0");
            t = barragePoint.transform;
            t.SetParent(ROOT);
            t.localPosition = new Vector3(-4.78003979f, -0.0156774241f, 0.254189521f);
            t.localRotation = Quaternion.Euler(new Vector3(1.56115079f, 268.144562f, 359.842438f));
            childLocator.AddChild("BarragePoint0", t);

            barragePoint = new GameObject("BarragePoint1");
            t = barragePoint.transform;
            t.SetParent(ROOT);
            t.localPosition = new Vector3(5.05441856f, -0.0676202178f, 0.178184763f);
            t.localRotation = Quaternion.Euler(new Vector3(0.210776329f, 91.5235596f, 359.295166f));
            childLocator.AddChild("BarragePoint1", t);

            barragePoint = new GameObject("BarragePoint2");
            t = barragePoint.transform;
            t.SetParent(ROOT);
            t.localPosition = new Vector3(-0.0125829512f, 5.17679119f, 0.251294643f);
            t.localRotation = Quaternion.Euler(new Vector3(270.913635f, 167.473419f, 191.620148f));
            childLocator.AddChild("BarragePoint2", t);


            barragePoint = new GameObject("BarragePoint3");
            t = barragePoint.transform;
            t.SetParent(ROOT);
            t.localPosition = new Vector3(-0.0500000007f, -5.19000006f, 0.409999996f);
            t.localRotation = Quaternion.Euler(new Vector3(89.0863571f, 347.477081f, 168.38353f));
            childLocator.AddChild("BarragePoint3", t);
        }
    }
}