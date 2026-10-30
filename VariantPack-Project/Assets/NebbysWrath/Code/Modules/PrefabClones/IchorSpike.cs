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
    public class IchorSpike : IClonedPrefabContentPiece, IContentPackModifier
    {
        public ClonedPrefabBehaviour component => asset.GetComponent<ClonedPrefabBehaviour>();

        public GameObject asset => ichorSpike;
        public static GameObject ichorSpike;

        public void Initialize()
        {
            if(ichorSpike.TryGetComponent<ProjectileDamage>(out var projectileDamage))
            {
                projectileDamage.damageType.AddModdedDamageType(DamageTypes.PulverizeOnHit.pulverizeOnHit);
            }
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var selfRequest = NWAssets.LoadAssetAsync<MaterialVariant>("matIchorSwipe");
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_ImpBoss.ImpVoidspikeProjectile_prefab);
            var projectileGhostRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_ImpBoss.ImpVoidspikeProjectileGhost_prefab);

            var coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(selfRequest);
            coroutine.Add(projectileRequest);
            coroutine.Add(projectileGhostRequest);

            while (!coroutine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);

            ichorSpike = PrefabAPI.InstantiateClone(projectileRequest.Result, "IchorSpikeProjectile", true);
            ichorSpike.AddComponent<ClonedPrefabBehaviour>();

            var controller = ichorSpike.GetComponent<ProjectileController>();
            controller.ghostPrefabAddress = new AssetReferenceT<GameObject>("");
            var ghostPrefab = PrefabAPI.InstantiateClone(projectileGhostRequest.Result, "IchorSpikeGhost");
            ghostPrefab.GetComponent<Light>().color = new Color(0.98f, 0.71f, 0, 1);
            ghostPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial = selfRequest.asset.material;
            controller.ghostPrefab = ghostPrefab;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(ichorSpike);
        }
    }
}