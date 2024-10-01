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
            var preppedRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/PreppedBellBall.prefab");
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBall.prefab");
            var materialRequest = NWAssets.LoadAssetAsync<Material>("matSteelContraption");

            var routine = new ParallelCoroutine();
            routine.Add(preppedRequest);
            routine.Add(projectileRequest);
            routine.Add(materialRequest);

            while (!routine.IsDone())
                yield return null;

            projectilePrefab = projectileRequest.Result.InstantiateClone("SteelBall", true);
            preppedBall = preppedRequest.Result.InstantiateClone("PreppedSteelBall");
            preppedBall.transform.localScale *= 4;
            preppedBall.GetComponentInChildren<MeshRenderer>().sharedMaterial = materialRequest.asset;
            MSUtil.DestroyImmediateSafe(preppedBall.GetComponent<PrintController>());

            projectilePrefab.transform.localScale *= 4;
            ProjectileController controller = projectilePrefab.GetComponent<ProjectileController>();
            controller.ghostPrefab = controller.ghostPrefab.InstantiateClone("SteelBallGhost");
            controller.ghostPrefab.transform.localScale *= 4;
            controller.ghostPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterial = materialRequest.asset;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(projectilePrefab);
        }
    }
}