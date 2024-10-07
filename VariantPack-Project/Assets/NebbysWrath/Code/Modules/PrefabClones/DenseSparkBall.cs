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
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Child/ChildTrackingSparkBall.prefab");
            var moonMeshMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/bazaar/matLunarInfection.mat");
            var trailMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarGolem/matLunarGolemShieldTrails.mat");
            var glowParticles = Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarGolem/matLunarGolemMuzzleFlash.mat");
            var orbParticles = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matJellyfishLightningSphere.mat");

            var coroutine = new ParallelCoroutine();
            coroutine.Add(projectileRequest);
            coroutine.Add(moonMeshMat);
            coroutine.Add(trailMat);
            coroutine.Add(glowParticles);
            coroutine.Add(orbParticles);

            while (!coroutine.IsDone())
                yield return null;

            denseSparkBall = PrefabAPI.InstantiateClone(projectileRequest.Result, "CollapsingSparkBall", true);
            denseSparkBall.AddComponent<ClonedPrefabBehaviour>();

            var impactExplosion = denseSparkBall.GetComponent<ProjectileImpactExplosion>();
            impactExplosion.bonusBlastForce = new Vector3(0, 10000, 0);
            impactExplosion.canRejectForce = false;

            var controller = denseSparkBall.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "CollapsingSparkBallGhost");

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