using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityStates.RoboBallBoss.Weapon;
using NW;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.RoboBallBoss.Weapon.Swarming
{
    public class DeploySwarm : BaseState
    {
        public static float baseDuration = DeployMinions.baseDuration;
        public static string attackSoundString = DeployMinions.attackSoundString;
        public static string summonSoundString = DeployMinions.summonSoundString;
        public static float maxSummonCount = 5;
        public static float summonDuration = DeployMinions.summonDuration;
        public static string summonMuzzleString = DeployMinions.summonMuzzleString;
        public static GameObject roboBallMiniMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RoboBallBoss/RoboBallMiniMaster.prefab").WaitForCompletion();
        public static VariantDef swarmingDef = NWAssets.LoadAsset<VariantDef>("SwarmerProbe");

        private Animator animator;
        private Transform modelTransform;
        private ChildLocator childLocator;
        private float duration;
        private float summonInterval;
        private float summonTimer;
        private int summonCount;
        private bool isSummoning;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            modelTransform = GetModelTransform();
            childLocator = modelTransform.GetComponent<ChildLocator>();
            duration = baseDuration;
            PlayCrossfade("Gesture, Additive", "DeployMinions", "DeployMinions.playbackRate", duration, 0.1f);
            Util.PlaySound(attackSoundString, base.gameObject);
            summonInterval = summonDuration / (float)maxSummonCount;
        }

        private Transform FindTargetClosest(Vector3 point, TeamIndex enemyTeam)
        {
            ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(enemyTeam);
            float num = 99999f;
            Transform result = null;
            for (int i = 0; i < teamMembers.Count; i++)
            {
                float num2 = Vector3.SqrMagnitude(teamMembers[i].transform.position - point);
                if (num2 < num)
                {
                    num = num2;
                    result = teamMembers[i].transform;
                }
            }
            return result;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = animator.GetFloat("DeployMinions.active") > 0.9f;
            if (isSummoning)
            {
                summonTimer += Time.fixedDeltaTime;
                if (NetworkServer.active && summonTimer > 0f && summonCount < maxSummonCount)
                {
                    summonCount++;
                    summonTimer -= summonInterval;
                    SummonMinion();
                }
            }
            isSummoning = flag;
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void SummonMinion()
        {
            var summon = new VariantSummon
            {
                position = childLocator.FindChild(summonMuzzleString).position,
                masterPrefab = roboBallMiniMaster,
                summonerBodyObject = characterBody.gameObject,
                applyOnStart = true,
                supressRewards = true,
                variantDefs = new VariantDef[] { swarmingDef }
            }.Perform();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}