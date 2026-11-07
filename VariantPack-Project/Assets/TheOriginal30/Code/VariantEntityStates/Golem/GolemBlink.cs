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
            Util.PlaySound(LoomingPresence.beginSoundString, gameObject);
            modelTransform = GetModelTransform();

            if (modelTransform)
            {
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }

            if (characterModel)
            {
                characterModel.invisibilityCount++;
            }

            if (hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            if (characterMotor)
            {
                characterMotor.enabled = false;
            }

            if (isAuthority)
            {
                Vector3 vector = inputBank.aimDirection * LoomingPresence.blinkDistance;
                blinkDestination = transform.position;
                blinkStart = transform.position;

                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(transform.position + vector, characterBody.hullClassification);

                groundNodes.GetNodePosition(nodeIndex, out blinkDestination);

                blinkDestination += transform.position - characterBody.footPosition;
                vector = blinkDestination - blinkStart;

                CreateBlinkEffect(Util.GetCorePosition(gameObject), vector);
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
            if (characterMotor)
            {
                characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;

            if (characterMotor && characterDirection)
            {
                characterMotor.velocity = Vector3.zero;
            }

            SetPosition(Vector3.Lerp(blinkStart, blinkDestination, stopwatch / LoomingPresence.duration));

            if (stopwatch >= LoomingPresence.duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(LoomingPresence.endSoundString, gameObject);
            modelTransform = GetModelTransform();

            if (characterDirection)
            {
                characterDirection.forward = GetAimRay().direction;
            }

            if (modelTransform && LoomingPresence.destealthMaterial)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = LoomingPresence.destealthDuration;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LoomingPresence.destealthMaterial;
                temporaryOverlay.inspectorCharacterModel = modelTransform.gameObject.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }

            if (characterModel)
            {
                characterModel.invisibilityCount--;
            }

            if (hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            if (characterMotor)
            {
                characterMotor.enabled = true;
            }

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}