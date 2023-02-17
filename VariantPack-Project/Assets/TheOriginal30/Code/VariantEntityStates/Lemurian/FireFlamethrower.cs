using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static EntityStates.LemurianMonster.FireFireball;
using static EntityStates.Mage.Weapon.Flamethrower;

namespace EntityStates.LemurianMonster.Flamethrower
{
    public class FireFlamethrower : BaseState
    {
		public static GameObject flameThrowerEffectPrefab;
        public float maxDistance = 40f;

		private float tickDamageCoefficient;
		private float flamethrowerStopwatch;
		private float stopwatch;
		private float entryDuration;
		private float flamethrowerDuration;
		private bool hasBegunFlamethrower;
		private ChildLocator childLocator;
		private Transform flamethrowerTransform;
		private Transform mouthMuzzle;
		private bool isCrit;
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
        public override void OnEnter()
        {
            base.OnEnter();
			entryDuration = baseEntryDuration / attackSpeedStat;
			flamethrowerDuration = baseFlamethrowerDuration;
			Transform modelTransform = GetModelTransform();
			if(characterBody)
            {
				characterBody.SetAimTimer(entryDuration + flamethrowerDuration + 1f);
            }
			if(modelTransform)
            {
				childLocator = modelTransform.GetComponent<ChildLocator>();
				mouthMuzzle = childLocator.FindChild("MuzzleMouth");
            }
			int num = Mathf.CeilToInt(flamethrowerDuration * tickFrequency);
			tickDamageCoefficient = 0.5f / num;
			if (base.isAuthority && (bool)base.characterBody)
			{
				isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			}
			PlayAnimation("Gesture", "ChargeFireball", "ChargeFireball.playbackRate", entryDuration);
		}

        public override void OnExit()
        {
			Util.PlaySound(endAttackSoundString, gameObject);
			if(flamethrowerTransform)
            {
				Destroy(flamethrowerTransform.gameObject);
            }
            base.OnExit();
        }

		private void FireGauntlet(string muzzleString)
        {
			Ray aimRay = GetAimRay();
			if (base.isAuthority)
			{
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.owner = base.gameObject;
				bulletAttack.weapon = base.gameObject;
				bulletAttack.origin = aimRay.origin;
				bulletAttack.aimVector = aimRay.direction;
				bulletAttack.minSpread = 0f;
				bulletAttack.damage = tickDamageCoefficient * damageStat;
				bulletAttack.force = Mage.Weapon.Flamethrower.force;
				bulletAttack.muzzleName = muzzleString;
				bulletAttack.hitEffectPrefab = impactEffectPrefab;
				bulletAttack.isCrit = isCrit;
				bulletAttack.radius = radius;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
				bulletAttack.stopperMask = LayerIndex.world.mask;
				bulletAttack.procCoefficient = procCoefficientPerTick;
				bulletAttack.maxDistance = maxDistance;
				bulletAttack.smartCollision = true;
				bulletAttack.damageType = (Util.CheckRoll(ignitePercentChance, base.characterBody.master) ? DamageType.IgniteOnHit : DamageType.Generic);
				bulletAttack.Fire();
				if ((bool)base.characterMotor)
				{
					base.characterMotor.ApplyForce(aimRay.direction * (0f - recoilForce));
				}
			}
		}

        public override void FixedUpdate()
        {
            base.FixedUpdate();
			stopwatch += Time.fixedDeltaTime;
			if(stopwatch >= entryDuration && !hasBegunFlamethrower)
            {
				hasBegunFlamethrower = true;
				Util.PlaySound(startAttackSoundString, base.gameObject);
				if ((bool)childLocator)
				{
					if(mouthMuzzle)
                    {
						flamethrowerTransform = Object.Instantiate(flameThrowerEffectPrefab, mouthMuzzle).transform;
						flamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
					}
				}
				PlayAnimation("Gesture", "FireFireball", "FireFireball.playbackRate", tickFrequency);
				FireGauntlet("MuzzleCenter");
			}
			if(hasBegunFlamethrower)
            {
				flamethrowerStopwatch += Time.deltaTime;
				float num = 1 / tickFrequency / 2;
				if(flamethrowerStopwatch > num)
                {
					flamethrowerStopwatch -= num;
					FireGauntlet("MuzzleMouth");
                }
				UpdateFlamethrowerEffect();
            }
			if(stopwatch >= flamethrowerDuration + entryDuration && isAuthority)
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
