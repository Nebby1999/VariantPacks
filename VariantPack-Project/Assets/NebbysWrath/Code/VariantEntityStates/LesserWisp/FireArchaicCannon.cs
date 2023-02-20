using EntityStates.ArchWispMonster;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Wisp1Monster.Archaic
{
    public class FireArchaicCannon : BaseState
    {
        public static GameObject projectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/ArchWisp/ArchWispCannon.prefab").WaitForCompletion();
        public static float baseDuration = 2f;
        public static float damageCoefficient = 12f;

        private float duration;
        private GameObject effectPrefab;
        private FireCannons goodState;

        public override void OnEnter()
        {
            if (goodState == null)
            {
                goodState = new FireCannons();
            }
            effectPrefab = goodState.effectPrefab;

            base.OnEnter();
            Ray aimRay = base.GetAimRay();
            duration = baseDuration / attackSpeedStat;

            PlayAnimation("Body", "FireAttack1", "FireAttack1.playbackRate", duration);
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(this.effectPrefab, base.gameObject, "Muzzle", false);
            }

            if (base.isAuthority && base.modelLocator && base.modelLocator.modelTransform)
            {
                ChildLocator childLocator = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    Transform muzzleTransform = childLocator.FindChild("Muzzle");
                    if (muzzleTransform)
                    {
                        ProjectileManager.instance.FireProjectile(projectile, transform.position, Util.QuaternionSafeLookRotation(aimRay.direction), gameObject, damageStat * damageCoefficient, 80f, Util.CheckRoll(critStat, characterBody.master), DamageColorIndex.Default, null, -1f);
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
