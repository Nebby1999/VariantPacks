using EntityStates.Mimic.Chest;
using RoR2;
using RoR2.AsyncManagement;
using UnityEngine;
using VAPI;

namespace EntityStates.Mimic.Gluttonous
{
    public class GluttonousBreakOpen : MimicRevealBaseState
    {
        [SystemInitializer]
        private static void Init()
        {
            On.EntityStates.Mimic.Devour.DevouredBody.RevealSelf += (orig, self) =>
            {
                if(self.TryGetComponent<CharacterBodyVariantController>(out var variantController) && variantController.IsVariantApplied(NW.NWContent.CharacterVariants.GluttonousMimic))
                {
                    EntityState nextState = new GluttonousBreakOpen();
                    self.outer.SetNextState(nextState);
                    return;
                }
                orig(self);
            };
        }

        [SerializeField]
        public float AnimDuration;

        [SerializeField]
        public float SelfStunDuration;

        [SerializeField]
        public EntityStateGameObject BreakOpenVFX;

        private static BreakOpen origState;

        public override void OnEnter()
        {
            origState ??= new BreakOpen();
            AnimDuration = origState.AnimDuration;
            SelfStunDuration = origState.SelfStunDuration;
            BreakOpenVFX = origState.BreakOpenVFX;
            base.OnEnter();
            PlayCrossfade("FullBody, Override", "Mimic|MonsterForm", "Release.playbackRate", AnimDuration, 0.1f);
            if ((bool)BreakOpenVFX)
            {
                Transform transform = GetModelChildLocator().FindChild("Muzzle");
                EffectData effectData = new EffectData();
                effectData.origin = transform.position;
                effectData.rotation = transform.rotation;
                effectData.scale = transform.localScale.magnitude;
                if ((bool)transform)
                {
                    EffectManager.SpawnEffect(BreakOpenVFX, effectData, transmit: false);
                }
            }
            base.characterBody.fakeActorCounter++;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= AnimDuration)
            {
                StunState nextState = new StunState
                {
                    stunDuration = SelfStunDuration
                };
                outer.SetNextState(nextState);
            }
        }

        public override void OnExit()
        {
            base.characterBody.fakeActorCounter--;
            base.OnExit();
        }
    }
}