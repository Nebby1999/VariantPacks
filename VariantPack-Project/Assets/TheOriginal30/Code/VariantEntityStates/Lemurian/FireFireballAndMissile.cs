using EntityStates.LemurianBruiserMonster;
using RoR2;
using RoR2.Projectile;
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
            Util.PlaySound(attackString, gameObject);
            Ray aimRay = GetAimRay();
            string muzzleName = "MuzzleMouth";
            string missileMuzzleString = "Chest";
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, muzzleName, transmit: false);
                EffectManager.SimpleMuzzleFlash(FireMegaFireball.muzzleflashEffectPrefab, gameObject, missileMuzzleString, false);
            }
            if (isAuthority)
            {
                ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, characterBody.master));
                ProjectileManager.instance.FireProjectile(missleProjectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(Vector3.up), gameObject, missileDamageCoef * damageStat, 0f, RollCrit(), DamageColorIndex.Default, null, -1f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
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