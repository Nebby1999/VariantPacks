using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LemurianMonster.Badass
{
    public class ChargeFireballAndMissile : BaseState
    {
        public static float baseDuration = ChargeFireball.baseDuration;

        public static GameObject chargeVfxPrefab = ChargeFireball.chargeVfxPrefab;

        public static string attackString = ChargeFireball.attackString;

        private float duration;

        private GameObject chargeVfxInstance;

        public override void OnEnter()
        {
            chargeVfxPrefab = ChargeFireball.chargeVfxPrefab;
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform transform = component.FindChild("MuzzleMouth");
                    if ((bool)transform && (bool)chargeVfxPrefab)
                    {
                        chargeVfxInstance = Object.Instantiate(chargeVfxPrefab, transform.position, transform.rotation);
                        chargeVfxInstance.transform.parent = transform;
                    }
                }
            }
            PlayAnimation("Gesture", "ChargeFireball", "ChargeFireball.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)chargeVfxInstance)
            {
                EntityState.Destroy(chargeVfxInstance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                FireFireballAndMissile nextState = new FireFireballAndMissile();
                outer.SetNextState(nextState);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}