using EntityStates.RoboBallBoss.Weapon;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.RoboBallMini.Weapon.MK2
{
    public class MK2Charge : BaseState
    {
        public static float baseDuration = 1f;

        public static GameObject chargeEffectPrefab;

        public static string attackString;

        public static string muzzleString;

        private float duration;

        private GameObject chargeInstance;

        public override void OnEnter()
        {
            baseDuration = ChargeEyeblast.baseDuration;
            chargeEffectPrefab = ChargeEyeblast.chargeEffectPrefab;
            attackString = ChargeEyeblast.attackString;
            muzzleString = "Muzzle";
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform transform = component.FindChild(muzzleString);
                    if ((bool)transform && (bool)chargeEffectPrefab)
                    {
                        chargeInstance = Object.Instantiate(chargeEffectPrefab, transform.position, transform.rotation);
                        chargeInstance.transform.parent = transform;
                        ScaleParticleSystemDuration component2 = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
                        if ((bool)component2)
                        {
                            component2.newDuration = duration;
                        }
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)chargeInstance)
            {
                EntityState.Destroy(chargeInstance);
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextState(GetNextState());
            }
        }

        public virtual EntityState GetNextState()
        {
            return new MK2Fire();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
