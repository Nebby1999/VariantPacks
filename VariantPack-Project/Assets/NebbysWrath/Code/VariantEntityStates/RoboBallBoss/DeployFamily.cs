using NW;
using RoR2;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.RoboBallBoss.Weapon.Swarming
{
    public class DeployFamily : BaseState
    {
        public static float baseDuration;
        public static string attackSoundString;
        public static string summonSoundString;
        public static int maxSummonCount = 3;
        public static int maxSummonCountSuper = 5;
        public static float summonDuration;
        public static string summonMuzzleString;
        public static WeightedSelection<GameObject> weightedMasterSelection = new WeightedSelection<GameObject>();

        [SystemInitializer]
        private static void SystemInit()
        {
            MasterCatalog.availability.CallWhenAvailable(() =>
            {
                weightedMasterSelection.AddChoice(MasterCatalog.FindMasterPrefab("RoboBallMiniMaster"), 1);
                weightedMasterSelection.AddChoice(MasterCatalog.FindMasterPrefab("TankerMaster"), 1f);
                weightedMasterSelection.AddChoice(MasterCatalog.FindMasterPrefab("WorkerUnitMaster"), 1);
                weightedMasterSelection.AddChoice(MasterCatalog.FindMasterPrefab("DefectiveUnitMaster"), 0.25f);
                weightedMasterSelection.AddChoice(MasterCatalog.FindMasterPrefab("ExtractorUnitMaster"), 0.25f);
                weightedMasterSelection.AddChoice(MasterCatalog.FindMasterPrefab("IronHaulerMaster"), 0.25f);
            });
        }

        private Animator animator;
        private Transform modelTransform;
        private ChildLocator childLocator;
        private float duration;
        private float summonInterval;
        private float summonTimer;
        private int summonCount;
        private bool isSummoning;
        private static bool _initFromOrig;

        public override void OnEnter()
        {
            base.OnEnter();
            if(!_initFromOrig)
            {
                _initFromOrig = true;

                baseDuration = DeployMinions.baseDuration;
                attackSoundString = DeployMinions.attackSoundString;
                summonSoundString = DeployMinions.summonSoundString;
                summonDuration = DeployMinions.summonDuration;
                summonMuzzleString = DeployMinions.summonMuzzleString;
            }
            animator = GetModelAnimator();
            modelTransform = GetModelTransform();
            childLocator = modelTransform.GetComponent<ChildLocator>();
            duration = baseDuration;
            PlayCrossfade("Gesture, Additive", "DeployMinions", "DeployMinions.playbackRate", duration, 0.1f);
            Util.PlaySound(attackSoundString, base.gameObject);
            int count = characterBody.bodyIndex == RoR2Content.BodyPrefabs.SuperRoboBallBossBody.bodyIndex ? maxSummonCountSuper : maxSummonCount;
            summonInterval = summonDuration / (float)count;
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
            if (!VariantSpawnManager.instance)
            {
                return;
            }

            var summon = new MasterSummon
            {
                position = childLocator.FindChild(summonMuzzleString).position,
                masterPrefab = weightedMasterSelection.Evaluate(VariantSpawnManager.instance.variantSpawnRng.nextNormalizedFloat),
                summonerBodyObject = characterBody.gameObject,
                preSpawnSetupCallback = (master) => Util.CopyEliteStatus(characterBody.masterObject, master.gameObject, false),
            };

            var variantMasterSummon = new VariantMasterSummon(summon)
            {
                preassignedVariantDefs = null,
                noGoldExpModifications = true,
                noItemReward = true,
                spawnManagerMultiplier = 1.1f
            };
            summon.Perform();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}