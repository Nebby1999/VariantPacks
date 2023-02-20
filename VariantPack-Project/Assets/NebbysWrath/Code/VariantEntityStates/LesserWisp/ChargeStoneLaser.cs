using EntityStates.GolemMonster;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Wisp1Monster.Stone
{
    public class ChargeStoneLaser : BaseState
    {
        public static float baseDuration = 3f;
        public static float laserMaxWidth = 0.2f;
        public static GameObject effectPrefab = ChargeLaser.effectPrefab;
        public static GameObject laserPrefab = ChargeLaser.laserPrefab;
        public static string attackSoundString = ChargeLaser.attackSoundString;

        private float duration;
        private uint chargePlayID;
        private GameObject chargeEffectInstance;
        private GameObject laserEffectInstance;
        private LineRenderer laserLineComponent;
        private Vector3 laserDirection;
        private Vector3 visualEndPosition;
        private float flashTimer;
        private bool laserOn;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            chargePlayID = Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform transform = component.FindChild("Muzzle");
                    if ((bool)transform)
                    {
                        if ((bool)effectPrefab)
                        {
                            chargeEffectInstance = UnityEngine.Object.Instantiate(effectPrefab, transform.position, transform.rotation);
                            chargeEffectInstance.transform.parent = transform;
                            ScaleParticleSystemDuration component2 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserPrefab)
                        {
                            laserEffectInstance = UnityEngine.Object.Instantiate(laserPrefab, transform.position, transform.rotation);
                            laserEffectInstance.transform.parent = transform;
                            laserLineComponent = laserEffectInstance.GetComponent<LineRenderer>();
                        }
                    }
                }
                PlayAnimation("Body", "ChargeAttack1", "ChargeAttack1.playbackRate", duration);
                if ((bool)base.characterBody)
                {
                    base.characterBody.SetAimTimer(duration);
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
            if ((bool)chargeEffectInstance)
            {
                EntityState.Destroy(chargeEffectInstance);
            }
            if ((bool)laserEffectInstance)
            {
                EntityState.Destroy(laserEffectInstance);
            }
        }

        public override void Update()
        {
            base.Update();
            if (!laserEffectInstance || !laserLineComponent)
            {
                return;
            }
            float num = 1000f;
            Ray aimRay = GetAimRay();
            Vector3 position = laserEffectInstance.transform.parent.position;
            Vector3 point = aimRay.GetPoint(num);
            laserDirection = point - position;
            if (Physics.Raycast(aimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask))
            {
                point = hitInfo.point;
            }
            laserLineComponent.SetPosition(0, position);
            laserLineComponent.SetPosition(1, point);
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
            laserLineComponent.startWidth = num2;
            laserLineComponent.endWidth = num2;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                FireStoneLaser fireLaser = new FireStoneLaser();
                fireLaser.laserDirection = laserDirection;
                outer.SetNextState(fireLaser);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}