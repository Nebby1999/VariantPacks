using RoR2;
using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using MSU;
using UnityEngine.AddressableAssets;
using VAPI.Legacy;
using NW;

namespace EntityStates.MoffeinAncientWispSkills.AncientStoneWisp
{
	public class StoneEnrage : BaseState
	{
		public static float baseDuration = 2.5f;
		public static GameObject enragePrefab;
		private Animator modelAnimator;
		private float duration;
		private bool hasCastBuff;
		private uint soundID;
		private bool stoppedSound = false;
		public static BuffDef enrageBuff;
		public static CharacterSpawnCard minionCard;
		public static VariantDef greaterStoneVariantDef;

		[AsyncAssetLoad]
		public static IEnumerator LoadAsset()
		{
			var effectRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/ExplosionGolemDeath.prefab");
			var enrageRequest = Addressables.LoadAssetAsync<BuffDef>("RoR2/Junk/Common/bdEnrageAncientWisp.asset");
			var greaterWispCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/GreaterWisp/cscGreaterWisp.asset");
			var defRequest = NWAssets.LoadAssetAsync<VariantDef>("GreaterStoneWisp");

			var routine = new ParallelCoroutine();
			routine.Add(enrageRequest);
			routine.Add(greaterWispCard);
			routine.Add(defRequest);
			routine.Add(effectRequest);

			while (!routine.IsDone())
				yield return null;

			minionCard = greaterWispCard.Result;
			enrageBuff = enrageRequest.Result;
			greaterStoneVariantDef = defRequest.asset;
			enragePrefab = effectRequest.Result;
        }
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = StoneEnrage.baseDuration;
			this.modelAnimator = base.GetModelAnimator();
			if (this.modelAnimator)
			{
				base.PlayCrossfade("Gesture", "Enrage", "Enrage.playbackRate", this.duration, 0.2f);
			}
			this.soundID = Util.PlayAttackSpeedSound(VagrantMonster.ChargeMegaNova.chargingSoundString, base.gameObject, this.attackSpeedStat);
			if (NetworkServer.active)
			{
				//base.characterBody.AddBuff(BuffIndex.ArmorBoost);
				base.characterBody.AddBuff(RoR2Content.Buffs.Slow50);
			}
		}

		public override void OnExit()
		{
			if (!stoppedSound)
			{
				AkSoundEngine.StopPlayingID(this.soundID);
			}
			if (NetworkServer.active)
			{
				/*if (base.characterBody.HasBuff(BuffIndex.ArmorBoost))
				{
					base.characterBody.RemoveBuff(BuffIndex.ArmorBoost);
				}*/
				if (base.characterBody.HasBuff(RoR2Content.Buffs.Slow50))
				{
					base.characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
				}
			}
			//Util.PlaySound("Play_MoffeinAW_death", base.gameObject);
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (this.modelAnimator && this.modelAnimator.GetFloat("Enrage.activate") > 0.5f && !this.hasCastBuff)
			{
				AkSoundEngine.StopPlayingID(this.soundID);
				this.hasCastBuff = true;
				stoppedSound = true;
				Util.PlaySound(VagrantMonster.FireMegaNova.novaSoundString, base.gameObject);
				Util.PlaySound("Play_MoffeinAW_lightning", base.gameObject);
				if (NetworkServer.active)
				{
					EffectData effectData = new EffectData();
					effectData.origin = base.transform.position;
					effectData.SetNetworkedObjectReference(base.gameObject);
					EffectManager.SpawnEffect(StoneEnrage.enragePrefab, effectData, true);
					for (int i = 0; i < 2; i++)
					{
						SummonEnemy();
					}

					if (!base.characterBody.HasBuff(enrageBuff))
					{
						base.characterBody.AddBuff(enrageBuff);
					}
				}
			}
			if (base.fixedAge >= this.duration && base.isAuthority && this.hasCastBuff)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		private void SummonEnemy()
		{
			VariantDirectorSpawnRequest directorSpawnRequest = new VariantDirectorSpawnRequest(minionCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				minDistance = 3f,
				maxDistance = 20f,
				spawnOnTarget = base.transform
			}, RoR2Application.rng);
			directorSpawnRequest.summonerBodyObject = base.gameObject;
			VariantDirectorSpawnRequest directorSpawnRequest2 = directorSpawnRequest;
			directorSpawnRequest2.onSpawnedServer = (Action<SpawnCard.SpawnResult>)Delegate.Combine(directorSpawnRequest2.onSpawnedServer, new Action<SpawnCard.SpawnResult>(delegate (SpawnCard.SpawnResult spawnResult)
			{
				spawnResult.spawnedInstance.GetComponent<Inventory>().CopyEquipmentFrom(base.characterBody.inventory);
			}));
			directorSpawnRequest.applyOnStart = true;
			directorSpawnRequest.supressRewards = true;
			directorSpawnRequest.variantDefs = new VariantDef[] { greaterStoneVariantDef };
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}
	}
}