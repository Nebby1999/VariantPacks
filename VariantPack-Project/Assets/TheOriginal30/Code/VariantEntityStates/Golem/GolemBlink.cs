using EntityStates;
using EntityStates.ParentMonster;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.GolemMonster.Rush
{
    public class GolemBlink : BaseState
    {
        private Transform modelTransform;
        private float stopwatch;
        private Vector3 blinkDestination = Vector3.zero;
        private Vector3 blinkStart = Vector3.zero;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(LoomingPresence.beginSoundString, base.gameObject);
            this.modelTransform = base.GetModelTransform();

            if (this.modelTransform)
            {
                this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
            }

            if (this.characterModel)
            {
                this.characterModel.invisibilityCount++;
            }

            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            if (base.characterMotor)
            {
                base.characterMotor.enabled = false;
            }

            if (base.isAuthority)
            {
                Vector3 vector = base.inputBank.aimDirection * LoomingPresence.blinkDistance;
                this.blinkDestination = base.transform.position;
                this.blinkStart = base.transform.position;

                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + vector, base.characterBody.hullClassification);

                groundNodes.GetNodePosition(nodeIndex, out this.blinkDestination);

                this.blinkDestination += base.transform.position - base.characterBody.footPosition;
                vector = this.blinkDestination - this.blinkStart;

                this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject), vector);
            }
        }

        private void CreateBlinkEffect(Vector3 origin, Vector3 direction)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(direction);
            effectData.origin = origin;
            EffectManager.SpawnEffect(LoomingPresence.blinkPrefab, effectData, true);
        }

        private void SetPosition(Vector3 newPosition)
        {
            if (base.characterMotor)
            {
                base.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity = Vector3.zero;
            }

            this.SetPosition(Vector3.Lerp(this.blinkStart, this.blinkDestination, this.stopwatch / LoomingPresence.duration));

            if (this.stopwatch >= LoomingPresence.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(LoomingPresence.endSoundString, base.gameObject);
            this.modelTransform = base.GetModelTransform();

            if (base.characterDirection)
            {
                base.characterDirection.forward = base.GetAimRay().direction;
            }

            if (this.modelTransform && LoomingPresence.destealthMaterial)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = LoomingPresence.destealthDuration;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LoomingPresence.destealthMaterial;
                temporaryOverlay.inspectorCharacterModel = this.modelTransform.gameObject.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }

            if (this.characterModel)
            {
                this.characterModel.invisibilityCount--;
            }

            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            if (base.characterMotor)
            {
                base.characterMotor.enabled = true;
            }

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}