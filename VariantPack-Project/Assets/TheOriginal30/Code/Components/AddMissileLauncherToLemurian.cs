using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI;

namespace TO30.Components
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterModel, useOnClient = true)]
    public class AddMissileLauncherToLemurian : VariantComponent
    {
        private static GameObject fab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/CommandMissile/DisplayMissileRack.prefab").WaitForCompletion();
        private CharacterModel model;
        private ChildLocator childLocator;
        private void Start()
        {
            model = GetComponentInChildren<CharacterModel>();
            childLocator = GetComponentInChildren<ChildLocator>();

            AddMissileLauncher();
            Destroy(this);
        }

        private void AddMissileLauncher()
        {
            if (model)
            {
                GameObject missileLauncher = Instantiate(fab, childLocator.FindChild("Chest"));
                missileLauncher.transform.localPosition = new Vector3(0, 0, 1.75f);
                missileLauncher.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0, 0));
                missileLauncher.transform.localScale = Vector3.one * 8f;
            }
        }
    }
}