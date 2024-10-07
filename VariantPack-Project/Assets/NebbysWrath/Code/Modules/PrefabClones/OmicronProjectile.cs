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
            var projectileRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/TitanRockProjectile.prefab");
            var matRequest1 = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/MajorAndMinorConstruct/matConstructBeamInitial.mat");
            var matRequest2 = Addressables.LoadAssetAsync<Material>("RoR2/DLC1/MajorAndMinorConstruct/matConstructBeamBackdrop.mat");

            var coroutine = new ParallelCoroutine();
            coroutine.Add(projectileRequest);
            coroutine.Add(matRequest1);
            coroutine.Add(matRequest2);

            while (!coroutine.IsDone())
                yield return null;

            omicronProjectile = PrefabAPI.InstantiateClone(projectileRequest.Result, "OmicronLaser", true);
            omicronProjectile.AddComponent<ClonedPrefabBehaviour>();

            var controller = omicronProjectile.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "OmicronLaserGhost");
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