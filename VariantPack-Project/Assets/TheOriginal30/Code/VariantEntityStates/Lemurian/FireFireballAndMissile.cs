using EntityStates.LemurianBruiserMonster;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LemurianMonster.Badass
{
    public class FireFireballAndMissile : BaseState
    {
        public static GameObject projectilePrefab = FireFireball.projectilePrefab;

        public static GameObject effectPrefab = FireFireball.effectPrefab;

        public static float baseDuration = FireFireball.baseDuration;

        public static float damageCoefficient = FireFireball.damageCoefficient;

        public static float force = FireFireball.force;

        public static string attackString = FireFireball.attackString;

        public static GameObject missleProjectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/MissileProjectile");

        public static float missileDamageCoef = 1.2f;
        
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            PlayAnimation("Gesture", "FireFireball", "FireFireball.playbackRate", duration);
            Util.PlaySound(attackString, base.gameObject);
            Ray aimRay = GetAimRay();
            string muzzleName = "MuzzleMouth";
            string missileMuzzleString = "Chest";
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
                EffectManager.SimpleMuzzleFlash(FireMegaFireball.muzzleflashEffectPrefab, base.gameObject, missileMuzzleString, false);
            }
            if (base.isAuthority)
            {
                ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
                ProjectileManager.instance.FireProjectile(missleProjectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(Vector3.up), base.gameObject, missileDamageCoef * this.damageStat, 0f, base.RollCrit(), DamageColorIndex.Default, null, -1f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
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