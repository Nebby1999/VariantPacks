using EntityStates;
using EntityStates.GolemMonster;
using RoR2;
using System;
using UnityEngine;

namespace EntityStates.GreaterWispMonster.Stone
{
    public class ChargeDoubleLaser : BaseState
    {
        public static float baseDuration = ChargeLaser.baseDuration;
        public static float laserMaxWidth = ChargeLaser.laserMaxWidth;
        public static GameObject effectPrefab = ChargeLaser.effectPrefab;
        public static GameObject laserPrefab = ChargeLaser.laserPrefab;
        public static string attackSoundString = ChargeLaser.attackSoundString;

        private float duration;
        private uint chargePlayID;
        private GameObject leftChargeEffectInstance;
        private GameObject leftLaserEffectInstance;
        private LineRenderer leftLaserLineComponent;
        private Vector3 leftLaserDirection;
        private GameObject rightChargeEffectInstance;
        private GameObject rightLaserEffectInstance;
        private LineRenderer rightLaserLineComponent;
        private Vector3 rightLaserDirection;
        private Vector3 visualEndPosition;
        private float flashTimer;
        private bool laserOn;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            GetModelAnimator();
            PlayAnimation("Gesture", "ChargeCannons", "ChargeCannons.playbackRate", duration);
            chargePlayID = Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform leftTransform = component.FindChild("MuzzleLeft");
                    Transform rightTransform = component.FindChild("MuzzleRight");
                    //Left Hand
                    if ((bool)leftTransform)
                    {
                        if ((bool)effectPrefab)
                        {
                            leftChargeEffectInstance = UnityEngine.Object.Instantiate(effectPrefab, leftTransform.position, leftTransform.rotation);
                            leftChargeEffectInstance.transform.parent = leftTransform;
                            ScaleParticleSystemDuration component2 = leftChargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserPrefab)
                        {
                            leftLaserEffectInstance = UnityEngine.Object.Instantiate(laserPrefab, leftTransform.position, leftTransform.rotation);
                            leftLaserEffectInstance.transform.parent = leftTransform;
                            leftLaserLineComponent = leftLaserEffectInstance.GetComponent<LineRenderer>();
                        }
                    }
                    //Right Hand
                    if ((bool)rightTransform)
                    {
                        if ((bool)effectPrefab)
                        {
                            rightChargeEffectInstance = UnityEngine.Object.Instantiate(effectPrefab, rightTransform.position, rightTransform.rotation);
                            rightChargeEffectInstance.transform.parent = rightTransform;
                            ScaleParticleSystemDuration component2 = rightChargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserPrefab)
                        {
                            rightLaserEffectInstance = UnityEngine.Object.Instantiate(laserPrefab, rightTransform.position, rightTransform.rotation);
                            rightLaserEffectInstance.transform.parent = rightTransform;
                            rightLaserLineComponent = rightLaserEffectInstance.GetComponent<LineRenderer>();
                        }
                    }
                }
            }
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(duration);
            }
            flashTimer = 0f;
            laserOn = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)leftChargeEffectInstance)
            {
                EntityState.Destroy(leftChargeEffectInstance);
            }
            if ((bool)leftLaserEffectInstance)
            {
                EntityState.Destroy(leftLaserEffectInstance);
            }
            if ((bool)rightChargeEffectInstance)
            {
                EntityState.Destroy(rightChargeEffectInstance);
            }
            if ((bool)rightLaserEffectInstance)
            {
                EntityState.Destroy(rightLaserEffectInstance);
            }
        }

        public override void Update()
        {
            base.Update();
            if (!(leftLaserEffectInstance && rightLaserEffectInstance) || !(rightLaserLineComponent && leftLaserLineComponent))
            {
                return;
            }
            //Left Laser
            {
                float num = 1000f;
                Ray leftAimRay = GetAimRay();
                Vector3 leftPosition = leftLaserEffectInstance.transform.parent.position;
                Vector3 leftPoint = leftAimRay.GetPoint(num);
                leftLaserDirection = leftPoint - leftPosition;
                if (Physics.Raycast(leftAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask))
                {
                    leftPoint = hitInfo.point;
                }
                leftLaserLineComponent.SetPosition(0, leftPosition);
                leftLaserLineComponent.SetPosition(1, leftPoint);
                float num2;
                if (duration - base.age > 0.5f)
                {
                    num2 = base.age / duration;
                }
                else
                {
                    flashTimer -= Time.deltaTime;
                    if (flashTimer <= 0f)
                    {
                        laserOn = !laserOn;
                        flashTimer = 71f / (678f * (float)Math.PI);
                    }
                    num2 = (laserOn ? 1f : 0f);
                }
                num2 *= laserMaxWidth;
                leftLaserLineComponent.startWidth = num2;
                leftLaserLineComponent.endWidth = num2;
            }
            //Right Laser
            {
                float num = 1000f;
                Ray rightAimRay = GetAimRay();
                Vector3 rightPosition = rightLaserEffectInstance.transform.parent.position;
                Vector3 rightPoint = rightAimRay.GetPoint(num);
                rightLaserDirection = rightPoint - rightPosition;
                if (Physics.Raycast(rightAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask))
                {
                    rightPoint = hitInfo.point;
                }
                rightLaserLineComponent.SetPosition(0, rightPosition);
                rightLaserLineComponent.SetPosition(1, rightPoint);
                float num2;
                if (duration - base.age > 0.5f)
                {
                    num2 = base.age / duration;
                }
                else
                {
                    flashTimer -= Time.deltaTime;
                    if (flashTimer <= 0f)
                    {
                        laserOn = !laserOn;
                        flashTimer = 71f / (678f * (float)Math.PI);
                    }
                    num2 = (laserOn ? 1f : 0f);
                }
                num2 *= laserMaxWidth;
                rightLaserLineComponent.startWidth = num2;
                rightLaserLineComponent.endWidth = num2;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                FireDoubleStoneLaser fireDoubleLaser = new FireDoubleStoneLaser();
                fireDoubleLaser.leftLaserDirection = leftLaserDirection;
                fireDoubleLaser.rightLaserDirection = rightLaserDirection;
                outer.SetNextState(fireDoubleLaser);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
