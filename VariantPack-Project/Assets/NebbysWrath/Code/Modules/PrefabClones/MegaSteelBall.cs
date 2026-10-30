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
    public class MegaSteelBall : IClonedPrefabContentPiece, IContentPackModifier
    {
        public ClonedPrefabBehaviour component => throw new NotImplementedException();

        public GameObject asset => projectilePrefab;
        public static GameObject projectilePrefab;
        public static GameObject preppedBall;

        public void Initialize()
        {
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var preppedRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Bell.PreppedBellBall_prefab);
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Bell.BellBall_prefab);
            var projectileGhostRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Bell.BellBallGhost_prefab);
            var materialRequest = NWAssets.LoadAssetAsync<MaterialVariant>("matSteelContraptionBall");

            var routine = ParallelCoroutine.RequestFromPool();

            routine.Add(preppedRequest);
            routine.Add(projectileRequest);
            routine.Add(projectileGhostRequest);
            routine.Add(materialRequest);

            while (!routine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(routine);

            projectilePrefab = projectileRequest.Result.InstantiateClone("SteelBall", true);
            preppedBall = preppedRequest.Result.InstantiateClone("PreppedSteelBall");
            preppedBall.transform.localScale *= 4;
            preppedBall.GetComponentInChildren<MeshRenderer>().sharedMaterial = materialRequest.asset.material;

            projectilePrefab.transform.localScale *= 4;
            ProjectileController controller = projectilePrefab.GetComponent<ProjectileController>();
            controller.ghostPrefabAddress = new AssetReferenceT<GameObject>("");
            controller.ghostPrefab = projectileGhostRequest.Result.InstantiateClone("SteelBallGhost");
            controller.ghostPrefab.transform.localScale *= 4;
            controller.ghostPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial = materialRequest.asset.material;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(projectilePrefab);
        }
    }
}