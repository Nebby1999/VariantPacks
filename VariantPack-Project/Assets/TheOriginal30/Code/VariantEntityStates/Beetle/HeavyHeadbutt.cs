using RoR2;
using System;
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

            Util.PlaySound(HeadbuttState.attackSoundString, gameObject);
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
                attack.forceVector = (characterDirection ? (characterDirection.forward * pushForce) : Vector3.zero);

                if (characterDirection && inputBank)
                {
                    characterDirection.moveVector = inputBank.aimDirection;
                }

                if (modelAnimator && modelAnimator.GetFloat("Headbutt.hitBoxActive") > 0.5f)
                {
                    attack.Fire(null);
                }
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
