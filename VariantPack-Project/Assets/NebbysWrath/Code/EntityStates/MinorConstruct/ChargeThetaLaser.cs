using RoR2;
using RoR2.AsyncManagement;
using UnityEngine;

namespace EntityStates.MinorConstruct.Weapon.Theta
{
    public class ChargeThetaLaser : BaseState
    {
        public static string enterSoundString;

        public static string exitSoundString;

        public static string animationLayerName;

        public static string animationStateName;

        public static string animationPlaybackRateParam;

        public static string chargeEffectMuzzle;

        public static EntityStateGameObject chargeEffectPrefab;

        public static float baseDuration;

        private float duration;

        private GameObject chargeInstance;

        private static bool _initFromOrig = false;

        public override void OnEnter()
        {
            base.OnEnter();
            if(!_initFromOrig)
            {
                _initFromOrig = true;
                var orig = new ChargeConstructBeam();
                enterSoundString = orig.enterSoundString;
                exitSoundString = orig.exitSoundString;
                animationLayerName = orig.animationLayerName;
                animationStateName = orig.animationStateName;
                animationPlaybackRateParam = orig.animationPlaybackRateParam;
                chargeEffectMuzzle = orig.chargeEffectMuzzle;
                chargeEffectPrefab = orig.chargeEffectPrefab;
                baseDuration = orig.baseDuration;
            }

            duration = baseDuration / attackSpeedStat;
            PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
            Util.PlaySound(enterSoundString, base.gameObject);
            Transform transform = FindModelChild(chargeEffectMuzzle);
            if ((bool)transform && (bool)chargeEffectPrefab)
            {
                chargeInstance = Object.Instantiate(chargeEffectPrefab.gameObject, transform.position, transform.rotation);
                chargeInstance.transform.parent = base.gameObject.transform;
                ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
                if ((bool)component)
                {
                    component.newDuration = duration;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if ((bool)chargeInstance)
            {
                Ray aimRay = GetAimRay();
                chargeInstance.transform.forward = aimRay.direction;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > duration)
            {
                outer.SetNextState(new FireThetaLaser());
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(exitSoundString, base.gameObject);
            if ((bool)chargeInstance)
            {
                EntityState.Destroy(chargeInstance);
            }
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

}