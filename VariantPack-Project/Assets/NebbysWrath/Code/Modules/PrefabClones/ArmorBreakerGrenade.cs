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
    public class ArmorBreakerGrenade : PrefabCloneBase
    {
        public static GameObject projectile = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/MiniMushroom/SporeGrenadeProjectile.prefab").WaitForCompletion(), "ArmorBreakerGrenadeProjectile", true);

        public override void Initialize()
        {
            HG.ArrayUtils.ArrayAppend(ref NWContent.Instance.SerializableContentPack.projectilePrefabs, projectile);
            var impactExplosion = projectile.GetComponent<ProjectileImpactExplosion>();
            var armorBreakerChild = impactExplosion.childrenProjectilePrefab.InstantiateClone("BreakerWard");
            HG.ArrayUtils.ArrayAppend(ref NWContent.Instance.SerializableContentPack.projectilePrefabs, armorBreakerChild);
            var moddedDamageType = armorBreakerChild.AddComponent<ModdedDamageTypeHolderComponent>();
            moddedDamageType.Add(DamageTypes.PulverizeOnHit.pulverizeOnHit);

            impactExplosion.childrenProjectilePrefab = armorBreakerChild;

            ProjectileController controller = projectile.GetComponent<ProjectileController>();
            var ghostPrefab = PrefabAPI.InstantiateClone(controller.ghostPrefab, "HealingGrenadeGhost", false);
            ghostPrefab.GetComponentInChildren<MeshRenderer>().material = NWAssets.LoadAsset<Material>("matADShroom");

            controller.ghostPrefab = ghostPrefab;
        }
    }
}