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
    public class DenseSparkBall : IClonedPrefabContentPiece, IContentPackModifier
    {
        public ClonedPrefabBehaviour component => asset.GetComponent<ClonedPrefabBehaviour>();

        public GameObject asset => denseSparkBall;
        public static GameObject denseSparkBall;

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
            var moonMeshMat = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_bazaar.matLunarInfection_mat);
            var trailMat = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_LunarGolem.matLunarGolemShieldTrails_mat);
            var glowParticles = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_LunarGolem.matLunarGolemMuzzleFlash_mat);
            var orbParticles = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Common_VFX.matJellyfishLightningSphere_mat);

            var coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(projectileRequest);
            coroutine.Add(projectileGhostRequest);
            coroutine.Add(moonMeshMat);
            coroutine.Add(trailMat);
            coroutine.Add(glowParticles);
            coroutine.Add(orbParticles);

            while (!coroutine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);

            denseSparkBall = PrefabAPI.InstantiateClone(projectileRequest.Result, "CollapsingSparkBall", true);
            denseSparkBall.AddComponent<ClonedPrefabBehaviour>();

            var impactExplosion = denseSparkBall.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.bonusBlastForce = new Vector3(0, 10000, 0);
            impactExplosion.canRejectForce = false;

            var controller = denseSparkBall.GetComponent<ProjectileController>();
            controller.ghostPrefabAddress = new AssetReferenceT<GameObject>("");
            var ghostPrefab = PrefabAPI.InstantiateClone(projectileGhostRequest.Result, "CollapsingSparkBallGhost");

            ghostPrefab.GetComponentInChildren<Light>().color = Color.blue;
            ghostPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial = moonMeshMat.Result;
            ghostPrefab.GetComponentInChildren<TrailRenderer>().sharedMaterial = trailMat.Result;
            var systems = ghostPrefab.GetComponentsInChildren<ParticleSystemRenderer>();
            systems[0].sharedMaterial = glowParticles.Result;
            systems[1].sharedMaterial = orbParticles.Result;

            controller.ghostPrefab = ghostPrefab;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(denseSparkBall);
        }
    }
}