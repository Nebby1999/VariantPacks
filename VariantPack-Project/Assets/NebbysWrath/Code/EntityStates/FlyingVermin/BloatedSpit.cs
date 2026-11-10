using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.FlyingVermin.Bloated
{
    public class BloatedSpit : GenericProjectileBaseState
    {
        private static int SpitStateHash = Animator.StringToHash("Spit");

        private static int SpitParamHash = Animator.StringToHash("Spit.playbackRate");

        private static GameObject _projectilePrefab = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_BeetleQueen.BeetleQueenSpit_prefab).WaitForCompletion();
        private static Weapon.Spit _origStatic;
        private void InitFromOrig()
        {
            _origStatic ??= new Weapon.Spit();

            effectPrefab = _origStatic.effectPrefab;
            projectilePrefab = _projectilePrefab;
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
        }
        public override void OnEnter()
        {
            InitFromOrig();

            base.OnEnter();
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(5f);
            }
        }

        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            PlayAnimation("Gesture, Additive", SpitStateHash, SpitParamHash, duration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void FireProjectile()
        {
            if (base.isAuthority)
            {
                Ray baseRay = GetAimRay();
                if(Util.CharacterRaycast(gameObject, baseRay, out RaycastHit hitInfo, float.PositiveInfinity, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
                {
                    float magnitude = projectilePrefab.GetComponent<ProjectileSimple>().desiredForwardSpeed;
                    float num = magnitude;
                    Vector3 vector = hitInfo.point - baseRay.origin;
                    Vector2 vector2 = new Vector2(vector.x, vector.z);
                    float magnitude2 = vector2.magnitude;
                    float y = Trajectory.CalculateInitialYSpeed(magnitude2 / num, vector.y);
                    Vector3 vector3 = new Vector3(vector2.x / magnitude2 * num, y, vector2.y / magnitude2 * num);
                    magnitude = vector3.magnitude;
                    baseRay.direction = vector3 / magnitude;
                }
                Ray[] rayArray = new Ray[3]
                {
                    baseRay,
                    baseRay,
                    baseRay
                };
                rayArray[1].direction = Quaternion.Euler(0, 22.5f, 0) * rayArray[1].direction;
                rayArray[2].direction = Quaternion.Euler(0, -22.5f, 0) * rayArray[2].direction;

                DamageTypeCombo? overrideCombo = null;
                if(projectilePrefab.TryGetComponent<ProjectileDamage>(out var prjDamage))
                {
                    var combo = prjDamage.damageType;
                    combo.damageSource |= DamageSource.Primary;
                    overrideCombo = combo;
                }
                for(int i = 0; i < rayArray.Length; i++)
                {
                    var ray = rayArray[i];
                    ray.direction = Util.ApplySpread(ray.direction, minSpread, maxSpread, 1f, 1f, 0f, projectilePitchBonus);
                    float damage = damageStat * damageCoefficient;

                    ProjectileManager.instance.FireProjectile(_projectilePrefab, ray.origin, Util.QuaternionSafeLookRotation(ray.direction), base.gameObject, damage, 0, Util.CheckRoll(critStat, base.characterBody.master), damageType: overrideCombo);
                }
            }
        }
    }
}