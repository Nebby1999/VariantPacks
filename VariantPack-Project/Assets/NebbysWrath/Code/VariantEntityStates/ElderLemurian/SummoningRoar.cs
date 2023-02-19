using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.LemurianBruiserMonster.GhostBrother
{
    public class SummoningRoar : BaseState
    {
        public static float baseEntryDuration = Flamebreath.baseEntryDuration;
        public static float baseExitDuration = Flamebreath.baseExitDuration;
        public static float baseRoarDuration = Flamebreath.baseFlamethrowerDuration;
        public static float tickFrequency = Flamebreath.tickFrequency;
        public static string startAttackSoundString = Flamebreath.startAttackSoundString;
        public static string roarSoundString = EntityStates.MagmaWorm.DeathState.deathSoundString;
        public static string endAttackSoundString = Flamebreath.endAttackSoundString;
        public static SpawnCard lemSpawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Lemurian/cscLemurian.asset").WaitForCompletion();

        private float summonStopwatch;
        private float stopwatch;
        private float entryDuration;
        private float exitDuration;
        private float roarDuration;
        private bool hasBegunRoar;
        private BullseyeSearch enemySearch;

        public override void OnEnter()
        {
            base.OnEnter();
            stopwatch = 0f;
            entryDuration = baseEntryDuration / attackSpeedStat;
            exitDuration = baseExitDuration / attackSpeedStat;
            roarDuration = baseRoarDuration;
            Transform modelTransform = GetModelTransform();
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(entryDuration + roarDuration + 1f);
            }
            PlayAnimation("Gesture, Override", "PrepFlamebreath", "PrepFlamebreath.playbackRate", entryDuration);
            if (NetworkServer.active)
            {
                enemySearch = new BullseyeSearch();
                enemySearch.filterByDistinctEntity = false;
                enemySearch.filterByLoS = false;
                enemySearch.maxDistanceFilter = float.PositiveInfinity;
                enemySearch.minDistanceFilter = 0;
                enemySearch.minAngleFilter = 0;
                enemySearch.maxAngleFilter = 180f;
                enemySearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
                enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
                enemySearch.viewer = base.characterBody;
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(endAttackSoundString, base.gameObject);
            PlayCrossfade("Gesture, Override", "BufferEmpty", 0.1f);
            base.OnExit();
        }

        public void SpawnLemurian()
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
            if ((bool)transform)
            {
                VariantDirectorSpawnRequest directorSpawnRequest = new VariantDirectorSpawnRequest(lemSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = 3f,
                    maxDistance = 20f,
                    spawnOnTarget = transform
                }, RoR2Application.rng);
                directorSpawnRequest.variantDefs = Array.Empty<VariantDef>();
                directorSpawnRequest.applyOnStart = false;
                directorSpawnRequest.supressRewards = true;
                directorSpawnRequest.summonerBodyObject = base.gameObject;
                var lem = DirectorCore.instance?.TrySpawnObject(directorSpawnRequest);
                if (lem)
                {
                    var Inventory = lem.GetComponent<Inventory>();
                    if (Inventory)
                    {
                        Inventory.GiveItemString("HealthDecay", 10);
                        var characterBody = Inventory.GetComponentInParent<CharacterMaster>().GetBody();
                        if (characterBody)
                        {
                            var characterModel = characterBody.modelLocator.modelTransform.GetComponent<CharacterModel>();
                            if (characterModel)
                            {
                                characterModel.baseRendererInfos[0].defaultMaterial = Resources.Load<Material>("Materials/matGhostEffect");
                            }
                        }
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= entryDuration && stopwatch < entryDuration + roarDuration && !hasBegunRoar)
            {
                hasBegunRoar = true;
                Util.PlaySound(roarSoundString, base.gameObject);
                PlayAnimation("Gesture, Override", "Flamebreath", "Flamebreath.playbackRate", roarDuration);
            }
            if (stopwatch >= entryDuration + roarDuration && hasBegunRoar)
            {
                hasBegunRoar = false;
                PlayCrossfade("Gesture, Override", "ExitFlamebreath", "ExitFlamebreath.playbackRate", exitDuration, 0.1f);
            }
            if (hasBegunRoar)
            {
                summonStopwatch += Time.deltaTime;
                if (summonStopwatch > 1f / tickFrequency)
                {
                    summonStopwatch -= 1f / tickFrequency;
                    SpawnLemurian();
                }
            }
            if (stopwatch >= roarDuration + entryDuration + exitDuration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
