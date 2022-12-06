using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.BeetleMonster.Battle
{
    public class HeavyHeadbutt : BaseState
    {
        public static float pushForce = 8000;

        private OverlapAttack attack;
        private Animator modelAnimator;
        private RootMotionAccumulator rootMotionAccumulator;
        private float duration;

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
                this.attack.forceVector = (base.characterDirection ? (base.characterDirection.forward * HeavyHeadbutt.pushForce) : Vector3.zero);

                if (base.characterDirection && base.inputBank)
                {
                    base.characterDirection.moveVector = base.inputBank.aimDirection;
                }

                if (this.modelAnimator && this.modelAnimator.GetFloat("Headbutt.hitBoxActive") > 0.5f)
                {
                    this.attack.Fire(null);
                }
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
