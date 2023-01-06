using EntityStates;
using EntityStates.GreaterWispMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Wisp1Monster.AlmostGreat
{
    public class FireGreaterCannon : BaseState
    {
        public static float baseDuration = 2f;
        public static float damageCoefficient = 12f;

        private float duration;
        private GameObject effectPrefab;
        private FireCannons goodState;

        public override void OnEnter()
        {
            if (this.goodState == null) this.goodState = new FireCannons();
            this.effectPrefab = this.goodState.effectPrefab;

            base.OnEnter();
            Ray aimRay = base.GetAimRay();
            this.duration = FireGreaterCannon.baseDuration / this.attackSpeedStat;

            base.PlayAnimation("Body", "FireAttack1", "FireAttack1.playbackRate", this.duration);

            //EffectManager.SimpleMuzzleFlash(this.effectPrefab, base.gameObject, "Muzzle", false);

            if (base.isAuthority && base.modelLocator && base.modelLocator.modelTransform)
            {
                ChildLocator childLocator = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    Transform muzzleTransform = childLocator.FindChild("Muzzle");
                    if (muzzleTransform)
                    {
                        ProjectileManager.instance.FireProjectile(Resources.Load<GameObject>("Prefabs/Projectiles/WispCannon"), transform.position, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageStat * FireGreaterCannon.damageCoefficient, 80f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
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

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
