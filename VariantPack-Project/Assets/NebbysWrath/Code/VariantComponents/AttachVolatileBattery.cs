using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI;

namespace NW.Components
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public sealed class AttachVolatileBattery : VariantComponent
    {
        private static GameObject prefab = NWAssets.LoadAsset<GameObject>("ExplosiveGupAttachment");
        private NetworkedBodyAttachment attachment;
        private CharacterBody _body;

        private void Awake()
        {
            _body = GetComponent<CharacterBody>();    
        }

        private void Start()
        {
            attachment = Object.Instantiate(prefab.GetComponent<NetworkedBodyAttachment>());
            attachment.AttachToGameObjectAndSpawn(_body.gameObject);
        }

        private void OnDestroy()
        {
            if ((bool)attachment)
            {
                Object.Destroy(attachment.gameObject);
                attachment = null;
            }
        }
    }
}