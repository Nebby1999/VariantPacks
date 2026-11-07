using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VAPI;

namespace NW.Components.OnHit
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public sealed class NullifyingMineOnHitHandler : VariantComponent, IOnDamageDealtServerReceiver
    {
        private CharacterBody _characterBody;

        private AssetReferenceT<GameObject> _nullifierExplosion = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Nullifier.NullifierExplosion_prefab);
        private AsyncOperationHandle<GameObject> _handle;
        private void Awake()
        {
            _characterBody = GetComponent<CharacterBody>();
            _handle = AssetAsyncReferenceManager<GameObject>.LoadAsset(_nullifierExplosion, AsyncReferenceHandleUnloadType.OnSceneUnload);
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            if(damageReport.victimBody && damageReport.attackerBody && damageReport.attackerBody == _characterBody && _characterBody.healthComponent && _characterBody.healthComponent.alive == false)
            {
                int nullifyStackCount = damageReport.victimBody.GetBuffCount(RoR2Content.Buffs.NullifyStack);
                if(nullifyStackCount < 2)
                {
                    damageReport.victimBody.AddBuff(RoR2Content.Buffs.NullifyStack);
                }
                else
                {
                    damageReport.victimBody.ClearTimedBuffs(RoR2Content.Buffs.NullifyStack);
                    damageReport.victimBody.SetBuffCount(RoR2Content.Buffs.NullifyStack.buffIndex, 0);
                    damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.Nullified, 3f);
                }
            }

            EffectManager.SpawnEffect(_handle.WaitForCompletion(), new EffectData
            {
                origin = damageReport.victimBody.corePosition,
                scale = 2,
            }, transmit: true);
        }

        private void OnDestroy()
        {
            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_nullifierExplosion);
            _handle = default;
        }
    }
}