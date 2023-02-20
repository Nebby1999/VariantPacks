using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityStates.Bell.BellWeapon;
using RoR2.Projectile;
using System.Globalization;
using RoR2;

namespace EntityStates.Bell.BellWeapon.Steel
{
    public class MegaSteelBall : BaseState
    {
        public static float basePrepDuration = ChargeTrioBomb.basePrepDuration;
        public static float baseTimeBetweenPreps = ChargeTrioBomb.baseTimeBetweenPreps;
        public static GameObject preppedBombPrefab = NW.PrefabClones.MegaSteelBall.PreppedPrefab;
        public static float baseBarrageDuration = ChargeTrioBomb.baseBarrageDuration;
        public static float baseTimeBetweenBarrages = ChargeTrioBomb.baseTimeBetweenBarrages;
        public static GameObject bombProjectilePrefab = NW.PrefabClones.MegaSteelBall.ProjectilePrefab;
        public static GameObject muzzleflashPrefab = ChargeTrioBomb.muzzleflashPrefab;
        public static float damageCoefficient = 1;
        public static float force = ChargeTrioBomb.force * 3;
        public static float selfForce = ChargeTrioBomb.force;

        private float prepDuration;
        private float timeBetweenPreps;
        private float barrageDuration;
        private float timeBetweenBarrages;
        private ChildLocator childLocator;
        private GameObject preppedBombPrefabInstance;
        private int currentBombIndex;
        private float perProjectileStopwatch;

        public override void OnEnter()
        {
            currentBombIndex = 1;
            base.OnEnter();
            prepDuration = basePrepDuration / attackSpeedStat;
            timeBetweenPreps = baseTimeBetweenPreps / attackSpeedStat;
            barrageDuration = baseBarrageDuration / attackSpeedStat;
            timeBetweenBarrages = baseTimeBetweenBarrages / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }
        }

        private string FindTargetChildStringFromBombIndex()
        {
            return string.Format(CultureInfo.InvariantCulture, "ProjectilePosition{0}", 2);
        }

        private Transform FindTargetChildTransformFromBombIndex()
        {
            string childName = FindTargetChildStringFromBombIndex();
            return childLocator.FindChild(childName);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            perProjectileStopwatch += Time.fixedDeltaTime;
            if (base.fixedAge < prepDuration)
            {
                if (perProjectileStopwatch > timeBetweenPreps && currentBombIndex < 2)
                {
                    currentBombIndex++;
                    perProjectileStopwatch = 0f;
                    Transform transform = FindTargetChildTransformFromBombIndex();
                    if ((bool)transform)
                    {
                        preppedBombPrefabInstance = Object.Instantiate(preppedBombPrefab, transform);
                    }
                }
            }
            else if (base.fixedAge < prepDuration + barrageDuration)
            {
                if (!(perProjectileStopwatch > timeBetweenBarrages) || currentBombIndex <= 1)
                {
                    return;
                }
                perProjectileStopwatch = 0f;
                Ray aimRay = GetAimRay();
                Transform transform2 = FindTargetChildTransformFromBombIndex();
                if ((bool)transform2)
                {
                    if (base.isAuthority)
                    {
                        ProjectileManager.instance.FireProjectile(bombProjectilePrefab, transform2.position, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
                        Rigidbody component = GetComponent<Rigidbody>();
                        if ((bool)component)
                        {
                            component.AddForceAtPosition((0f - selfForce) * transform2.forward, transform2.position);
                        }
                    }
                    EffectManager.SimpleMuzzleFlash(muzzleflashPrefab, base.gameObject, FindTargetChildStringFromBombIndex(), transmit: false);
                }
                currentBombIndex--;
                EntityState.Destroy(preppedBombPrefabInstance);
            }
            else if (base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            EntityState.Destroy(preppedBombPrefabInstance);
        }
    }
}