using MSU;
using NW.Modules;
using R2API;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NW.PrefabClones
{
    public class CollapsingSparkBall : IClonedPrefabContentPiece, IContentPackModifier
    {
        public ClonedPrefabBehaviour component => asset.GetComponent<ClonedPrefabBehaviour>();

        public GameObject asset => collapsingSparkBallProjectile;
        public static GameObject collapsingSparkBallProjectile;

        public void Initialize()
        {
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC2_Child.ChildTrackingSparkBall_prefab);
            var projectileGhostRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC2_Child.ChildTrackingSparkBallGhost_prefab);
            var nullifierExplosion = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Nullifier.NullifierDeathBombProjectile_prefab);
            var moonMeshMat = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Common.matVoidDeathBombAreaIndicatorFront_mat);
            var trailMat = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_EliteVoid.matVoidInfestorBillboard_mat);
            var glowParticles = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Nullifier.matNullifierStarParticleOffset_mat);
            var orbParticles = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Nullifier.matNullifierStarParticle_mat);

            var coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(projectileRequest);
            coroutine.Add(projectileGhostRequest);
            coroutine.Add(nullifierExplosion);
            coroutine.Add(moonMeshMat);
            coroutine.Add(trailMat);
            coroutine.Add(glowParticles);
            coroutine.Add(orbParticles);

            while (!coroutine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);

            collapsingSparkBallProjectile = PrefabAPI.InstantiateClone(projectileRequest.Result, "CollapsingSparkBall", true);
            collapsingSparkBallProjectile.AddComponent<ClonedPrefabBehaviour>();

            var impactExplosion = collapsingSparkBallProjectile.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.blastDamageCoefficient = 0;
            impactExplosion.blastProcCoefficient = 0;
            impactExplosion.explosionEffect = null;
            impactExplosion.fireChildren = true;
            impactExplosion.childrenProjectilePrefab = nullifierExplosion.Result;
            impactExplosion.childrenCount = 1;

            var controller = collapsingSparkBallProjectile.GetComponent<ProjectileController>();
            controller.ghostPrefabAddress = new AssetReferenceT<GameObject>("");
            var ghostPrefab = PrefabAPI.InstantiateClone(projectileGhostRequest.Result, "CollapsingSparkBallGhost");

            ghostPrefab.GetComponentInChildren<Light>().color = Color.black;
            ghostPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial = moonMeshMat.Result;
            ghostPrefab.GetComponentInChildren<TrailRenderer>().sharedMaterial = trailMat.Result;
            var systems = ghostPrefab.GetComponentsInChildren<ParticleSystemRenderer>();
            systems[0].sharedMaterial = glowParticles.Result;
            systems[1].sharedMaterial = orbParticles.Result;

            controller.ghostPrefab = ghostPrefab;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(collapsingSparkBallProjectile);
        }
    }
}