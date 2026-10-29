using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using EntityStates.BeetleQueenMonster;
using VAPI;

namespace EntityStates.BeetleQueenMonster.Empress
{
    public class BeetleSwarm : BaseState
    {
        public static float baseDuration = SummonEggs.baseDuration;
        public static string attackSoundString = "Play_beetle_queen_attack1";
        public static float randomRadius = SummonEggs.randomRadius;
        public static GameObject spitPrefab = SummonEggs.spitPrefab;
        public static int maxBeetleCount = 10;
        public static float summonBeetleInterval = 0.3f;
        public static SpawnCard beetleSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Beetle/cscBeetle.asset").WaitForCompletion();
        public static CharacterVariantDef[] _beetleVariants = null;

        private Animator animator;
        private Transform modelTransform;
        private ChildLocator childLocator;
        private float duration;
        private float summonBeetleTimer;
        private int beetleSummonCount;
        private bool isSummoning;
        private BullseyeSearch enemySearch;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            modelTransform = GetModelTransform();
            childLocator = modelTransform.GetComponent<ChildLocator>();
            duration = baseDuration;
            PlayCrossfade("Gesture", "SummonEggs", 0.5f);
            Util.PlaySound(attackSoundString, base.gameObject);
            if (NetworkServer.active)
            {
                enemySearch = new BullseyeSearch();
                enemySearch.filterByDistinctEntity = false;
                enemySearch.filterByLoS = false;
                enemySearch.maxDistanceFilter = float.PositiveInfinity;
                enemySearch.minDistanceFilter = 0f;
                enemySearch.minAngleFilter = 0f;
                enemySearch.maxAngleFilter = 180f;
                enemySearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
                enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
                enemySearch.viewer = base.characterBody;
            }
        }

        private void SummonBeetleEgg()
        {
            Vector3 searchOrigin = GetAimRay().origin;
            if ((bool)base.inputBank && base.inputBank.GetAimRaycast(float.PositiveInfinity, out var hitInfo))
            {
                searchOrigin = hitInfo.point;
            }
            if (enemySearch == null)
            {
                return;
            }
            enemySearch.searchOrigin = searchOrigin;
            enemySearch.RefreshCandidates();
            HurtBox hurtBox = enemySearch.GetResults().FirstOrDefault();
            Transform transform = (((bool)hurtBox && (bool)hurtBox.healthComponent) ? hurtBox.healthComponent.body.coreTransform : base.characterBody.coreTransform);
            if ((bool)transform && VariantSpawnManager.instance)
            {
                DirectorSpawnRequest spawnRequest = new DirectorSpawnRequest(beetleSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 3,
                    maxDistance = 20,
                    spawnOnTarget = transform,
                }, VariantSpawnManager.instance.variantSpawnRng)
                {
                    summonerBodyObject = gameObject,
                    onSpawnedServer = (result) => Util.CopyEliteStatus(characterBody.masterObject, result.spawnedInstance, false),
                };

                VariantDirectorSpawnRequest variantRequest = new VariantDirectorSpawnRequest(spawnRequest)
                {
                    variantDefs = new CharacterVariantDef[] { GetRandomVariant(VariantSpawnManager.instance.variantSpawnRng) },
                    supressArrivalMessage = true,
                    noItemReward = true,
                    noGoldExpModifications = true
                };

                DirectorCore.instance.TrySpawnObject(spawnRequest);
            }
        }

        private CharacterVariantDef GetRandomVariant(Xoroshiro128Plus rng)
        {
            if(_beetleVariants == null)
            {
                CharacterVariantProvider variantProvider = CharacterVariantCatalog.GetCharacterVariantProvider(BodyCatalog.FindBodyIndex("BeetleBody"));
                _beetleVariants = new CharacterVariantDef[variantProvider.totalVariantCount];
                for(int i = 0; i < _beetleVariants.Length; i++)
                {
                    _beetleVariants[i] = variantProvider.allVariants[i];
                }
            }
            return _beetleVariants.Length > 0 ? _beetleVariants[rng.RangeInt(0, _beetleVariants.Length)] : null;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool flag = animator.GetFloat("SummonEggs.active") > 0.9f;
            if (flag && !isSummoning)
            {
                string muzzleName = "Mouth";
                EffectManager.SimpleMuzzleFlash(spitPrefab, base.gameObject, muzzleName, transmit: false);
            }
            if (isSummoning)
            {
                summonBeetleTimer += Time.fixedDeltaTime;
                if (NetworkServer.active && summonBeetleTimer > 0f && beetleSummonCount < maxBeetleCount)
                {
                    beetleSummonCount++;
                    summonBeetleTimer -= summonBeetleInterval;
                    SummonBeetleEgg();
                }
            }
            isSummoning = flag;
            if (base.fixedAge >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}