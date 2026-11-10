using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.VagrantMonster.Weapon.Mothership
{
    public class ChargeJellies : BaseState
    {
        public static float baseDuration = 3f;

        public static GameObject chargingEffectPrefab;

        public static string chargingSoundString;

        private float duration;

        private float stopwatch;

        private GameObject chargeEffectInstance;

        private uint soundID;

        public override void OnEnter()
        {
            chargingEffectPrefab = ChargeTrackingBomb.chargingEffectPrefab;
            chargingSoundString = ChargeTrackingBomb.chargingSoundString;
            base.OnEnter();
            stopwatch = 0f;
            duration = baseDuration / 5;
            Transform modelTransform = GetModelTransform();
            PlayCrossfade("Gesture, Override", "ChargeTrackingBomb", "ChargeTrackingBomb.playbackRate", duration, 0.3f);
            soundID = Util.PlayAttackSpeedSound(chargingSoundString, base.gameObject, 5);
            if (!modelTransform)
            {
                return;
            }
            ChildLocator component = modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                Transform transform = component.FindChild("TrackingBombMuzzle");
                if ((bool)transform && (bool)chargingEffectPrefab)
                {
                    chargeEffectInstance = Object.Instantiate(chargingEffectPrefab, transform.position, transform.rotation);
                    chargeEffectInstance.transform.parent = transform;
                    chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)chargeEffectInstance)
            {
                EntityState.Destroy(chargeEffectInstance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= duration && base.isAuthority)
            {
                outer.SetNextState(new FireJellies());
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
