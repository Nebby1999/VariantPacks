using MSU;
using NW.Modules;
using R2API;
using RoR2;
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

namespace NW.PrefabClones
{
    public class HealerGrenade : IClonedPrefabContentPiece, IContentPackModifier
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
            var selfRequest = NWAssets.LoadAssetAsync<Material>("matHealerShroom");
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectile.prefab");

            var coroutine = new ParallelCoroutine();
            coroutine.Add(selfRequest);
            coroutine.Add(request);

            while (!coroutine.IsDone())
                yield return null;

            projectile = PrefabAPI.InstantiateClone(request.Result, "HealerGrenadeProjectile", true);
            projectile.AddComponent<ClonedPrefabBehaviour>();

            var controller = projectile.GetComponent<ProjectileController>();
            _projectileGhost = controller.ghostPrefab.InstantiateClone("HealingGrenadeGhost");
            _projectileGhost.GetComponentInChildren<MeshRenderer>().material = selfRequest.asset;
            controller.ghostPrefab = _projectileGhost;

            var impactExplosion = projectile.GetComponent<ProjectileImpactExplosion>();
            _childProjectile = impactExplosion.childrenProjectilePrefab.InstantiateClone("HealingWard", true);
            impactExplosion.childrenProjectilePrefab = _childProjectile;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.Add(new GameObject[] { projectile, _childProjectile });
        }

        public void Initialize()
        {
            var healingWard = _childProjectile.AddComponent<HealingWard>();
            healingWard.radius = 15;
            healingWard.interval = 0.5f;
            healingWard.rangeIndicator = projectile.transform;
            healingWard.floorWard = true;
        }
    }
}