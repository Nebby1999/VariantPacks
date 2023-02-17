using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NW.Modules.PrefabClones
{
    public class MegaSteelBall : PrefabCloneBase
    {
        public static GameObject PreppedPrefab { get; } = R2API.PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/PreppedBellBall.prefab").WaitForCompletion(), "PreppedSteelBall", false);
        public static GameObject ProjectilePrefab { get; } = R2API.PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bell/BellBall.prefab").WaitForCompletion(), "SteelBall", true);

        public override void Initialize()
        {
            base.Initialize();
            var steelContraptionMat = NWAssets.LoadAsset<Material>("matSteelContraption");
            PreppedPrefab.transform.localScale *= 4;
            PreppedPrefab.GetComponentInChildren<MeshRenderer>().material = steelContraptionMat;

            ProjectilePrefab.transform.localScale *= 4;
            ProjectileController controller = ProjectilePrefab.GetComponent<ProjectileController>();
            var ghostPrefab = R2API.PrefabAPI.InstantiateClone(controller.ghostPrefab, "SteelBallGhost");
            ghostPrefab.transform.localScale *= 4;
            ghostPrefab.GetComponentInChildren<MeshRenderer>().material = steelContraptionMat;
            controller.ghostPrefab = ghostPrefab;
        }
    }
}