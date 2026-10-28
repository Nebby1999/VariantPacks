using EntityStates.Commando.CommandoWeapon;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Vulture.Deagle
{
    public class FireVulturePistol : BaseSkillState
    {
        public static GameObject muzzleEffectPrefab;

        public static GameObject hitEffectPrefab;

        public static GameObject tracerEffectPrefab;

        public static float damageCoefficient;

        public static float force;

        public static float baseDuration = 2f;

        public static string firePistolSoundString;

        public static float recoilAmplitude = 1f;

        public static float spreadBloomValue = 0.3f;

        public static float commandoBoostBuffCoefficient = 0.4f;

        public static float trajectoryAimAssistMultiplier = 0.75f;

        private static bool init = false;

        private Ray aimRay;

        private float duration; 
        
        public override void OnEnter()
        {
            base.OnEnter();
            if(init == false)
            {
                muzzleEffectPrefab = FirePistol2.muzzleEffectPrefab;
                hitEffectPrefab = FirePistol2.hitEffectPrefab;
                tracerEffectPrefab = FirePistol2.tracerEffectPrefab;
                damageCoefficient = FirePistol2.damageCoefficient;
                force = FirePistol2.force;
                baseDuration = FirePistol2.baseDuration;
                firePistolSoundString = FirePistol2.firePistolSoundString;
                recoilAmplitude = FirePistol2.recoilAmplitude;
                spreadBloomValue = FirePistol2.spreadBloomValue;
                commandoBoostBuffCoefficient = FirePistol2.commandoBoostBuffCoefficient;
                trajectoryAimAssistMultiplier = FirePistol2.trajectoryAimAssistMultiplier;
                init = true;
            }

            duration = baseDuration / attackSpeedStat;
            aimRay = GetAimRay();
            StartAimMode(aimRay, 3f);
            FireBullet("MouthMuzzle");
        }

        private void FireBullet(string targetMuzzle)
        {
            Util.PlaySound(firePistolSoundString, base.gameObject);
            if ((bool)muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, targetMuzzle, transmit: false);
            }

            AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
                bulletAttack.damage = damageCoefficient * damageStat;
                bulletAttack.force = force;
                bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
                bulletAttack.muzzleName = targetMuzzle;
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.radius = 0.1f;
                bulletAttack.smartCollision = true;
                bulletAttack.trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier;
                bulletAttack.damageType = DamageTypeCombo.GenericPrimary;
                bulletAttack.Fire();
            }
            base.characterBody.AddSpreadBloom(spreadBloomValue);
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