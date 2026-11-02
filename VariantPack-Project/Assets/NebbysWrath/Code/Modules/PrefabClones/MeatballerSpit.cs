using MSU;
using NW.Modules;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NW.PrefabClones
{
    public sealed class MeatballerSpit : IClonedPrefabContentPiece, IContentPackModifier
    {
        public ClonedPrefabBehaviour component => throw new NotImplementedException();

        public GameObject asset => projectilePrefab;
        public static GameObject projectilePrefab;
        private static GameObject _dotZone;

        public void Initialize()
        {
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC2_Scorchling.ScorchlingBombProjectile_prefab);

            var routine = ParallelCoroutine.RequestFromPool();

            routine.Add(projectileRequest);

            while (!routine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(routine);

            projectilePrefab = projectileRequest.Result.InstantiateClone("MeatballBombProjectile", true);

            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<HealthComponent>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<CharacterBody>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<TeamComponent>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<SkillLocator>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<DisableCollisionsBetweenColliders>());
            UnityEngine.Object.Destroy(projectilePrefab.GetComponent<ModelLocator>());
            UnityEngine.Object.Destroy(projectilePrefab.transform.GetChild(0).gameObject);

            if(projectilePrefab.TryGetComponent<ProjectileImpactExplosion>(out var impactExplosion))
            {
                impactExplosion.childrenProjectilePrefab = impactExplosion.childrenProjectilePrefab.InstantiateClone("MeatballBombDOTZone", true);
                _dotZone = impactExplosion.childrenProjectilePrefab;
                if(impactExplosion.childrenProjectilePrefab.TryGetComponent<ProjectileDotZone>(out var dotZone))
                {
                    dotZone.forceVector = Vector3.zero;
                }
            }
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(projectilePrefab);
            contentPack.projectilePrefabs.AddSingle(_dotZone);
        }
    }
}