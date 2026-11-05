// RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EntityStates.MinorConstruct.Weapon.FireConstructBeam

using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.MinorConstruct.Weapon.Theta
{

    public class FireThetaLaser : GenericProjectileBaseState
    {
        [SerializeField]
        public string animationLayerName;

        [SerializeField]
        public string animationStateName;

        private static FireConstructBeam _origStatic;

        private void InitFromOrig()
        {
            _origStatic ??= new FireConstructBeam();

            effectPrefab = _origStatic.effectPrefab;
            projectilePrefab = NW.PrefabClones.OmicronProjectile.omicronProjectile;
            damageCoefficient = _origStatic.damageCoefficient;
            force = _origStatic.force;
            minSpread = _origStatic.minSpread;
            maxSpread = _origStatic.maxSpread;
            baseDuration = _origStatic.baseDuration;
            recoilAmplitude = _origStatic.recoilAmplitude;
            attackSoundString = _origStatic.attackSoundString;
            projectilePitchBonus = _origStatic.projectilePitchBonus;
            baseDelayBeforeFiringProjectile = _origStatic.baseDelayBeforeFiringProjectile;
            targetMuzzle = _origStatic.targetMuzzle;
            bloom = _origStatic.bloom;
            animationLayerName = _origStatic.animationLayerName;
            animationStateName = _origStatic.animationStateName;
        }

        public override void FireProjectile()
        {
            if (!isAuthority)
                return;

            Ray aimRay = aimRayOverride ?? GetAimRay();
            aimRay = ModifyProjectileAimRay(aimRay);
            aimRay.direction = Util.ApplySpread(aimRay.direction, minSpread, maxSpread, 1f, 1f, 0f, projectilePitchBonus);

            Ray secondRay = aimRay;
            Quaternion rotatedLeft = Quaternion.Euler(0, 45, 0);
            Quaternion rotatedRight = Quaternion.Euler(0, -45, 0);
            Quaternion pickedRotation = (Random.value > 0.5f) ? rotatedLeft : rotatedRight;
            secondRay.direction = pickedRotation * secondRay.direction;

            bool isCrit = RollCrit();
            FireProjectileInfo info = new FireProjectileInfo
            {
                projectilePrefab = projectilePrefab,
                position = aimRay.origin,
                rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                owner = gameObject,
                damage = damageStat * damageCoefficient,
                force = force,
                crit = isCrit,
                fuseOverride = projectileFuseOverride ?? -1,
                speedOverride = projectileSpeedOverride ?? -1
            };

            ModifyProjectileInfo(ref info);
            ProjectileManager.instance.FireProjectile(info);

            info.rotation = Util.QuaternionSafeLookRotation(secondRay.direction);
            ModifyProjectileInfo(ref info);
            ProjectileManager.instance.FireProjectile(info);
        }

        public override void OnEnter()
        {
            InitFromOrig();
            base.OnEnter();
            PlayAnimation(animationLayerName, animationStateName);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

}