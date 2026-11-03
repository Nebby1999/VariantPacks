using EntityStates.Scorchling;
using HG;
using MSU;
using NW;
using NW.PrefabClones;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VAPI;

namespace EntityStates.LemurianMonster.Meatballer
{
    public class FireMeatball : BaseState
    {
        public static GameObject effectPrefab = FireFireball.effectPrefab;

        public static float baseDuration = FireFireball.baseDuration;

        public static float damageCoefficient = FireFireball.damageCoefficient;

        public static float force = FireFireball.force;

        public static string attackString = FireFireball.attackString;

        public static float projectileVelocity = 55f;

        public static float minimumDistance;

        public static float timeToTarget = 3f;

        public static float mortarDamageCoefficient = 1;

        private float duration;

        private static bool _gotValues;
        private bool _isBadass;
        private static GameObject missleProjectilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/MissileProjectile");

        public override void OnEnter()
        {
            base.OnEnter();
            if(!_gotValues)
            {
                _gotValues = true;

                projectileVelocity = ScorchlingLavaBomb.projectileVelocity;
                minimumDistance = ScorchlingLavaBomb.minimumDistance;
                timeToTarget = ScorchlingLavaBomb.timeToTarget;
            }
            duration = baseDuration / attackSpeedStat;
            PlayAnimation("Gesture", "FireFireball", "FireFireball.playbackRate", duration);
            Util.PlaySound(attackString, gameObject);
            Ray aimRay = GetAimRay();
            string muzzleName = "MuzzleMouth";

            //Lore accurate badass meatballer lemurian
            if (MSUtil.IsModInstalled(TO30.TO30Main.GUID) && characterBody.TryGetComponent<CharacterBodyVariantController>(out var variantController))
            {
                var variantDefsApplied = variantController.GetCharacterVariantDefs();

                //Retrieve the dependant variant defs
                var badassDef = CharacterVariantCatalog.GetCharacterVariantDef(CharacterVariantCatalog.FindCharacterVariantIndex("BadassLemurian"));
                List<CharacterVariantDef> badassDefs = CharacterVariantCatalog.RetrieveDependantSpawnVariants(badassDef);
                badassDefs.Add(badassDef); //add the basic one

                foreach (var defToCheck in badassDefs)
                {
                    if (variantDefsApplied.Contains(defToCheck))
                    {
                        NWLog.Message($"Badass Meatballer Lemurian Working as intended");
                        _isBadass = true;
                    }
                }

                ListPool<CharacterVariantDef>.ReturnCollection(badassDefs);
            }

            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, gameObject, muzzleName, transmit: false);
            }
            if (isAuthority)
            {
                SpitMeatball(muzzleName);
                if(_isBadass)
                {
                    MissileUtils.FireMissile(characterBody.corePosition, characterBody, default, null, damageStat, RollCrit(), missleProjectilePrefab, DamageColorIndex.DeathMark, false);
                }
            }
        }

        public void SpitMeatball(string muzzleName)
        {
            ChildLocator locator = GetModelChildLocator();
            if (!locator)
                return;

            Transform transform = locator.FindChild(muzzleName);
            Ray ray = GetAimRay();
            Ray ray2 = new Ray(ray.origin, Vector3.up);

            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = ray.origin;
            bullseyeSearch.searchDirection = ray.direction;
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            if ((bool)base.teamComponent)
            {
                bullseyeSearch.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
            }
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.RefreshCandidates();
            HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
            Vector3 vector = Vector3.zero;
            RaycastHit hitInfo;
            if ((bool)hurtBox)
            {
                vector = hurtBox.transform.position;
            }
            else if (Util.CharacterRaycast(gameObject, ray, out hitInfo, 1024, LayerIndex.world.mask | LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                vector = hitInfo.point;
            }
            else
            {
                vector = characterBody.transform.position + (ray.direction * 50);
            }

            float magnitude = projectileVelocity;

            Vector3 vector2 = vector - ray2.origin;
            Vector2 vector3 = new Vector2(vector2.x, vector2.z);
            float magnitude2 = vector3.magnitude;
            Vector2 vector4 = vector3 / magnitude2;
            if (magnitude2 < minimumDistance)
            {
                magnitude2 = minimumDistance;
            }
            float y = Trajectory.CalculateInitialYSpeed(timeToTarget, vector2.y);
            float num = magnitude2 / timeToTarget;
            Vector3 direction = new Vector3(vector4.x * num, y, vector4.y * num);
            magnitude = direction.magnitude;
            ray2.direction = direction;

            Quaternion rotation = Util.QuaternionSafeLookRotation(ray2.direction);
            ProjectileManager.instance.FireProjectile(MeatballerSpit.projectilePrefab, ray2.origin, rotation, base.gameObject, damageStat * mortarDamageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, magnitude);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}