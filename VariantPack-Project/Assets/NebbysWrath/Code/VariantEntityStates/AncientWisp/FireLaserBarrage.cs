using EntityStates;
using MSU;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.MoffeinAncientWispSkills.AncientStoneWisp
{
    public class FireLaserBarrage : BaseState
    {
        public static GameObject projectilePrefab;
        public static GameObject effectPrefab = EntityStates.AncientWispMonster.FireRHCannon.effectPrefab;
        public static float baseDuration = 2f;
        public static float baseDurationBetweenShots = 0.05f;
        public static float damageCoefficient = 2.1f;
        public static float force = 20f;

        public static int maxBullets = 32;
        public static int bulletCount = 12;
        private float duration;
        private float durationBetweenShots;
        public int bulletCountCurrent = 1;
        public int bulletsToFire = 0;

        [AsyncAssetLoad]
        public static IEnumerator LoadAssets()
        {
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/TitanRockProjectile.prefab");

            while (!request.IsDone)
                yield return null;

            projectilePrefab = request.Result;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = base.GetAimRay();

            string text = "MuzzleRight";
            duration = FireLaserBarrage.baseDuration / attackSpeedStat;
            durationBetweenShots = FireLaserBarrage.baseDurationBetweenShots / attackSpeedStat;
            if (FireLaserBarrage.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireLaserBarrage.effectPrefab, base.gameObject, text, false);
            }
            if (base.isAuthority && base.modelLocator && base.modelLocator.modelTransform)
            {
                bulletsToFire = Math.Min(FireLaserBarrage.maxBullets, Mathf.CeilToInt(FireLaserBarrage.bulletCount * attackSpeedStat));
                ChildLocator component = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    Transform transform = component.FindChild(text);
                    if (transform)
                    {
                        Vector3 forward = aimRay.direction;
                        //Vector3 forward = new Vector3(aimRay.direction.x, aimRay.direction.y - 15f, aimRay.direction.z);
                        RaycastHit raycastHit;
                        if (Physics.Raycast(aimRay, out raycastHit, (float)LayerIndex.world.mask))
                        {
                            forward = raycastHit.point - transform.position;
                        }
                        ProjectileManager.instance.FireProjectile(FireLaserBarrage.projectilePrefab, bulletCountCurrent < bulletCount / 2 ? transform.position : aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * FireLaserBarrage.damageCoefficient, FireLaserBarrage.force, base.RollCrit());
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (bulletCountCurrent >= bulletsToFire && base.fixedAge >= duration)
                {
                    outer.SetNextStateToMain();
                    return;
                }
                if (bulletCountCurrent < bulletsToFire && base.fixedAge >= durationBetweenShots)
                {
                    FireLaserBarrage fireRHCannon = new FireLaserBarrage();
                    fireRHCannon.bulletCountCurrent = bulletCountCurrent + 1;
                    fireRHCannon.bulletsToFire = bulletsToFire;
                    outer.SetNextState(fireRHCannon);
                    return;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}