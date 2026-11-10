using EntityStates;
using NW;
using RoR2;
using UnityEngine;

namespace EntityStates.GrandParentBoss.Great
{
    public class ChargeSupernova : BaseState
    {
        public static float baseDuration = 5;

        public static GameObject chargingEffectPrefab = NWAssets.LoadAsset<GameObject>("ChargeSuperNova");

        public static GameObject areaIndicatorPrefab = NWAssets.LoadAsset<GameObject>("SuperNovaAreaIndicator");

        public static string chargingSoundString = "Play_vagrant_R_charge";

        public static string animLayerName = "Body";

        public static string animStateName = "ChannelStart";

        public static string animPlaybackRateParam = "ChannelStart.playbackRate";

        public static float novaRadius = 160;

        private float duration;

        private CharacterModel characterModel;

        private float stopwatch;

        private GameObject chargeEffectInstance;

        private GameObject areaIndicatorInstance;

        private uint soundID;

        private float ogIntensity0;
        private float ogIntensity1;

        public override void OnEnter()
        {
            base.OnEnter();
            stopwatch = 0f;
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            characterModel = modelTransform.GetComponent<CharacterModel>();
            ogIntensity0 = characterModel.baseLightInfos[0].light.intensity;
            ogIntensity1 = characterModel.baseLightInfos[1].light.intensity;
            PlayAnimation(animLayerName, animStateName, animPlaybackRateParam, duration);
            soundID = Util.PlayAttackSpeedSound(chargingSoundString, base.gameObject, attackSpeedStat);
            if (!modelTransform)
            {
                return;
            }
            ChildLocator component = modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                Transform transform = component.FindChild("Head");
                Transform transform2 = component.FindChild("Head");
                if ((bool)transform && (bool)chargingEffectPrefab)
                {
                    chargeEffectInstance = Object.Instantiate(chargingEffectPrefab, transform.position, transform.rotation);
                    chargeEffectInstance.transform.localScale = new Vector3(novaRadius, novaRadius, novaRadius);
                    chargeEffectInstance.transform.parent = transform;
                    chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
                }
                if ((bool)transform2 && (bool)areaIndicatorPrefab)
                {
                    areaIndicatorInstance = Object.Instantiate(areaIndicatorPrefab, transform2.position, transform2.rotation);
                    areaIndicatorInstance.transform.localScale = new Vector3(novaRadius * 2f, novaRadius * 2f, novaRadius * 2f);
                    areaIndicatorInstance.transform.parent = transform2;

                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            AkSoundEngine.StopPlayingID(soundID);
            if ((bool)chargeEffectInstance)
            {
                EntityState.Destroy(chargeEffectInstance);
            }
            if ((bool)areaIndicatorInstance)
            {
                EntityState.Destroy(areaIndicatorInstance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            var light0 = characterModel.baseLightInfos[0].light;
            var light1 = characterModel.baseLightInfos[1].light;
            light0.intensity += 100 * Time.fixedDeltaTime;
            light1.intensity += 100 * Time.fixedDeltaTime;
            if (stopwatch >= duration && base.isAuthority)
            {
                light0.intensity = ogIntensity0 / 2;
                light1.intensity = ogIntensity1 / 2;
                FireSuperNova fireSuperNova = new FireSuperNova();
                fireSuperNova.novaRadius = novaRadius;
                outer.SetNextState(fireSuperNova);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
