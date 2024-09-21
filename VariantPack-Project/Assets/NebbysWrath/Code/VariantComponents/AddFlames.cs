using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using VAPI.Components;

namespace NW.Components
{
    public class AddFlames : VariantComponent
    {
        private static GameObject _prefab = NWAssets.LoadAsset<GameObject>("IncineratingFlames");
        private ChildLocator _childLocator;

        [AsyncAssetLoad]
        private static IEnumerator Load()
        {
            var routine = NWAssets.LoadAssetAsync<GameObject>("IncineratingFlames");
            while (!routine.isDone)
                yield return null;

            _prefab = routine.asset;
        }
        private void Start()
        {
            this._childLocator = GetComponentInChildren<ChildLocator>();


            AttatchFlames();
        }

        private void AttatchFlames()
        {
            if (this.characterModel && _childLocator)
            {
                var muzzle = _childLocator.FindChild("MuzzleMouth");
                muzzle.transform.localPosition = new Vector3(0, 2, 0);

                GameObject flamePrefab = UnityEngine.Object.Instantiate<GameObject>(_prefab, _childLocator.FindChild("Head"));
                var _particleSystem = flamePrefab.GetComponent<ParticleSystem>();
                var _particleSystemRenderer = flamePrefab.GetComponent<ParticleSystemRenderer>();
                flamePrefab.transform.localPosition = new Vector3(0, 4.9f, -1f);
                flamePrefab.transform.localRotation = Quaternion.Euler(-84, -12, -80);
                flamePrefab.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            }
        }
    }
}