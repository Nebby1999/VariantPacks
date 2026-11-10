using RoR2;
using RoR2.AsyncManagement;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Mimic.Stalker
{
    public class TeleportBehind : BaseState
    {
        public float baseDuration = 0.2f;

        [SerializeField]
        public bool scaleWithAttackSpeed = true;

        [SerializeField]
        public float nodeSearchRadiusMin = 0;

        [SerializeField]
        public float nodeSearchRadiusMax = 20;

        [SerializeField]
        public float collisionCheckDistance = 0;
        private float duration;

        private CharacterBody target;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = (scaleWithAttackSpeed ? baseDuration : (baseDuration / attackSpeedStat));
            PlayEnterAnimation();

            CharacterBody deployableBody = GetComponent<Deployable>().ownerMaster.GetBody();
            target = deployableBody ? deployableBody : GetComponent<TargetingAndPredictionController>().GetTargetBody();

            if (!target)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((bool)target && base.fixedAge > duration)
            {
                Teleport();
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            PlayExitAnimation();
        }

        public virtual void PlayEnterAnimation()
        {
        }

        public virtual void PlayExitAnimation()
        {
        }

        public void Teleport()
        {
            Vector3 position = target.corePosition + -target.inputBank.GetAimRay().direction * 40;
            if (FindOffsetPosition(position, out var offsetPosition))
            {
                TeleportHelper.TeleportBody(base.characterBody, offsetPosition, forceOutOfVehicle: true);
            }
        }

        public bool FindOffsetPosition(Vector3 desiredPosition, out Vector3 offsetPosition)
        {
            return FindBestTeleportPosition(desiredPosition, out offsetPosition);
        }

        public bool FindBestTeleportPosition(Vector3 nearPosition, out Vector3 teleportPosition)
        {
            if (!DirectorCore.instance || !SceneInfo.instance)
            {
                teleportPosition = default(Vector3);
                return false;
            }


            NodeGraph nodeGraph = SceneInfo.instance.GetNodeGraph(MapNodeGroup.GraphType.Ground);
            List<NodeGraph.NodeIndex> list = nodeGraph.FindNodesInRange(nearPosition, nodeSearchRadiusMin, nodeSearchRadiusMax, HullMask.Human);
            if (list.Count >= 1)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    NodeGraph.NodeIndex nodeIndex = list[i];
                    if (!DirectorCore.instance.CheckNodeOccupied(nodeGraph, nodeIndex) && nodeGraph.GetNodePosition(nodeIndex, out var position) && (!(collisionCheckDistance >= 0f) || CheckTeleportPositionValid(position)))
                    {
                        teleportPosition = position;
                        return true;
                    }
                }
            }

            teleportPosition = default(Vector3);
            return false;
        }

        public bool CheckTeleportPositionValid(Vector3 telePosition)
        {
            if (Physics.OverlapSphere(telePosition, collisionCheckDistance, LayerIndex.CommonMasks.allCharacterCollisions, QueryTriggerInteraction.Ignore).Length != 0)
            {
                return false;
            }

            return true;
        }
    }
}