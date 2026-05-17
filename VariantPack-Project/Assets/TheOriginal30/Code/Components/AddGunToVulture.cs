using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using VAPI.Legacy.Components;

namespace TO30.Components
{
    public class AddGunToVulture : VariantComponent
    {
        private static GameObject _gun;
        private CharacterModel _model;
        private ChildLocator _childLocator;

        [AsyncAssetLoad]
        private static IEnumerator Load()
        {
            var numerator = TO30Assets.LoadAssetAsync<GameObject>("VulturePistol");
            while (!numerator.isDone)
                yield return null;

            _gun = numerator.asset;
        }

        private void Start()
        {
            this._childLocator = base.GetComponentInChildren<ChildLocator>();

            this.AddGun();
            Destroy(this);
        }

        private void AddGun()
        {
            if (characterModel)
            {
                GameObject gun = UnityEngine.Object.Instantiate<GameObject>(_gun, _childLocator.FindChild("Head"));
                gun.transform.localPosition = new Vector3(0, 3.5f, 0.5f);
                gun.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 180));
                gun.transform.localScale = Vector3.one * 16f;
            }
        }
    }
}
