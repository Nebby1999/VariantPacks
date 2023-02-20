using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NW.PrefabClones
{
    public class HealerGrenade : PrefabCloneBase
    {
        public static GameObject projectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectile.prefab").WaitForCompletion(), "HealerGrenadeProjectile", true);

        public override void Initialize()
        {
            base.Initialize();
            HG.ArrayUtils.ArrayAppend(ref NWContent.Instance.SerializableContentPack.projectilePrefabs, projectile);
            var impactExplosion = projectile.GetComponent<ProjectileImpactExplosion>();
            var healingChild = impactExplosion.childrenProjectilePrefab.InstantiateClone("HealingWard", false);
            HG.ArrayUtils.ArrayAppend(ref NWContent.Instance.SerializableContentPack.projectilePrefabs, healingChild);
            var healingWard = healingChild.AddComponent<HealingWard>();
            healingWard.radius = 15;
            healingWard.interval = 0.5f;
            healingWard.rangeIndicator = projectile.transform;
            healingWard.floorWard = true;
            impactExplosion.childrenProjectilePrefab = healingChild;

            ProjectileController controller = projectile.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "HealingGrenadeGhost", false);
            ghostPrefab.GetComponentInChildren<MeshRenderer>().material = NWAssets.LoadAsset<Material>("matHealerShroom");

            controller.ghostPrefab = ghostPrefab;
        }
    }
}