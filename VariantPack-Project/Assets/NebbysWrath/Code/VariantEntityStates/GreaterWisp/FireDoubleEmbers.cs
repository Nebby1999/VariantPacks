

using EntityStates;
using EntityStates.GreaterWispMonster;
using EntityStates.Wisp1Monster;
using RoR2;
using UnityEngine;

namespace EntityStates.GreaterWispMonster.Amalgamated
{
    public class FireDoubleEmbers : BaseState
    {
        public static string attackString = FireEmbers.attackString;
        public static float minSpread = FireEmbers.minSpread;
        public static float maxSpread = FireEmbers.maxSpread;
        public static int bulletCount = (int)(FireEmbers.bulletCount * 2.5f);
        public static GameObject tracerEffectPrefab = FireEmbers.tracerEffectPrefab;
        public static GameObject hitEffectPrefab = FireEmbers.hitEffectPrefab;
        public GameObject effectPrefab = FireEmbers.effectPrefab;
        public static float baseDuration = FireCannons.baseDuration;
        public float damageCoefficient = FireEmbers.damageCoefficient;
        public float force = FireEmbers.force;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Ray aimRay = GetAimRay();
            string text1 = "MuzzleLeft";
            string text2 = "MuzzleRight";
            duration = baseDuration / attackSpeedStat;
            Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
            StartAimMode(aimRay);
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, text1, transmit: false);
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, text2, transmit: false);
            }
            PlayAnimation("Gesture", "FireCannons", "FireCannons.playbackRate", duration);
            if (!base.isAuthority || !base.modelLocator || !base.modelLocator.modelTransform)
            {
                return;
            }
            ChildLocator component = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                int childIndex = component.FindChildIndex(text1);
                int childIndex2 = component.FindChildIndex(text2);
                Transform transform = component.FindChild(childIndex);
                Transform transform2 = component.FindChild(childIndex2);
                if ((bool)transform)
                {
                    BulletAttack bulletAttack = new BulletAttack();
                    bulletAttack.owner = base.gameObject;
                    bulletAttack.weapon = base.gameObject;
                    bulletAttack.origin = aimRay.origin;
                    bulletAttack.aimVector = aimRay.direction;
                    bulletAttack.minSpread = minSpread;
                    bulletAttack.maxSpread = maxSpread;
                    bulletAttack.bulletCount = (uint)((bulletCount > 0) ? bulletCount : 0);
                    bulletAttack.damage = damageCoefficient * damageStat;
                    bulletAttack.force = force;
                    bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
                    bulletAttack.muzzleName = text1;
                    bulletAttack.hitEffectPrefab = hitEffectPrefab;
                    bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                    bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
                    bulletAttack.HitEffectNormal = false;
                    bulletAttack.radius = 0.5f;
                    bulletAttack.procCoefficient = 1f / (float)bulletCount;
                    bulletAttack.Fire();
                }
                if ((bool)transform2)
                {
                    BulletAttack bulletAttack = new BulletAttack();
                    bulletAttack.owner = base.gameObject;
                    bulletAttack.weapon = base.gameObject;
                    bulletAttack.origin = aimRay.origin;
                    bulletAttack.aimVector = aimRay.direction;
                    bulletAttack.minSpread = minSpread;
                    bulletAttack.maxSpread = maxSpread;
                    bulletAttack.bulletCount = (uint)((bulletCount > 0) ? bulletCount : 0);
                    bulletAttack.damage = damageCoefficient * damageStat;
                    bulletAttack.force = force;
                    bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
                    bulletAttack.muzzleName = text2;
                    bulletAttack.hitEffectPrefab = hitEffectPrefab;
                    bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                    bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
                    bulletAttack.HitEffectNormal = false;
                    bulletAttack.radius = 0.5f;
                    bulletAttack.procCoefficient = 1f / (float)bulletCount;
                    bulletAttack.Fire();
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
