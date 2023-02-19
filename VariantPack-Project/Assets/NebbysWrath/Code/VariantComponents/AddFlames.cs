using RoR2;
using UnityEngine;
using VAPI.Components;

namespace NW.Components
{
    public class AddFlames : VariantComponent
    {
        private static GameObject prefab = NWAssets.LoadAsset<GameObject>("IncineratingFlames");
        private CharacterModel model;
        private ChildLocator childLocator;
        private ParticleSystem particleSystem;
        private ParticleSystemRenderer particleSystemRenderer;

        private void Start()
        {
            this.model = base.GetComponent<CharacterModel>();
            this.childLocator = base.GetComponentInChildren<ChildLocator>();
            var muzzle = childLocator.FindChild("MuzzleMouth");
            muzzle.transform.localPosition = new Vector3(0, 2, 0);

            AttatchFlames();
        }

        private void AttatchFlames()
        {
            if (this.model)
            {
                GameObject flamePrefab = UnityEngine.Object.Instantiate<GameObject>(prefab, childLocator.FindChild("Head"));
                particleSystem = flamePrefab.GetComponent<ParticleSystem>();
                particleSystemRenderer = flamePrefab.GetComponent<ParticleSystemRenderer>();
                flamePrefab.transform.localPosition = new Vector3(0, 4.9f, -1f);
                flamePrefab.transform.localRotation = Quaternion.Euler(-84, -12, -80);
                flamePrefab.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            }
        }
    }
}