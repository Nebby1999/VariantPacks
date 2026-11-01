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
        private float _timeBetweenTeleports = 20f;
        private bool _scaleWithAttackSpeed = true;

        private Run.FixedTimeStamp timeStamp;
        private TargetingAndPredictionController _targetingAndPredictionController;

        private void Awake()
        {
            _characterBody = GetComponent<CharacterBody>();
            _targetingAndPredictionController = gameObject.EnsureComponent<TargetingAndPredictionController>();
        }

        private void Start()
        {
            _bodyStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            timeStamp = GetTimeForNextTeleport();
        }

        private Run.FixedTimeStamp GetTimeForNextTeleport() => Run.FixedTimeStamp.now + (_timeBetweenTeleports / ((_scaleWithAttackSpeed && _characterBody) ? _characterBody.attackSpeed : 1));

        private void FixedUpdate()
        {
            if(!Util.HasEffectiveAuthority(gameObject))
            {
                return;
            }

            if(timeStamp.hasPassed)
            {
                if(!Util.HasEffectiveAuthority(gameObject))
                {
                    timeStamp = GetTimeForNextTeleport();
                    return;
                }

                if(TrySetTeleportState())
                {
                    timeStamp = GetTimeForNextTeleport();
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
            if(_targetingAndPredictionController)
            {
                Destroy(_targetingAndPredictionController);
                _targetingAndPredictionController = null;
            }
        }
    }
}