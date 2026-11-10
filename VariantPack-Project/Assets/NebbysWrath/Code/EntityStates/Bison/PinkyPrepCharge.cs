// RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EntityStates.Bison.PrepCharge
using RoR2;
using RoR2.AsyncManagement;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Bison.Pinky
{

    public class PinkyPrepCharge : BaseState
    {
        public static float basePrepDuration;

        public static string enterSoundString;

        public static EntityStateGameObject chargeEffectPrefab;

        public float stopwatch;

        public float prepDuration;

        private GameObject chargeEffectInstance;

        private EffectManagerHelper _emh_chargeEffectInstance;

        private static bool _initFromOrig;

        public HurtBox bestHurtBox;
        public int retryCount;

        private float _origTurnSpeed;

        public override void Reset()
        {
            base.Reset();
            prepDuration = 0f;
            chargeEffectInstance = null;
            _emh_chargeEffectInstance = null;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (_initFromOrig == false)
            {
                _initFromOrig = true;
                basePrepDuration = PrepCharge.basePrepDuration;
                enterSoundString = PrepCharge.enterSoundString;
                chargeEffectPrefab = PrepCharge.chargeEffectPrefab;
            }

            _origTurnSpeed = characterDirection.turnSpeed;
            characterDirection.turnSpeed = 720;

            if(isAuthority && retryCount > 0 && skillLocator.utility) //If we've entered our retries, restart stopwatch.
            {
                skillLocator.utility.rechargeStopwatch = 0;
            }

            if(isAuthority && !bestHurtBox)
            {
                bestHurtBox = GetBestTarget();
            }

            prepDuration = basePrepDuration;
            base.characterBody.SetAimTimer(prepDuration);
            PlayCrossfade("Body", "PrepCharge", "PrepCharge.playbackRate", prepDuration, 0.2f);
            Util.PlaySound(enterSoundString, base.gameObject);
            Transform modelTransform = GetModelTransform();
            AimAnimator component = modelTransform.GetComponent<AimAnimator>();
            if ((bool)component)
            {
                component.enabled = true;
            }

            //Turn to face our target
            if ((bool)base.characterDirection)
            {
                base.characterDirection.moveVector = bestHurtBox ? (bestHurtBox.transform.position - characterBody.corePosition).normalized : GetAimRay().direction;
            }

            if (!modelTransform)
            {
                return;
            }
            ChildLocator component2 = modelTransform.GetComponent<ChildLocator>();
            if (!component2)
            {
                return;
            }
            Transform transform = component2.FindChild("ChargeIndicator");
            if ((bool)transform && (bool)chargeEffectPrefab)
            {
                if (!EffectManager.ShouldUsePooledEffect(chargeEffectPrefab))
                {
                    chargeEffectInstance = Object.Instantiate(chargeEffectPrefab.gameObject, transform.position, transform.rotation);
                }
                else
                {
                    _emh_chargeEffectInstance = EffectManager.GetAndActivatePooledEffect(chargeEffectPrefab, transform.position, transform.rotation);
                    chargeEffectInstance = _emh_chargeEffectInstance.gameObject;
                }
                chargeEffectInstance.transform.parent = transform;
                ScaleParticleSystemDuration component3 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                if ((bool)component3)
                {
                    component3.newDuration = prepDuration;
                }
            }
        }

        public override void OnExit()
        {
            characterDirection.turnSpeed = _origTurnSpeed;

            if ((bool)chargeEffectInstance)
            {
                if (_emh_chargeEffectInstance != null && _emh_chargeEffectInstance.OwningPool != null)
                {
                    _emh_chargeEffectInstance.OwningPool.ReturnObject(_emh_chargeEffectInstance);
                }
                else
                {
                    EntityState.Destroy(chargeEffectInstance);
                }
                chargeEffectInstance = null;
                _emh_chargeEffectInstance = null;
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += GetDeltaTime();
            if (stopwatch > prepDuration && base.isAuthority)
            {
                outer.SetNextState(new PinkyCharge()
                {
                    _hurtBoxAssigned = bestHurtBox,
                    _bestHurtBox = bestHurtBox,
                    _retryCount = retryCount
                });
            }
        }

        private HurtBox GetBestTarget()
        {
            //Stinky but i'll do it i dont care.
            if (characterBody && characterBody.master && characterBody.master.TryGetComponent<BaseAI>(out var ai) && ai.skillDriverEvaluation.aimTarget != null && ai.skillDriverEvaluation.aimTarget.bestHurtBox != null)
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
            foreach (var result in searchResults)
            {
                if (result != null && result.healthComponent && result.healthComponent.alive)
                {
                    return result;
                }
            }

            return null;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(retryCount);
            bool hasHurtbox = bestHurtBox;
            writer.Write(hasHurtbox);
            if(hasHurtbox)
            {
                HurtBoxReference hurtboxReference = HurtBoxReference.FromHurtBox(bestHurtBox);
                writer.Write(hurtboxReference);
            }
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            retryCount = reader.ReadInt32();
            bool hasHurtbox = reader.ReadBoolean();
            if(hasHurtbox)
            {
                bestHurtBox = reader.ReadHurtBoxReference().ResolveHurtBox();
            }
        }
    }
}