using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI.Components;

namespace TO30.Components
{
    public class AddMissileLauncherToLemurian : VariantComponent
    {
        private static GameObject fab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/CommandMissile/DisplayMissileRack.prefab").WaitForCompletion();
        private CharacterModel model;
        private ChildLocator childLocator;
        private void Start()
        {
            this.model = GetComponentInChildren<CharacterModel>();
            this.childLocator = GetComponentInChildren<ChildLocator>();

            this.AddMissileLauncher();
            Destroy(this);
        }

        private void AddMissileLauncher()
        {
            if (this.model)
            {
                GameObject missileLauncher = UnityEngine.Object.Instantiate(fab, childLocator.FindChild("Chest"));
                missileLauncher.transform.localPosition = new Vector3(0, 0, 1.75f);
                missileLauncher.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0, 0));
                missileLauncher.transform.localScale = Vector3.one * 8f;
            }
        }
    }
}