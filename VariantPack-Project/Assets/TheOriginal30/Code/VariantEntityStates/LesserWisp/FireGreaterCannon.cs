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
            if (goodState == null) goodState = new FireCannons();
            effectPrefab = goodState.effectPrefab;

            base.OnEnter();
            Ray aimRay = GetAimRay();
            duration = baseDuration / attackSpeedStat;

            PlayAnimation("Body", "FireAttack1", "FireAttack1.playbackRate", duration);

            //EffectManager.SimpleMuzzleFlash(this.effectPrefab, base.gameObject, "Muzzle", false);

            if (isAuthority && modelLocator && modelLocator.modelTransform)
            {
                ChildLocator childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    Transform muzzleTransform = childLocator.FindChild("Muzzle");
                    if (muzzleTransform)
                    {
                        DamageTypeCombo? comboOverride = null;
                        var projectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/WispCannon");
                        if(projectilePrefab.TryGetComponent<ProjectileDamage>(out var prjDamage))
                        {
                            var combo = prjDamage.damageType;
                            combo.damageSource |= DamageSource.Primary;
                            comboOverride = combo;
                        }

                        ProjectileManager.instance.FireProjectile(projectilePrefab, transform.position, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damageStat * damageCoefficient, 80f, Util.CheckRoll(critStat, characterBody.master), DamageColorIndex.Default, null, -1f, comboOverride);
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

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
