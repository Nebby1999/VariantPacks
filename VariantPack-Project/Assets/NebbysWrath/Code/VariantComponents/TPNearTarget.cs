using MSU.Config;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using VAPI.Legacy;
using VAPI.Legacy.Components;
using static MSU.Config.ConfiguredVariable;

namespace NW.Components
{
    public class TPNearTarget : VariantComponent
    {
        public static float timeBetweenTeleports = 20f;

        private static float _minDistanceRequired = 20f;
        private BaseAI _baseAI;
        private float _timeBetweenTeleports;
        private float _stopwatch;
        private Xoroshiro128Plus _rng;

        public void Start()
        {
            _baseAI = GetComponent<BaseAI>();
            if (NetworkServer.active)
            {
                _rng = new Xoroshiro128Plus(VAPIUtils.GetVariantRNG().nextUlong);
                var half = timeBetweenTeleports / 2;
                var modifier = _rng.RangeFloat(half, half);
                _timeBetweenTeleports = timeBetweenTeleports + (_rng.nextInt % 2 == 1 ? -modifier : modifier);
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;

            if (!_baseAI || !characterBody)
                return;

            _stopwatch += Time.fixedDeltaTime;
            if(_stopwatch > _timeBetweenTeleports)
            {
                _stopwatch -= _timeBetweenTeleports;
                TryTeleport();
            }
        }

        private void TryTeleport()
        {
            var target = TryGetTarget();
            if (!target)
                return;

            if (!SceneInfo.instance)
                return;

            var dist = Vector3.Distance(characterBody.transform.position, target.position);
            if(dist < _minDistanceRequired)
            {
                _stopwatch += _timeBetweenTeleports / 2;
                return;
            }

            var origState = Random.state;
            Random.InitState(VAPIUtils.GetVariantRNG().nextInt);
            var insideUnitCircle = Random.insideUnitCircle;
            Random.state = origState;
            insideUnitCircle *= _rng.RangeFloat(1, 5);
            var pos = target.position + new Vector3(insideUnitCircle.x, 0, insideUnitCircle.y);

            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(pos, HullClassification.Human);
            groundNodes.GetNodePosition(nodeIndex, out pos);

            if (characterBody.characterMotor && characterBody.characterMotor.Motor)
            {
                characterBody.characterMotor.Motor.SetPosition(pos, true);
            }
            else
            {
                characterBody.transform.SetPositionAndRotation(pos, characterBody.transform.rotation);
            }

            TrySetState();
        }

        private Transform TryGetTarget()
        {
            if(_baseAI.customTarget != null && _baseAI.customTarget.gameObject)
            {
                //TP to target
                return _baseAI.customTarget.gameObject.transform;
            }

            var enemiesForAI = GetAllEnemies();
            return enemiesForAI.Length == 0 ? null : enemiesForAI[_rng.RangeInt(0, enemiesForAI.Length)].transform;
        }

        private TeamComponent[] GetAllEnemies()
        {
            List<TeamComponent> enemies = new List<TeamComponent>();
            TeamMask mask = TeamMask.GetEnemyTeams(characterMaster.teamIndex);
            for(TeamIndex i = TeamIndex.None; i < TeamIndex.Count; i++)
            {
                if(mask.HasTeam(i))
                {
                    enemies.AddRange(TeamComponent.GetTeamMembers(i));
                }
            }
            return enemies.ToArray();
        }

        private void TrySetState()
        {
            var bodyStateMachine = EntityStateMachine.FindByCustomName(characterBody.gameObject, "Body");
            if(bodyStateMachine)
            {
                bodyStateMachine.SetNextState(EntityStateCatalog.InstantiateState(ref bodyStateMachine.initialStateType));
            }
        }
    }
}