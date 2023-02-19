using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ClaymanMonster.Assasin
{
    public class CloakingSwipe : BaseState
    {
        public static float baseDuration = 1.1f;
        public static float damageCoefficient = 1.4f;
        public static float forceMagnitude = 16f;
        public static float selfForceMagnitude = 1800;
        public static float radius = 3f;
        public static GameObject hitEffectPrefab = SwipeForward.hitEffectPrefab;
        public static GameObject swingEffectPrefab = SwipeForward.swingEffectPrefab;
        public static string attackString = "Play_merc_sword_swing";

        private OverlapAttack attack;
        private Animator modelAnimator;
        private float duration;
        private bool hasSlashed;

        public override void OnEnter()
        {
            base.OnEnter();
            if(NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
                characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
            }
            duration = baseDuration / attackSpeedStat;
            modelAnimator = GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            attack = new OverlapAttack();
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
            attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
            attack.damage = (attack.isCrit ? damageCoefficient/ 10 : damageCoefficient) * damageStat;
            attack.damageType = attack.isCrit ? DamageType.BleedOnHit : DamageType.SuperBleedOnCrit;
            attack.hitEffectPrefab = hitEffectPrefab;
            Util.PlaySound(attackString, base.gameObject);
            if ((bool)modelTransform)
            {
                attack.hitBoxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Sword");
            }
            if ((bool)modelAnimator)
            {
                PlayAnimation("Gesture, Override", "SwipeForward", "SwipeForward.playbackRate", duration);
                PlayAnimation("Gesture, Additive", "SwipeForward", "SwipeForward.playbackRate", duration);
            }
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && (bool)modelAnimator && modelAnimator.GetFloat("SwipeForward.hitBoxActive") > 0.1f)
            {
                if (!hasSlashed)
                {
                    EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "SwingCenter", transmit: true);
                    HealthComponent healthComponent = base.characterBody.healthComponent;
                    CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();
                    if ((bool)healthComponent)
                    {
                        healthComponent.TakeDamageForce(selfForceMagnitude * component.forward, alwaysApply: true);
                    }
                    if (NetworkServer.active)
                    {
                        characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                        characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
                    }
                    hasSlashed = true;
                }
                attack.forceVector = base.transform.forward * forceMagnitude;
                attack.Fire();
            }
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}