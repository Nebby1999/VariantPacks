using EntityStates.LemurianMonster.Badass;
using HG;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using TO30;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VAPI;

namespace EntityStates.LemurianMonster.Flamethrower
{
    public class FireFlamethrower : BaseState
    {
        private static bool _initializedFromMageFlamethrower;
        private static float baseFlamethrowerDuration;
        private static float tickFrequency;

        public static GameObject flameThrowerEffectPrefab;
        public static float maxDistance = 40f;

        private float tickDamageCoefficient;
        private float flamethrowerStopwatch;
        private float stopwatch;
        private float entryDuration;
        private float flamethrowerDuration;
        private bool hasBegunFlamethrower;
        private ChildLocator childLocator;
        private Transform flamethrowerTransform;
        private Transform mouthMuzzle;
        private bool isBadass;
        private const float flamethrowerEffectBaseDistance = 16f;

        [SystemInitializer]
        private static void Initialize()
        {
            flameThrowerEffectPrefab = R2API.PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/DroneFlamethrowerEffect.prefab").WaitForCompletion(), "FlamethrowerLemurianEffect", false);
            Destroy(flameThrowerEffectPrefab.GetComponent<DestroyOnTimer>());
            var fireForwardParticleSystem = flameThrowerEffectPrefab.GetComponentInChildren<ParticleSystem>();
            var main = fireForwardParticleSystem.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 70;

            var emission = fireForwardParticleSystem.emission;
            emission.rateOverTime = 80;
        }
        private void InitFromMage()
        {
            if (_initializedFromMageFlamethrower)
                return;

            _initializedFromMageFlamethrower = true;
            var mageFlamethrower = new Mage.Weapon.Flamethrower();

            baseFlamethrowerDuration = mageFlamethrower.baseFlamethrowerDuration;
            tickFrequency = mageFlamethrower.tickFrequency;
        }
        public override void OnEnter()
        {
            InitFromMage();
            base.OnEnter();
            entryDuration = Mage.Weapon.Flamethrower.baseEntryDuration / attackSpeedStat;
            flamethrowerDuration = baseFlamethrowerDuration;
            Transform modelTransform = GetModelTransform();
            if (characterBody)
            {
                characterBody.SetAimTimer(entryDuration + flamethrowerDuration + 1f);
            }
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
                mouthMuzzle = childLocator.FindChild("MuzzleMouth");
            }
            int num = Mathf.CeilToInt(flamethrowerDuration * tickFrequency);
            tickDamageCoefficient = 0.5f / num;
            PlayAnimation("Gesture", "ChargeFireball", "ChargeFireball.playbackRate", entryDuration);

            //Lore accurate badass flamethrower lemurian
            if(characterBody.TryGetComponent<CharacterBodyVariantController>(out var variantController))
            {
                var variantDefsApplied = variantController.GetCharacterVariantDefs();

                //Retrieve the dependant variant defs
                List<CharacterVariantDef> badassDefs = CharacterVariantCatalog.RetrieveDependantSpawnVariants(TO30Content.CharacterVariants.BadassLemurian);
                badassDefs.Add(TO30Content.CharacterVariants.BadassLemurian); //add the basic one

                foreach(var badassDef in badassDefs)
                {
                    if(variantDefsApplied.Contains(badassDef))
                    {
                        TO30Log.Message($"Badass Flamethrower Lemurian Working as intended");
                        isBadass = true;
                    }
                }

                ListPool<CharacterVariantDef>.ReturnCollection(badassDefs);
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(Mage.Weapon.Flamethrower.endAttackSoundString, gameObject);
            if (flamethrowerTransform)
            {
                Destroy(flamethrowerTransform.gameObject);
            }
            base.OnExit();
        }

        private void FireGauntlet(string muzzleString)
        {
            Ray aimRay = GetAimRay();
            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0f;
                bulletAttack.damage = tickDamageCoefficient * damageStat;
                bulletAttack.force = Mage.Weapon.Flamethrower.force;
                bulletAttack.muzzleName = muzzleString;
                bulletAttack.hitEffectPrefab = Mage.Weapon.Flamethrower.impactEffectPrefab;
                bulletAttack.isCrit = RollCrit();
                bulletAttack.radius = Mage.Weapon.Flamethrower.radius;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.stopperMask = LayerIndex.world.mask;
                bulletAttack.procCoefficient = Mage.Weapon.Flamethrower.procCoefficientPerTick;
                bulletAttack.maxDistance = maxDistance;
                bulletAttack.smartCollision = true;
                bulletAttack.damageType = (Util.CheckRoll(Mage.Weapon.Flamethrower.ignitePercentChance, characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);
                bulletAttack.Fire();
                if ((bool)characterMotor)
                {
                    characterMotor.ApplyForce(aimRay.direction * (0f - Mage.Weapon.Flamethrower.recoilForce));
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= entryDuration && !hasBegunFlamethrower)
            {
                hasBegunFlamethrower = true;
                Util.PlaySound(Mage.Weapon.Flamethrower.startAttackSoundString, gameObject);
                if ((bool)childLocator)
                {
                    if (mouthMuzzle)
                    {
                        flamethrowerTransform = Object.Instantiate(flameThrowerEffectPrefab, mouthMuzzle).transform;
                        flamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
                    }
                }
                PlayAnimation("Gesture", "FireFireball", "FireFireball.playbackRate", tickFrequency);
                FireGauntlet("MuzzleCenter");
            }
            if (hasBegunFlamethrower)
            {
                flamethrowerStopwatch += Time.deltaTime;
                float num = 1 / tickFrequency / 2;
                if (flamethrowerStopwatch > num)
                {
                    flamethrowerStopwatch -= num;
                    FireGauntlet("MuzzleMouth");
                    if (isBadass)
                    {
                        MissileUtils.FireMissile(characterBody.corePosition, characterBody, default, null, 1.2f * damageStat, RollCrit(), FireFireballAndMissile.missleProjectilePrefab, DamageColorIndex.DeathMark, false);
                    }
                }
                UpdateFlamethrowerEffect();
            }
            if (stopwatch >= flamethrowerDuration + entryDuration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private void UpdateFlamethrowerEffect()
        {
            Ray aimRay = GetAimRay();
            Vector3 direction = aimRay.direction;
            if ((bool)flamethrowerTransform)
            {
                flamethrowerTransform.forward = direction;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
