using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LemurianBruiserMonster.Incinerating
{
    public class ChargeSingleFireball : BaseState
    {
        public static float baseDuration = ChargeMegaFireball.baseDuration;
        public static GameObject chargeEffectPrefab = ChargeMegaFireball.chargeEffectPrefab;
        public static string attackString = ChargeMegaFireball.attackString;
        private float duration;
        private GameObject chargeInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Animator modelAnimator = GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform transform = component.FindChild("MuzzleMouth");
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
            if ((bool)modelAnimator)
            {
                PlayCrossfade("Gesture, Additive", "ChargeMegaFireball", "ChargeMegaFireball.playbackRate", duration, 0.1f);
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
                FireSingleFireball nextState = new FireSingleFireball();
                outer.SetNextState(nextState);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}