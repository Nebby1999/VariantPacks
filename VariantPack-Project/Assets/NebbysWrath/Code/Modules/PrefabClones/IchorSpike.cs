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
            var damageTypeComponent = ichorSpike.AddComponent<ModdedDamageTypeHolderComponent>();
            damageTypeComponent.Add(DamageTypes.PulverizeOnHit.pulverizeOnHit);
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var selfRequest = NWAssets.LoadAssetAsync<Material>("matIchorSwipe");
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeProjectile.prefab");

            var coroutine = new ParallelCoroutine();
            coroutine.Add(selfRequest);
            coroutine.Add(request);

            while (!coroutine.IsDone())
                yield return null;

            ichorSpike = PrefabAPI.InstantiateClone(request.Result, "IchorSpikeProjectile", true);
            ichorSpike.AddComponent<ClonedPrefabBehaviour>();

            var controller = ichorSpike.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "IchorSpikeGhost");
            ghostPrefab.GetComponent<Light>().color = new Color(0.98f, 0.71f, 0, 1);
            ghostPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial = selfRequest.asset;
            controller.ghostPrefab = ghostPrefab;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(ichorSpike);
        }
    }
}