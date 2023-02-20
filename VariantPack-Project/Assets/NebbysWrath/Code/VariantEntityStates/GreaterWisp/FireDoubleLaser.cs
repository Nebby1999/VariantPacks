using EntityStates;
using EntityStates.GolemMonster;
using RoR2;
using UnityEngine;

namespace EntityStates.GreaterWispMonster.Stone
{
    public class FireDoubleStoneLaser : BaseState
    {
        public static GameObject effectPrefab = FireLaser.effectPrefab;
        public static GameObject hitEffectPrefab = FireLaser.hitEffectPrefab;
        public static GameObject tracerEffectPrefab = FireLaser.tracerEffectPrefab;
        public static float damageCoefficient = FireLaser.damageCoefficient;
        public static float blastRadius = FireLaser.blastRadius;
        public static float force = FireLaser.force;
        public static float minSpread = FireLaser.minSpread;
        public static float maxSpread = FireLaser.maxSpread;
        public static int bulletCount = FireLaser.bulletCount;
        public static float baseDuration = FireLaser.baseDuration;
        public static string attackSoundString = FireLaser.attackSoundString;

        public Vector3 leftLaserDirection;
        private Ray leftModifiedAimRay;
        public Vector3 rightLaserDirection;
        private Ray rightModifiedAimRay;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            leftModifiedAimRay = GetAimRay();
            leftModifiedAimRay.direction = leftLaserDirection;
            rightModifiedAimRay = GetAimRay();
            rightModifiedAimRay.direction = leftLaserDirection;
            GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlaySound(attackSoundString, base.gameObject);
            string leftHandMuzzle = "MuzzleLeft";
            string rightHandMuzzle = "MuzzleRight";
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
            PlayAnimation("Gesture", "FireCannons", "FireCannons.playbackRate", duration);
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, leftHandMuzzle, transmit: false);
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, rightHandMuzzle, transmit: false);
            }
            if (!base.isAuthority)
            {
                return;
            }
            float num = 1000f;
            //Left Hand
            {
                Vector3 leftVector = leftModifiedAimRay.origin + leftModifiedAimRay.direction * num;
                if (Physics.Raycast(leftModifiedAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask | (int)LayerIndex.entityPrecise.mask))
                {
                    leftVector = hitInfo.point;
                }
                BlastAttack leftBlastAttack = new BlastAttack();
                leftBlastAttack.attacker = base.gameObject;
                leftBlastAttack.inflictor = base.gameObject;
                leftBlastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                leftBlastAttack.baseDamage = damageStat * damageCoefficient;
                leftBlastAttack.baseForce = force * 0.2f;
                leftBlastAttack.position = leftVector;
                leftBlastAttack.radius = blastRadius;
                leftBlastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                leftBlastAttack.bonusForce = force * leftModifiedAimRay.direction;
                leftBlastAttack.Fire();
                _ = leftModifiedAimRay.origin;
                if (!modelTransform)
                {
                    return;
                }
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    int childIndex = component.FindChildIndex(leftHandMuzzle);
                    if ((bool)tracerEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = leftVector,
                            start = leftModifiedAimRay.origin
                        };
                        effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
                        EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                        EffectManager.SpawnEffect(hitEffectPrefab, effectData, transmit: true);
                    }
                }
            }
            //Right Hand
            {
                Vector3 rightVector = rightModifiedAimRay.origin + rightModifiedAimRay.direction * num;
                if (Physics.Raycast(rightModifiedAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask | (int)LayerIndex.entityPrecise.mask))
                {
                    rightVector = hitInfo.point;
                }
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = base.gameObject;
                blastAttack.inflictor = base.gameObject;
                blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                blastAttack.baseDamage = damageStat * damageCoefficient;
                blastAttack.baseForce = force * 0.2f;
                blastAttack.position = rightVector;
                blastAttack.radius = blastRadius;
                blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
                blastAttack.bonusForce = force * rightModifiedAimRay.direction;
                blastAttack.Fire();
                _ = rightModifiedAimRay.origin;
                if (!modelTransform)
                {
                    return;
                }
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    int childIndex = component.FindChildIndex(rightHandMuzzle);
                    if ((bool)tracerEffectPrefab)
                    {
                        EffectData effectData = new EffectData
                        {
                            origin = rightVector,
                            start = rightModifiedAimRay.origin
                        };
                        effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
                        EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                        EffectManager.SpawnEffect(hitEffectPrefab, effectData, transmit: true);
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
