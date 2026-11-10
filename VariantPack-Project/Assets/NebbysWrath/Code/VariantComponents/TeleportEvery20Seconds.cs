using HG;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = true, useOnServer = true)]
    public class TeleportEvery20Seconds : VariantComponent
    {
        private CharacterBody _characterBody;
        private EntityStateMachine _bodyStateMachine;
        private RangeFloat _timeBetweenTeleports = new RangeFloat
        {
            min = 15f,
            max = 25f
        };
        private bool _scaleWithAttackSpeed = true;

        private Run.FixedTimeStamp timeStampAuthority;
        private bool _targetingAndPredictionControllerAdded;
        private TargetingAndPredictionController _targetingAndPredictionController;

        private void Awake()
        {
            _characterBody = GetComponent<CharacterBody>();
            if(!gameObject.TryGetComponent(out _targetingAndPredictionController))
            {
                _targetingAndPredictionControllerAdded = true;
                gameObject.AddComponent<TargetingAndPredictionController>();
            }
        }

        private void Start()
        {
            _bodyStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            if(Util.HasEffectiveAuthority(gameObject))
            {
                timeStampAuthority = GetTimeForNextTeleport();
            }
        }

        private Run.FixedTimeStamp GetTimeForNextTeleport() => Run.FixedTimeStamp.now + (UnityEngine.Random.Range(_timeBetweenTeleports.min, _timeBetweenTeleports.max) / ((_scaleWithAttackSpeed && _characterBody) ? _characterBody.attackSpeed : 1));

        private void FixedUpdate()
        {
            if(!Util.HasEffectiveAuthority(gameObject))
            {
                return;
            }

            if(timeStampAuthority.hasPassed)
            {
                if(!Util.HasEffectiveAuthority(gameObject))
                {
                    timeStampAuthority = GetTimeForNextTeleport();
                    return;
                }

                if(TrySetTeleportState())
                {
                    timeStampAuthority = GetTimeForNextTeleport();
                }
            }
        }

        private bool TrySetTeleportState()
        {
            if (_bodyStateMachine == null)
                return true;

            if(_bodyStateMachine.CanInterruptState(EntityStates.InterruptPriority.Any))
            {
                _bodyStateMachine.SetNextState(new EntityStates.MinePod.Teleport());
                _bodyStateMachine.StartCoroutine(WaitThenCallInitialState());
                return true;
            }
            return false;
        }

        private IEnumerator WaitThenCallInitialState()
        {
            yield return new WaitForSeconds(0.21f);
            if(_bodyStateMachine)
            {
                _bodyStateMachine.SetNextState(EntityStateCatalog.InstantiateState(_bodyStateMachine.initialStateType.stateType));
            }
        }

        private void OnDestroy()
        {
            if(_targetingAndPredictionController && _targetingAndPredictionControllerAdded)
            {
                Destroy(_targetingAndPredictionController);
                _targetingAndPredictionController = null;
                _targetingAndPredictionControllerAdded = false;
            }
        }
    }
}