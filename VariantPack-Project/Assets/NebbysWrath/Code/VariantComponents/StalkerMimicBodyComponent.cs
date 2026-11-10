using HG;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = true, useOnServer = true)]
    public class StalkerMimicComponent : VariantComponent
    {
        private class VisibilityDetector : MonoBehaviour
        {
            public bool visible { get; private set; }
            private void OnBecameVisible()
            {
                visible = true;
            }

            private void OnBecameInvisible()
            {
                visible = false;
            }
        }

        private CharacterBody _characterBody;
        private EntityStateMachine _bodyStateMachine;
        private MimicVariantBodyApplicator _mimicVariantBodyApplicator;
        private bool _isDisguised;
        private RangeFloat _timeBetweenTeleports = new RangeFloat
        {
            min = 15f,
            max = 25f
        };
        private Run.FixedTimeStamp _timeStampAuthority;
        private bool _targetingAndPredictionControllerAdded;
        private TargetingAndPredictionController _targetingAndPredictionController;
        private VisibilityDetector _visibilityDetector;

        private void Awake()
        {
            _mimicVariantBodyApplicator = GetComponent<MimicVariantBodyApplicator>();
            _characterBody = GetComponent<CharacterBody>();
            if(_characterBody.modelLocator && _characterBody.modelLocator.modelTransform && _characterBody.modelLocator.modelTransform.TryGetComponent<CharacterModel>(out var model))
            {
                StartCoroutine(RetrieveMainRendererThenAddVisibilityDetector(model));
            }

            if(!gameObject.TryGetComponent(out _targetingAndPredictionController))
            {
                _targetingAndPredictionControllerAdded = true;
                _targetingAndPredictionController = gameObject.AddComponent<TargetingAndPredictionController>();
            }
        }

        private IEnumerator RetrieveMainRendererThenAddVisibilityDetector(CharacterModel model)
        {
            while (model != null && model.GetMainRenderer() == null)
                yield return null;

            if (model == null)
                yield break;

            SkinnedMeshRenderer renderer = model.GetMainRenderer();
            if (!renderer)
                yield break;

            if(Util.HasEffectiveAuthority(gameObject))
            {
                _visibilityDetector = renderer.gameObject.AddComponent<VisibilityDetector>();
            }
        }

        private void OnEnable()
        {
            _mimicVariantBodyApplicator.onDisguisedChanged += OnDisguisedChanged;
        }

        private void OnDisable()
        {
            _mimicVariantBodyApplicator.onDisguisedChanged -= OnDisguisedChanged;
        }
        private void OnDisguisedChanged()
        {
            _isDisguised = _mimicVariantBodyApplicator.isDisguised;
            if(Util.HasEffectiveAuthority(gameObject))
            {
                _timeStampAuthority = GetTimeForNextTeleport();
            }
        }

        private void Start()
        {
            _isDisguised = _mimicVariantBodyApplicator.isDisguised;
            _bodyStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            if (_isDisguised && Util.HasEffectiveAuthority(gameObject))
            {
                _timeStampAuthority = GetTimeForNextTeleport();
            }
        }

        private void OnDestroy()
        {
            if(_targetingAndPredictionControllerAdded && _targetingAndPredictionController)
            {
                Destroy(_targetingAndPredictionController);
                _targetingAndPredictionControllerAdded = false;
                _targetingAndPredictionController = null;
            }

            if(_visibilityDetector)
            {
                Destroy(_visibilityDetector);
            }
        }

        private void FixedUpdate()
        {
            if (!Util.HasEffectiveAuthority(gameObject))
            {
                return;
            }

            if(!_isDisguised)
            {
                return;
            }

            if (_timeStampAuthority.hasPassed)
            {
                if (!Util.HasEffectiveAuthority(gameObject))
                {
                    _timeStampAuthority = GetTimeForNextTeleport();
                    return;
                }

                NWLog.Info($"Trying to teleport");
                if (TrySetTeleportState())
                {
                    _timeStampAuthority = GetTimeForNextTeleport();
                }
                else
                {
                    _timeStampAuthority = Run.FixedTimeStamp.now + 0.25f;
                }
            }
        }

        private bool TrySetTeleportState()
        {
            if (_bodyStateMachine == null)
                return true;

            if(_visibilityDetector && _visibilityDetector.visible == true)
            {
                NWLog.Info($"Mimic is visible by authority, not teleporting");
                return false;
            }

            //Closest thing we'll get i guess.
            var target = GetTarget();
            if(target)
            {
                float distanceFromTargetToSelf = Vector3.Distance(target.corePosition, _characterBody.corePosition);
                if (distanceFromTargetToSelf < 75)
                {
                    NWLog.Info($"Target too close to mimic, not teleporting");
                    return false;
                }

                Ray rayToSelf = new Ray(target.corePosition, target.corePosition - _characterBody.corePosition);
                if(Util.CharacterRaycast(gameObject, rayToSelf, out var hitInfo, 1024, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    var hitInfoPoint = hitInfo.point;
                    var distance = Vector3.Distance(hitInfoPoint, _characterBody.corePosition);
                    if(distance < 50)
                    {
                        NWLog.Info("Bad occluded check failed");
                        return false;
                    }
                }
            }

            NWLog.Info($"Teleporting behind target");
            _bodyStateMachine.SetNextState(new EntityStates.Mimic.Stalker.TeleportBehind());
            return true;
        }

        private Run.FixedTimeStamp GetTimeForNextTeleport() => Run.FixedTimeStamp.now + UnityEngine.Random.Range(_timeBetweenTeleports.min, _timeBetweenTeleports.max);

        private BullseyeSearch _targetBullseyeSearch = new BullseyeSearch();
        private List<HurtBox> _results = new List<HurtBox>();

        private CharacterBody GetTarget()
        {
            CharacterBody result = null;
            if(TryGetComponent<Deployable>(out var deployable))
            {
                result = deployable.ownerMaster.GetBody();
            }

            return result.AsValidOrNull() ? result : _targetingAndPredictionController.GetTargetBody();
        }
    }
}