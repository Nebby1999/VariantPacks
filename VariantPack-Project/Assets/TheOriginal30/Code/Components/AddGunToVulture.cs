using MSU;
using System.Collections;
using UnityEngine;
using VAPI;

namespace TO30.Components
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterModel, useOnClient = true)]
    public class AddGunToVulture : VariantComponent
    {
        private static GameObject _gun;
        private ChildLocator _childLocator;

        [AsyncAssetLoad]
        private static IEnumerator Load()
        {
            using var numerator = TO30Assets.LoadAssetAsync<GameObject>("VulturePistol");
            while (!numerator.isComplete)
                yield return null;

            _gun = numerator.asset;
        }

        private void Start()
        {
            _childLocator = GetComponentInChildren<ChildLocator>();

            AddGun();
            Destroy(this);
        }

        private void AddGun()
        {
            if (gameObject && _childLocator)
            {
                GameObject gun = Instantiate(_gun, _childLocator.FindChild("Head"));
                gun.transform.localPosition = new Vector3(0, 3.5f, 0.5f);
                gun.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 180));
                gun.transform.localScale = Vector3.one * 16f;
            }
        }
    }
}
