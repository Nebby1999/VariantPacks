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
    public class OmicronProjectile : IClonedPrefabContentPiece, IContentPackModifier
    {
        public ClonedPrefabBehaviour component => asset.GetComponent<ClonedPrefabBehaviour>();

        public GameObject asset => omicronProjectile;
        public static GameObject omicronProjectile;

        public void Initialize()
        {
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Titan.TitanRockProjectile_prefab);
            var projectileGhostRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Titan.TitanRockProjectileGhost_prefab);
            var matRequest1 = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_MajorAndMinorConstruct.matConstructBeamInitial_mat);
            var matRequest2 = Addressables.LoadAssetAsync<Material>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_MajorAndMinorConstruct.matConstructBeamBackdrop_mat);

            var coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(projectileRequest);
            coroutine.Add(projectileGhostRequest);
            coroutine.Add(matRequest1);
            coroutine.Add(matRequest2);

            while (!coroutine.IsDone())
                yield return null;

            ParallelCoroutine.ReturnToPool(coroutine);

            omicronProjectile = PrefabAPI.InstantiateClone(projectileRequest.Result, "OmicronLaser", true);
            omicronProjectile.AddComponent<ClonedPrefabBehaviour>();

            var controller = omicronProjectile.GetComponent<ProjectileController>();
            controller.ghostPrefabAddress = new AssetReferenceT<GameObject>("");
            var ghostPrefab = PrefabAPI.InstantiateClone(projectileGhostRequest.Result, "OmicronLaserGhost");
            var trail = ghostPrefab.GetComponentInChildren<TrailRenderer>();

            var gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[1] { new GradientColorKey { color = Color.white, time = 0 } };
            gradient.alphaKeys = new GradientAlphaKey[1] { new GradientAlphaKey { alpha = 1, time = 0 } };
            trail.colorGradient = gradient;

            trail.sharedMaterials = new Material[2] { matRequest1.Result, matRequest2.Result };
            //matConstructBeamInitial
            //matConstructBeamBackdrop
            controller.ghostPrefab = ghostPrefab;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(omicronProjectile);
        }
    }
}