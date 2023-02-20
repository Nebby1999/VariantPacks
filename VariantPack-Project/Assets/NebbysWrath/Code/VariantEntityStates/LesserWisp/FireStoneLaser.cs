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
    public class FireStoneLaser : BaseState
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

        public Vector3 laserDirection;
        private float duration;
        private Ray modifiedAimRay;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            modifiedAimRay = GetAimRay();
            modifiedAimRay.direction = laserDirection;
            GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlaySound(attackSoundString, base.gameObject);
            string muzzleName = "Muzzle";
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(2f);
            }
            PlayAnimation("Body", "FireAttack1", "FireAttack1.playbackRate", duration);
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
            }
            if (!base.isAuthority)
            {
                return;
            }
            float num = 1000f;
            Vector3 vector = modifiedAimRay.origin + modifiedAimRay.direction * num;
            if (Physics.Raycast(modifiedAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask | (int)LayerIndex.entityPrecise.mask))
            {
                vector = hitInfo.point;
            }
            BlastAttack blastAttack = new BlastAttack();
            blastAttack.attacker = base.gameObject;
            blastAttack.inflictor = base.gameObject;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
            blastAttack.baseDamage = damageStat * 5.72f * damageCoefficient;
            blastAttack.baseForce = force * 0.2f;
            blastAttack.position = vector;
            blastAttack.radius = blastRadius;
            blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            blastAttack.bonusForce = force * modifiedAimRay.direction;
            blastAttack.Fire();
            _ = modifiedAimRay.origin;
            if (!modelTransform)
            {
                return;
            }
            ChildLocator component = modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                int childIndex = component.FindChildIndex(muzzleName);
                if ((bool)tracerEffectPrefab)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = vector,
                        start = modifiedAimRay.origin
                    };
                    effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
                    EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
                    EffectManager.SpawnEffect(hitEffectPrefab, effectData, transmit: true);
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