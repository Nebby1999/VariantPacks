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
            rootMotionAccumulator = GetModelRootMotionAccumulator();
            modelAnimator = GetModelAnimator();
            duration = HeadbuttState.baseDuration / attackSpeedStat;

            attack = new OverlapAttack();
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
            attack.damage = HeadbuttState.damageCoefficient * damageStat;
            attack.hitEffectPrefab = HeadbuttState.hitEffectPrefab;

            Transform modelTransform = GetModelTransform();
            if (modelTransform)
            {
                attack.hitBoxGroup = Array.Find<HitBoxGroup>(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Headbutt");
            }

            if(sfxLocator)
            {
                Util.PlaySound(sfxLocator.deathSound, gameObject);
            }
            PlayCrossfade("Body", "Headbutt", "Headbutt.playbackRate", duration, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (rootMotionAccumulator)
            {
                Vector3 vector = rootMotionAccumulator.ExtractRootMotion();
                if (vector != Vector3.zero && isAuthority && characterMotor)
                {
                    characterMotor.rootMotion += vector;
                }
            }

            if (isAuthority)
            {
                attack.forceVector = (characterDirection ? (characterDirection.forward * HeadbuttState.forceMagnitude) : Vector3.zero);

                if (characterDirection && inputBank)
                {
                    characterDirection.moveVector = inputBank.aimDirection;
                }

                if (modelAnimator && modelAnimator.GetFloat("Headbutt.hitBoxActive") > 0.5f)
                {
                    Detonate();
                    return;
                }
            }
        }

        private void Detonate()
        {
            if (hasExploded) return;
            hasExploded = true;

            if (modelLocator)
            {
                if (modelLocator.modelBaseTransform)
                {
                    Destroy(modelLocator.modelBaseTransform.gameObject);
                }
                if (modelLocator.modelTransform)
                {
                    Destroy(modelLocator.modelTransform.gameObject);
                }
            }

            new BlastAttack
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                baseDamage = damageStat * damageCoefficent,
                baseForce = 0f,
                position = transform.position,
                radius = explosionRadius,
                procCoefficient = 2f,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.PoisonOnHit
            }.Fire();

            Vector3 footPosition = characterBody.footPosition;
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = Croco.BaseLeap.projectilePrefab,
                crit = RollCrit(),
                force = 0f,
                damage = damageStat,
                owner = gameObject,
                rotation = Quaternion.identity,
                position = footPosition
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);

            EffectManager.SpawnEffect(crocoEffect, new EffectData
            {
                origin = footPosition,
                scale = explosionRadius
            }, true);

            if(Croco.BaseLeap.landingSound)
            {
                EffectManager.SimpleSoundEffect(Croco.BaseLeap.landingSound.index, characterBody.corePosition, transmit: true);
            }


            if (healthComponent) healthComponent.Suicide(null, null, DamageType.Generic);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}