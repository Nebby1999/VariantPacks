using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ImpBossMonster.Ichor
{
    public class FireIchorSpikes : BaseState
    {
        public static float baseDuration = FireVoidspikes.baseDuration;
        public static float damageCoefficient = FireVoidspikes.damageCoefficient;
        public static float procCoefficient = FireVoidspikes.procCoefficient;
        public static float selfForce = FireVoidspikes.selfForce;
        public static float forceMagnitude = FireVoidspikes.forceMagnitude;
        public static GameObject hitEffectPrefab = FireVoidspikes.hitEffectPrefab;
        public static GameObject swipeEffectPrefab = FireVoidspikes.swipeEffectPrefab;
        public static string enterSoundString = FireVoidspikes.enterSoundString;
        public static string attackSoundString = FireVoidspikes.attackSoundString;
        public static float walkSpeedPenaltyCoefficient = FireVoidspikes.walkSpeedPenaltyCoefficient;
        public static int projectileCount = FireVoidspikes.projectileCount;
        public static float projectileYawSpread = FireVoidspikes.projectileYawSpread;
        public static float projectileDamageCoefficient = FireVoidspikes.projectileDamageCoefficient;
        public static float projectileSpeed = FireVoidspikes.projectileSpeed;
        public static float projectileSpeedPerProjectile = FireVoidspikes.projectileSpeedPerProjectile;
        public static GameObject projectilePrefab = NW.PrefabClones.IchorSpike.ichorSpike;

        private OverlapAttack attack;
        private Animator modelAnimator;
        private float duration;
        private int slashCount;
        private Transform modelTransform;
        private int chosenAnim = -1;

        public override void OnEnter()
        {
            hitEffectPrefab = FireVoidspikes.hitEffectPrefab;
            swipeEffectPrefab = FireVoidspikes.swipeEffectPrefab;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            modelAnimator = GetModelAnimator();
            modelTransform = GetModelTransform();
            base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedPenaltyCoefficient;
            attack = new OverlapAttack();
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = damageCoefficient * damageStat;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
            attack.procCoefficient = procCoefficient;
            attack.damageType = DamageType.Generic;

            DamageAPI.AddModdedDamageType(attack, NW.DamageTypes.PulverizeOnHit.pulverizeOnHit);

            Util.PlaySound(enterSoundString, base.gameObject);
            if (base.isAuthority)
            {
                chosenAnim = ((!Util.CheckRoll(50f)) ? 1 : 0);
            }
            if ((bool)modelAnimator)
            {
                string animationStateName = ((chosenAnim == 1) ? "FireVoidspikesL" : "FireVoidspikesR");
                PlayAnimation("Gesture, Additive", animationStateName, "FireVoidspikes.playbackRate", duration);
                PlayAnimation("Gesture, Override", animationStateName, "FireVoidspikes.playbackRate", duration);
            }
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(duration + 3f);
            }
        }

        public override void OnExit()
        {
            if ((bool)base.characterMotor)
            {
                base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((bool)modelAnimator && slashCount <= 0)
            {
                if (modelAnimator.GetFloat("HandR.hitBoxActive") > 0.1f)
                {
                    FireSpikeFan(GetAimRay(), "FireVoidspikesR", "HandR");
                }
                if (modelAnimator.GetFloat("HandL.hitBoxActive") > 0.1f)
                {
                    FireSpikeFan(GetAimRay(), "FireVoidspikesL", "HandL");
                }
            }
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void FireSpikeFan(Ray aimRay, string muzzleName, string hitBoxGroupName)
        {
            Util.PlaySound(attackSoundString, base.gameObject);
            EffectManager.SimpleMuzzleFlash(swipeEffectPrefab, base.gameObject, muzzleName, transmit: false);
            slashCount++;
            if (base.isAuthority)
            {
                Vector3 forward = base.characterDirection.forward;
                if ((bool)modelTransform)
                {
                    attack.hitBoxGroup = FindHitBoxGroup(hitBoxGroupName);
                    attack.forceVector = forward * forceMagnitude;
                    attack.Fire();
                }
                if ((bool)base.characterMotor)
                {
                    base.characterMotor.ApplyForce(forward * selfForce, alwaysApply: true);
                }
                for (int i = 0; i < projectileCount; i++)
                {
                    FireSpikeAuthority(aimRay, 0f, ((float)projectileCount / 2f - (float)i) * projectileYawSpread, projectileSpeed + projectileSpeedPerProjectile * (float)i);
                }
            }
        }

        private void FireSpikeAuthority(Ray aimRay, float bonusPitch, float bonusYaw, float speed)
        {
            Vector3 forward = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, bonusYaw, bonusPitch);
            ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * projectileDamageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, speed);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((char)chosenAnim);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            chosenAnim = reader.ReadChar();
        }
    }
}