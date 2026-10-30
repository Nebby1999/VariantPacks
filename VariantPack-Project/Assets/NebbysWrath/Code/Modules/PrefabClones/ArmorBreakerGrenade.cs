using MSU;
using NW.Modules;
using R2API;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

namespace NW.PrefabClones
{
    public class ArmorBreakerGrenade : IClonedPrefabContentPiece, IContentPackModifier
    {
        public ClonedPrefabBehaviour component => asset.GetComponent<ClonedPrefabBehaviour>();

        public GameObject asset => projectile;
        public static GameObject projectile;
        private GameObject _childProjectile;
        private GameObject _projectileGhost;

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var selfRequest = NWAssets.LoadAssetAsync<MaterialVariant>("matADShroom");
            var sporeGrenadeRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_MiniMushroom.SporeGrenadeProjectile_prefab);
            var sporeGrenadeGhostRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_MiniMushroom.SporeGrenadeGhost_prefab);

            ParallelCoroutine routine = ParallelCoroutine.RequestFromPool();

            routine.Add(selfRequest);
            routine.Add(sporeGrenadeRequest);
            routine.Add(sporeGrenadeGhostRequest);

            while (!routine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(routine);

            projectile = PrefabAPI.InstantiateClone(sporeGrenadeRequest.Result, "ArmorBreakerGrenade", true);
            projectile.AddComponent<ClonedPrefabBehaviour>();
            var projectileController = projectile.GetComponent<ProjectileController>();

            _projectileGhost = sporeGrenadeGhostRequest.Result.InstantiateClone("ArmorBreakerGrenadeGhost", false);
            _projectileGhost.GetComponentInChildren<MeshRenderer>().sharedMaterial = selfRequest.asset.material;
            projectileController.ghostPrefabAddress = new AssetReferenceGameObject("");
            projectileController.ghostPrefab = _projectileGhost;

            var impactExplosion = projectile.GetComponent<ProjectileImpactExplosion>();
            _childProjectile = impactExplosion.childrenProjectilePrefab.InstantiateClone("BreakerWard", true);
            impactExplosion.childrenProjectilePrefab = _childProjectile;

        }

        public void Initialize()
        {
            if (projectile.TryGetComponent<ProjectileDamage>(out var damage))
            {
                damage.damageType.AddModdedDamageType(DamageTypes.PulverizeOnHit.pulverizeOnHit);
            }
            if (_childProjectile.TryGetComponent<ProjectileDamage>(out damage))
            {
                damage.damageType.AddModdedDamageType(DamageTypes.PulverizeOnHit.pulverizeOnHit);
            }
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.Add(new GameObject[] { projectile, _childProjectile });
        }
    }
}