using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.MoffeinAncientWispSkills.AncientStoneWisp
{
    public class ChargeLaserBarrage : BaseState
    {

        public static float baseDuration = 2.5f;
        public static GameObject effectPrefab;
        private float duration;

        private GameObject chargeEffectLeft;
        private GameObject chargeEffectRight;
        private Animator modelAnimator;
        private bool playedSwing2 = false;

        [AsyncAssetLoad]
        public static IEnumerator LoadAssets()
        {
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/ChargeGolem.prefab");

            while (!request.IsDone)
                yield return null;

            effectPrefab = request.Result;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            duration = ChargeLaserBarrage.baseDuration / attackSpeedStat;
            Transform modelTransform = base.GetModelTransform();
            //base.PlayAnimation("Gesture", "ChargeRHCannon", "ChargeRHCannon.playbackRate", this.duration);
            modelAnimator = base.GetModelAnimator();
            if (modelAnimator)
            {
                int layerIndex = modelAnimator.GetLayerIndex("Gesture");
                if (modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Throw1"))
                {
                    base.PlayCrossfade("Gesture", "Throw2", "Throw.playbackRate", duration / 0.3f, 0.2f);
                }
                else
                {
                    base.PlayCrossfade("Gesture", "Throw1", "Throw.playbackRate", duration / 0.3f, 0.2f);
                }
            }
            if (modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if (component && ChargeLaserBarrage.effectPrefab)
                {
                    Transform transform = component.FindChild("MuzzleRight");
                    if (transform)
                    {
                        chargeEffectRight = UnityEngine.Object.Instantiate<GameObject>(ChargeLaserBarrage.effectPrefab, transform.position, transform.rotation);
                        chargeEffectRight.transform.parent = transform;
                    }
                }
            }
            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(duration);
            }
            Util.PlayAttackSpeedSound("Play_greater_wisp_attack", base.gameObject, attackSpeedStat * (2f / ChargeLaserBarrage.baseDuration));
        }

        public override void OnExit()
        {
            base.OnExit();
            EntityState.Destroy(chargeEffectLeft);
            EntityState.Destroy(chargeEffectRight);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!playedSwing2 && duration - base.fixedAge < 0.6f)
            {
                if (modelAnimator)
                {
                    playedSwing2 = true;
                    int layerIndex = modelAnimator.GetLayerIndex("Gesture");
                    if (modelAnimator.GetCurrentAnimatorStateInfo(layerIndex).IsName("Throw1"))
                    {
                        base.PlayCrossfade("Gesture", "Throw2", "Throw.playbackRate", FireLaserBarrage.baseDuration * 6f, 0.1f);
                    }
                    else
                    {
                        base.PlayCrossfade("Gesture", "Throw1", "Throw.playbackRate", FireLaserBarrage.baseDuration * 6f, 0.1f);
                    }
                }
            }

            if (base.fixedAge >= duration && base.isAuthority)
            {
                FireLaserBarrage nextState = new FireLaserBarrage();
                outer.SetNextState(nextState);
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}