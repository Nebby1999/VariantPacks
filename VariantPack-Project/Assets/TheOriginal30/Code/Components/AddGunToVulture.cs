using RoR2;
using UnityEngine;
using VAPI.Components;

namespace TO30.Components
{
    public class AddGunToVulture : VariantComponent
    {
        private CharacterModel model;
        private ChildLocator childLocator;

        private void Start()
        {
            this.model = base.GetComponent<CharacterModel>();
            this.childLocator = base.GetComponentInChildren<ChildLocator>();

            this.AddGun();
            Destroy(this);
        }

        private void AddGun()
        {
            if (this.model)
            {
                GameObject gun = UnityEngine.Object.Instantiate<GameObject>(TO30Assets.LoadAsset<GameObject>("VulturePistol"), childLocator.FindChild("Head"));
                gun.transform.localPosition = new Vector3(0, 3.5f, 0.5f);
                gun.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 180));
                gun.transform.localScale = Vector3.one * 16f;
            }
        }
    }
}
