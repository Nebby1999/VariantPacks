using R2API;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DamageAPI;

namespace NW.PrefabClones
{
    public class IchorSpike : PrefabCloneBase
    {
        public static GameObject ichorSpike = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpVoidspikeProjectile.prefab").WaitForCompletion(), "IchorSpikeProjectile");

        public override void Initialize()
        {
            HG.ArrayUtils.ArrayAppend(ref NWContent.Instance.SerializableContentPack.projectilePrefabs, ichorSpike);
            var damageComponent = ichorSpike.GetComponent<ProjectileDamage>();
            damageComponent.damageType = RoR2.DamageType.Generic;

            var damageTypeComponent = ichorSpike.AddComponent<ModdedDamageTypeHolderComponent>();
            damageTypeComponent.Add(DamageTypes.PulverizeOnHit.pulverizeOnHit);

            var controller = ichorSpike.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "IchorSpikeGhost", false);
            ghostPrefab.GetComponent<Light>().color = new Color(0.98f, 0.71f, 0, 1);

            var material = NWAssets.LoadAsset<Material>("matIchorClaw");
            var meshRenderer = ghostPrefab.GetComponentInChildren<MeshRenderer>();

            meshRenderer.material = material;
            meshRenderer.sharedMaterial = material;

            controller.ghostPrefab = ghostPrefab;
        }
    }
}