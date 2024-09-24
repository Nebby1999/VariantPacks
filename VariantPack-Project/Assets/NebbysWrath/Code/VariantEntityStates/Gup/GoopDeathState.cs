using MSU;
using RoR2;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.Gup.Goop
{
    public class GoopDeath : GenericCharacterDeath
    {
        public static GameObject gupPrefab;
        public static int spawnCount = 2;
        public static float deathDelay = 0.5f;
        public float moneyMultiplier = 0.75f;

        public static float spawnRadiusCoefficient = 0.5f;
        public static GameObject deathEffectPrefab;
        private bool hasDied;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var masterRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Gup/GupMaster.prefab");
            var deathRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Gup/GupExplosion.prefab");

            var routine = new ParallelCoroutine();
            routine.Add(masterRequest);
            routine.Add(deathRequest);

            while (!routine.IsDone())
            {
                yield return new WaitForEndOfFrame();
            }

            gupPrefab = masterRequest.Result;
            deathEffectPrefab = deathRequest.Result;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!(base.fixedAge > deathDelay) || hasDied)
            {
                return;
            }
            hasDied = true;
            if (NetworkServer.active)
            {
                EffectManager.SpawnEffect(deathEffectPrefab, new EffectData
                {
                    origin = base.characterBody.corePosition,
                    scale = base.characterBody.radius
                }, transmit: true);
                if ((bool)gupPrefab && spawnCount > 0 && (ulong)(base.healthComponent.killingDamageType & (DamageType.VoidDeath | DamageType.OutOfBounds)) == 0L)
                {
                    BodySplitter bodySplitter = new BodySplitter();
                    bodySplitter.body = base.characterBody;
                    bodySplitter.masterSummon = new VariantSummon
                    {
                        masterPrefab = gupPrefab,
                        variantDefs = Array.Empty<VariantDef>(),
                        applyOnStart = true,
                        supressRewards = true,
                        ignoreTeamMemberLimit = false,
                        useAmbientLevel = null,
                        teamIndexOverride = null
                    };
                    bodySplitter.count = spawnCount;
                    bodySplitter.splinterInitialVelocityLocal = new Vector3(0f, 20f, 10f);
                    bodySplitter.minSpawnCircleRadius = base.characterBody.radius * spawnRadiusCoefficient;
                    bodySplitter.moneyMultiplier = moneyMultiplier;
                    bodySplitter.Perform();
                }
                DestroyBodyAsapServer();
            }
        }

        public override void OnExit()
        {
            DestroyModel();
            base.OnExit();
        }
    }
}