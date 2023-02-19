using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EntityStates.LemurianMonster;
using RoR2;
using RoR2.Projectile;

namespace EntityStates.LemurianBruiserMonster.Incinerating
{
    public class FireSingleFireball : BaseState
    {
        public static GameObject projectilePrefab = FireFireball.projectilePrefab;
        public static GameObject muzzleflashEffectPrefab = FireMegaFireball.muzzleflashEffectPrefab;
        public static int projectileCount = 1;
        public static float totalYawSpread = 5f;
        public static float baseDuration = FireFireball.baseDuration;
        public static float baseFireDuration = FireMegaFireball.baseFireDuration;
        public static float damageCoefficient = FireFireball.damageCoefficient;
        public static float projectileSpeed = FireMegaFireball.projectileSpeed;
        public static float force = FireFireball.force;
        public static string attackString = FireMegaFireball.attackString;

        private float duration;
        private float fireDuration;
        private int projectilesFired;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDuration = baseFireDuration / attackSpeedStat;
            PlayAnimation("Gesture, Additive", "FireMegaFireball", "FireMegaFireball.playbackRate", duration);
            Util.PlaySound(attackString, base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            string muzzleName = "MuzzleMouth";
            if (base.isAuthority)
            {
                int num = Mathf.FloorToInt(base.fixedAge / fireDuration * (float)projectileCount);
                if (projectilesFired <= num && projectilesFired < projectileCount)
                {
                    if ((bool)muzzleflashEffectPrefab)
                    {
                        EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleName, transmit: false);
                    }
                    Ray aimRay = GetAimRay();
                    float speedOverride = projectileSpeed;
                    float bonusYaw = (float)Mathf.FloorToInt((float)projectilesFired - (float)(3 - 1) / 2f) / (float)(3 - 1) * totalYawSpread;
                    Vector3 forward = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, bonusYaw);
                    ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speedOverride);
                    projectilesFired++;
                }
            }
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
