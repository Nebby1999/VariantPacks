using RoR2;
using RoR2.Projectile;
using UnityEngine;
using VAPI;

namespace NW.Components.OnKill
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = false, useOnServer = true)]
    public sealed class InventoryDisableBlastOnKill : VariantComponent, IOnKilledServerReceiver, IOnDamageDealtServerReceiver
    {
        private GameObject collectiveDeathProjectile;
        private CharacterBody _characterBody;

        private void Awake()
        {
            _characterBody = GetComponent<CharacterBody>();
            collectiveDeathProjectile = ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("CollectiveDeathProjectile"));
        }

        public void OnKilledServer(DamageReport damageReport)
        {

            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = collectiveDeathProjectile,
                position = transform.position,
                rotation = transform.rotation,
                owner = gameObject,
                damage = _characterBody.damage,
                force = 0f,
                crit = _characterBody.RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                speedOverride = 0f,
                target = null
            };

            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        private void OnDestroy()
        {
            NWLog.Info($"Destroyed");
        }

        public void OnDamageDealtServer(DamageReport damageReport)
        {
            NWLog.Info($"Dealt damage to {damageReport.attackerBody}");
        }
    }
}