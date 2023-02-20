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

namespace EntityStates.VagrantMonster.Weapon.Mothership
{
    public class FireJellies : BaseState
    {
        public static float baseDuration = 3f;
        public static SpawnCard JellySpawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Jellyfish/cscJellyfish.asset").WaitForCompletion();
        public static UnityEngine.GameObject SpawnEffectPrefab;

        private float duration;
        private float stopwatch;
        private BullseyeSearch enemySearch;

        public override void OnEnter()
        {
            SpawnEffectPrefab = FireTrackingBomb.muzzleEffectPrefab;
            base.OnEnter();
            stopwatch = 0f;
            duration = baseDuration / 5;
            PlayAnimation("Gesture, Override", "FireTrackingBomb", "FireTrackingBomb.playbackRate", duration);
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
            SpawnJellies();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void SpawnJellies()
        {
            Vector3 searchOrigin = GetAimRay().origin;
            if ((bool)base.inputBank && base.inputBank.GetAimRaycast(float.PositiveInfinity, out var hitinfo))
            {
                searchOrigin = hitinfo.point;
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
                for (int i = 0; i < 5; i++)
                {
                    VariantDirectorSpawnRequest directorSpawnRequest = new VariantDirectorSpawnRequest(JellySpawnCard, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                        minDistance = 3f,
                        maxDistance = 20f,
                        spawnOnTarget = base.transform
                    }, RoR2Application.rng);
                    directorSpawnRequest.applyOnStart = false;
                    directorSpawnRequest.supressRewards = true;
                    directorSpawnRequest.variantDefs = Array.Empty<VariantDef>();
                    directorSpawnRequest.summonerBodyObject = base.gameObject;

                    var jellyMasterObject = DirectorCore.instance?.TrySpawnObject(directorSpawnRequest);
                    if (jellyMasterObject)
                    {
                        CharacterMaster jellyMaster = jellyMasterObject.GetComponent<CharacterMaster>();
                        CharacterBody jellyBody = jellyMaster.GetBody();
                        EffectManager.SimpleMuzzleFlash(SpawnEffectPrefab, jellyBody.gameObject, "TrackingBombMuzzle", transmit: false);
                    }
                }
            }
        }
    }
}