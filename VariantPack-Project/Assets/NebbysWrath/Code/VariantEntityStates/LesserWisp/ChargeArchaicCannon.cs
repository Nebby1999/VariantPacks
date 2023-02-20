
using EntityStates.ArchWispMonster;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.Wisp1Monster.Archaic
{
    public class ChargeArchaicCannon : BaseState
    {
        public static GameObject chargeEffectPrefab;
        public static GameObject laserEffectPrefab = ChargeEmbers.laserEffectPrefab;
        public static float baseDuration = 3f;

        private GameObject chargeEffectInstance;

        private GameObject laserEffectInstance;
        private LineRenderer laserEffectInstanceLineRenderer;

        protected float duration;
        private string attackString;
        private float stopwatch;

        public override void OnEnter()
        {
            var goodState = new ChargeCannons();
            chargeEffectPrefab = goodState.effectPrefab;
            attackString = goodState.attackString;
            base.OnEnter();
            Util.PlayAttackSpeedSound(attackString, gameObject, attackSpeedStat * (2f / baseDuration));
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();

            if (modelTransform)
            {
                ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    Transform muzzleTransform = childLocator.FindChild("Muzzle");
                    if (muzzleTransform)
                    {
                        if ((bool)chargeEffectPrefab)
                        {
                            chargeEffectInstance = UnityEngine.Object.Instantiate(chargeEffectPrefab, transform.position, transform.rotation);
                            chargeEffectInstance.transform.parent = transform;
                            ScaleParticleSystemDuration component2 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserEffectPrefab)
                        {
                            laserEffectInstance = UnityEngine.Object.Instantiate(laserEffectPrefab, transform.position, transform.rotation);
                            laserEffectInstance.transform.parent = transform;
                            laserEffectInstanceLineRenderer = laserEffectInstance.GetComponent<LineRenderer>();
                        }
                    }
                }
            }

            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(duration);
            }
            base.PlayAnimation("Body", "ChargeAttack1", "ChargeAttack1.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)chargeEffectPrefab)
            {
                EntityState.Destroy(chargeEffectInstance);
            }
            if ((bool)laserEffectPrefab)
            {
                EntityState.Destroy(laserEffectInstance);
            }
        }

        public override void Update()
        {
            base.Update();
            Ray aimRay = GetAimRay();
            float distance = 50f;
            Vector3 origin = aimRay.origin;
            Vector3 point = aimRay.GetPoint(distance);
            laserEffectInstanceLineRenderer.SetPosition(0, origin);
            laserEffectInstanceLineRenderer.SetPosition(1, point);
            Color startColor = new Color(255f, 191f, 237f, stopwatch / duration);
            Color clear = Color.clear;
            laserEffectInstanceLineRenderer.startColor = startColor;
            laserEffectInstanceLineRenderer.endColor = clear;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                FireArchaicCannon nextState = new FireArchaicCannon();
                outer.SetNextState(nextState);
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}