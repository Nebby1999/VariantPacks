// RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EntityStates.Bison.Charge
using NW;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Bison.Pinky
{

    public class PinkyCharge : BaseState
    {
        public static float chargeDuration;
        public static string startSoundString;
        public static string footstepOverrideSoundString;
        public static float damageCoefficient;
        public static GameObject hitEffectPrefab;
        public static float upwardForceMagnitude;
        public static float awayForceMagnitude;
        public static string endSoundString;
        public static float overlapResetFrequency;
        public static float overlapSphereRadius;
        public static float selfStunDuration;
        public static float selfStunForce;
        public static string headbuttImpactSound;
        public static float chargeMovementSpeedCoefficient;
        private static bool _initFromOrig;
        private static int forwardSpeedParamHash = Animator.StringToHash("forwardSpeed");

        public bool _hurtBoxAssigned;
        public int _retryCount;
        public HurtBox _bestHurtBox;

        private Animator animator;
        private ChildLocator childLocator;
        private Transform sphereCheckTransform;
        private string baseFootstepString; 
        private OverlapAttack attack;
        private HitBoxGroup hitboxGroup;
        private Vector3 idealDirection;
        private float stopwatch;
        private float overlapResetStopwatch;
        private int origLayer = LayerIndex.defaultLayer.intVal;
        private float _origTurnSpeed;

        public override void OnEnter()
        {
            base.OnEnter();
            if(!_initFromOrig)
            {
                _initFromOrig = true;
                startSoundString = Charge.startSoundString;
                footstepOverrideSoundString = Charge.footstepOverrideSoundString;
                upwardForceMagnitude = Charge.upwardForceMagnitude;
                awayForceMagnitude = 500;
                damageCoefficient = Charge.damageCoefficient;
                hitEffectPrefab = Charge.hitEffectPrefab;
                endSoundString = Charge.endSoundString;
                chargeDuration = Charge.chargeDuration;
                overlapResetFrequency = Charge.overlapResetFrequency;
                overlapSphereRadius = Charge.overlapSphereRadius;
                selfStunDuration = 3f;
                selfStunForce = -3000f;
                endSoundString = Charge.endSoundString;
                headbuttImpactSound = Charge.headbuttImpactSound;
                chargeMovementSpeedCoefficient = Charge.chargeMovementSpeedCoefficient;
            }
            _origTurnSpeed = characterDirection.turnSpeed;
            characterDirection.turnSpeed = 720;

            origLayer = gameObject.layer;
            gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(teamComponent.teamIndex).intVal;
            characterMotor.Motor.RebuildCollidableLayers();

            animator = GetModelAnimator();
            childLocator = animator.GetComponent<ChildLocator>();
            FootstepHandler component = animator.GetComponent<FootstepHandler>();
            if ((bool)component)
            {
                baseFootstepString = component.baseFootstepString;
                component.baseFootstepString = footstepOverrideSoundString;
            }
            Util.PlaySound(startSoundString, base.gameObject);
            PlayCrossfade("Body", "ChargeForward", 0.2f);
            ResetOverlapAttack();
            SetSprintEffectActive(active: true);
            if ((bool)childLocator)
            {
                sphereCheckTransform = childLocator.FindChild("SphereCheckTransform");
            }
            if (!sphereCheckTransform && (bool)base.characterBody)
            {
                sphereCheckTransform = base.characterBody.coreTransform;
            }
            if (!sphereCheckTransform)
            {
                sphereCheckTransform = base.transform;
            }

            if(!_bestHurtBox && !_hurtBoxAssigned)
                _bestHurtBox = GetBestTarget();

            UpdateDirection();
        }

        public override void OnExit()
        {
            characterDirection.turnSpeed = _origTurnSpeed;
            base.characterMotor.moveDirection = Vector3.zero;
            gameObject.layer = origLayer;
            characterMotor.Motor.RebuildCollidableLayers();

            Util.PlaySound(endSoundString, base.gameObject);
            Util.PlaySound("stop_bison_charge_attack_loop", base.gameObject);
            SetSprintEffectActive(active: false);
            FootstepHandler component = animator.GetComponent<FootstepHandler>();
            if ((bool)component)
            {
                component.baseFootstepString = baseFootstepString;
            }
            base.OnExit();
        }

        private Vector3 targetMoveVector;
        private Vector3 currentVelocity;
        private List<HurtBox> hitHurtboxes = new List<HurtBox>();
        private bool _hitTarget;
        private bool _reachedCloseDistance;

        private bool CanUpdateMoveVector()
        {
            if (_hitTarget)
                return false;

            if (_reachedCloseDistance)
                return false;

            float distance = Vector3.Distance(_lastKnownBestTargetPosition, characterBody.corePosition);
            if (distance > 10)
                return true;

            _reachedCloseDistance = true;
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            hitHurtboxes.Clear();
            UpdateDirection();

            targetMoveVector = CanUpdateMoveVector() ? idealDirection : targetMoveVector;

            characterMotor.moveDirection = targetMoveVector * moveSpeedStat;
            characterDirection.moveVector = targetMoveVector;

            animator.SetFloat(forwardSpeedParamHash, moveSpeedStat * chargeMovementSpeedCoefficient);

            //If our target is out of range, and we've already charged for a bit OR hit our target, go to a new state
            if(isAuthority && IsBestTargetOutOfRange() && (_hitTarget || stopwatch > (chargeDuration / (2 * moveSpeedStat))))
            {
                //Target hit? go to main
                //If we've tried 3 or more times to charge, go to main as well
                if (_hitTarget || _retryCount >= 2)
                {
                    outer.SetNextStateToMain();
                }
                else //restart state, ram again.
                {
                    outer.SetNextState(new PinkyPrepCharge
                    {
                        bestHurtBox = _bestHurtBox,
                        retryCount = _retryCount + 1
                    });
                }
                return;
            }

            if (base.isAuthority && attack.Fire(hitHurtboxes))
            {
                if(_hitTarget == false && _bestHurtBox)
                {
                    foreach(var hitHb in hitHurtboxes)
                    {
                        if(_bestHurtBox.healthComponent == hitHb.healthComponent)
                        {
                            _hitTarget = true;
                            break;
                        }
                    }
                }
                Util.PlaySound(headbuttImpactSound, base.gameObject);
            }
            if (overlapResetStopwatch >= 1f / overlapResetFrequency)
            {
                overlapResetStopwatch -= 1f / overlapResetFrequency;
            }
            if (base.isAuthority && HGPhysics.DoesOverlapSphere(sphereCheckTransform.position, overlapSphereRadius, LayerIndex.world.mask))
            {
                Util.PlaySound(headbuttImpactSound, base.gameObject);
                EffectManager.SimpleMuzzleFlash(hitEffectPrefab, base.gameObject, "SphereCheckTransform", transmit: true);
                base.healthComponent.TakeDamageForce(targetMoveVector * selfStunForce, alwaysApply: true);
                StunState stunState = new StunState();
                stunState.stunDuration = selfStunDuration;
                outer.SetNextState(stunState);
                return;
            }
            float deltaTime = GetDeltaTime();
            stopwatch += deltaTime;
            overlapResetStopwatch += deltaTime;
            if (stopwatch > chargeDuration)
            {
                outer.SetNextStateToMain();
            }
        }

        private bool IsBestTargetOutOfRange()
        {
            if(!_bestHurtBox)
            {
                return false;
            }

            var bestTargetPos = _lastKnownBestTargetPosition;
            Vector3 myDirection = targetMoveVector;
            Vector3 directionToOther = Vector3.Normalize(bestTargetPos - characterBody.corePosition).normalized;

            float dotProduct = Vector3.Dot(myDirection, directionToOther);
            NWLog.Info($"dotProduct PinkyCharge: {dotProduct}");

            //Character is behind us.
            if(dotProduct < 0)
            {
                //Too far behind us, return true.
                if(Vector3.Distance(bestTargetPos, characterBody.corePosition) > 10)
                {
                    return true;
                }
            }

            //Keep charging
            return false;
        }

        private HurtBox GetBestTarget()
        {
            //Stinky but i'll do it i dont care.
            if(characterBody && characterBody.master && characterBody.master.TryGetComponent<BaseAI>(out var ai) && ai.skillDriverEvaluation.aimTarget != null && ai.skillDriverEvaluation.aimTarget.bestHurtBox != null)
            {
                return ai.skillDriverEvaluation.aimTarget.bestHurtBox;
            }

            Ray aimRay = GetAimRay();
            BullseyeSearch targetSearch = new BullseyeSearch();
            targetSearch.teamMaskFilter = TeamMask.all;
            targetSearch.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
            targetSearch.filterByLoS = true;
            targetSearch.searchOrigin = aimRay.origin;
            targetSearch.searchDirection = aimRay.direction;
            targetSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
            targetSearch.maxDistanceFilter = 120;
            targetSearch.maxAngleFilter = 90;
            targetSearch.RefreshCandidates();
            targetSearch.FilterOutGameObject(gameObject);

            var searchResults = targetSearch.GetResults();
            foreach(var result in searchResults )
            {
                if (result != null && result.healthComponent && result.healthComponent.alive)
                {
                    return result;
                }
            }

            return null;
        }

        private void SetSprintEffectActive(bool active)
        {
            if ((bool)childLocator)
            {
                childLocator.FindChild("SprintEffect")?.gameObject.SetActive(active);
            }
        }

        private void ResetOverlapAttack()
        {
            if (!hitboxGroup)
            {
                Transform modelTransform = GetModelTransform();
                if ((bool)modelTransform)
                {
                    hitboxGroup = Array.Find(modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Charge");
                }
            }
            attack = new OverlapAttack();
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
            attack.damage = damageCoefficient * damageStat;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = Vector3.up * upwardForceMagnitude;
            attack.pushAwayForce = awayForceMagnitude;
            attack.hitBoxGroup = hitboxGroup;
        }

        private Vector3 _lastKnownBestTargetPosition;
        private void UpdateDirection()
        {
            Ray aimRay = GetAimRay();

            if(_bestHurtBox != null && _bestHurtBox.healthComponent && _bestHurtBox.healthComponent.alive)
            {
                _lastKnownBestTargetPosition = (_bestHurtBox.healthComponent && _bestHurtBox.healthComponent.body) ? _bestHurtBox.healthComponent.body.corePosition : _bestHurtBox.transform.position;
                idealDirection = (_lastKnownBestTargetPosition - aimRay.origin).normalized;
            }
            else if(inputBank)
            {
                idealDirection = inputBank.aimDirection.normalized;
                _lastKnownBestTargetPosition = aimRay.origin + (aimRay.direction * 10);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(_retryCount);
            bool hasHurtbox = _bestHurtBox;
            writer.Write(hasHurtbox);
            if (hasHurtbox)
            {
                HurtBoxReference hurtboxReference = HurtBoxReference.FromHurtBox(_bestHurtBox);
                writer.Write(hurtboxReference);
            }
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            _retryCount = reader.ReadInt32();
            bool hasHurtbox = reader.ReadBoolean();
            if (hasHurtbox)
            {
                _bestHurtBox = reader.ReadHurtBoxReference().ResolveHurtBox();
                _hurtBoxAssigned = _bestHurtBox;
            }
        }
    }

}