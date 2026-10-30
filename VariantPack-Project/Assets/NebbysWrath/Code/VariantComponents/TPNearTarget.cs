using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using RoR2.Skills;
using System;
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

        private void Awake()
        {
            _characterBody = GetComponent<CharacterBody>();
        }

        private void Start()
        {
            _bodyStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            timeStamp = GetTimeForNextTeleport();
        }

        private Run.FixedTimeStamp GetTimeForNextTeleport() => Run.FixedTimeStamp.now + (_timeBetweenTeleports / ((_scaleWithAttackSpeed && _characterBody) ? _characterBody.attackSpeed : 1));

        private void FixedUpdate()
        {
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
                return true;
            }
            return false;
        }
    }
}