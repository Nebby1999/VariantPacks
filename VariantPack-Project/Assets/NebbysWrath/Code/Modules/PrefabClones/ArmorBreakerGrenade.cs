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
            var selfRequest = NWAssets.LoadAssetAsync<Material>("matADShroom");
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectile.prefab");

            ParallelCoroutine routine = new ParallelCoroutine();
            routine.Add(selfRequest);
            routine.Add(request);

            while (!routine.IsDone())
                yield return null;

            projectile = PrefabAPI.InstantiateClone(request.Result, "ArmorBreakerGrenade", true);
            projectile.AddComponent<ClonedPrefabBehaviour>();

            var projectileController = projectile.GetComponent<ProjectileController>();
            _projectileGhost = projectileController.ghostPrefab.InstantiateClone("HealingGrenadeGhost");
            _projectileGhost.GetComponentInChildren<MeshRenderer>().material = selfRequest.asset;
            projectileController.ghostPrefab = _projectileGhost;

            var impactExplosion = projectile.GetComponent<ProjectileImpactExplosion>();
            _childProjectile = impactExplosion.childrenProjectilePrefab.InstantiateClone("BreakerWard", true);
            impactExplosion.childrenProjectilePrefab = _childProjectile;

        }

        public void Initialize()
        {
            projectile.AddComponent<ModdedDamageTypeHolderComponent>().Add(DamageTypes.PulverizeOnHit.pulverizeOnHit);
            _childProjectile.AddComponent<ModdedDamageTypeHolderComponent>().Add(DamageTypes.PulverizeOnHit.pulverizeOnHit);
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.Add(new GameObject[] { projectile, _childProjectile });
        }
    }
}