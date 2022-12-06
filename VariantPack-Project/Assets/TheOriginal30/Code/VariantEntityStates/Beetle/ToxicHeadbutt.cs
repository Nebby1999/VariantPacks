using EntityStates;
using EntityStates.BeetleMonster;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.BeetleMonster.Toxic
{
    public class ToxicHeadbutt : BaseState
    {
        public static float explosionRadius = 2;
        public static float damageCoefficent = 0.8f;
        public static GameObject crocoEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoLeapExplosion.prefab").WaitForCompletion();

        private OverlapAttack attack;
        private Animator modelAnimator;
        private RootMotionAccumulator rootMotionAccumulator;
        private float duration;
        private bool hasExploded;

        public override void OnEnter()
        {
            base.OnEnter();
            this.rootMotionAccumulator = base.GetModelRootMotionAccumulator();
            this.modelAnimator = base.GetModelAnimator();
            this.duration = HeadbuttState.baseDuration / this.attackSpeedStat;

            this.attack = new OverlapAttack();
            this.attack.attacker = base.gameObject;
            this.attack.inflictor = base.gameObject;
            this.attack.teamIndex = TeamComponent.GetObjectTeam(this.attack.attacker);
            this.attack.damage = HeadbuttState.damageCoefficient * this.damageStat;
            this.attack.hitEffectPrefab = HeadbuttState.hitEffectPrefab;

            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                this.attack.hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Headbutt");
            }

            Util.PlaySound(HeadbuttState.attackSoundString, base.gameObject);
            base.PlayCrossfade("Body", "Headbutt", "Headbutt.playbackRate", this.duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.rootMotionAccumulator)
            {
                Vector3 vector = this.rootMotionAccumulator.ExtractRootMotion();
                if (vector != Vector3.zero && base.isAuthority && base.characterMotor)
                {
                    base.characterMotor.rootMotion += vector;
                }
            }

            if (base.isAuthority)
            {
                this.attack.forceVector = (base.characterDirection ? (base.characterDirection.forward * HeadbuttState.forceMagnitude) : Vector3.zero);

                if (base.characterDirection && base.inputBank)
                {
                    base.characterDirection.moveVector = base.inputBank.aimDirection;
                }

                if (this.modelAnimator && this.modelAnimator.GetFloat("Headbutt.hitBoxActive") > 0.5f)
                {
                    this.Detonate();
                    return;
                }
            }
        }

        private void Detonate()
        {
            if (this.hasExploded) return;
            this.hasExploded = true;

            if (base.modelLocator)
            {
                if (base.modelLocator.modelBaseTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelBaseTransform.gameObject);
                }
                if (base.modelLocator.modelTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelTransform.gameObject);
                }
            }

            new BlastAttack
            {
                attacker = base.gameObject,
                inflictor = base.gameObject,
                teamIndex = TeamComponent.GetObjectTeam(base.gameObject),
                baseDamage = this.damageStat * ToxicHeadbutt.damageCoefficent,
                baseForce = 0f,
                position = base.transform.position,
                radius = ToxicHeadbutt.explosionRadius,
                procCoefficient = 2f,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.PoisonOnHit
            }.Fire();

            Vector3 footPosition = base.characterBody.footPosition;
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = EntityStates.Croco.BaseLeap.projectilePrefab,
                crit = base.RollCrit(),
                force = 0f,
                damage = this.damageStat,
                owner = base.gameObject,
                rotation = Quaternion.identity,
                position = footPosition
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);

            EffectManager.SpawnEffect(crocoEffect, new EffectData
            {
                origin = footPosition,
                scale = ToxicHeadbutt.explosionRadius
            }, true);

            if (base.healthComponent) base.healthComponent.Suicide(null, null, DamageType.Generic);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}