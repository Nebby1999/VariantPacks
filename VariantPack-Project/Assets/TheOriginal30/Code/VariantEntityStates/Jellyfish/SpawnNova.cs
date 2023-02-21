using EntityStates;
using EntityStates.JellyfishMonster.Nuclear;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.JellyfishMonster.MOAJ
{
    public class SpawnNova : BaseState
    {
        public static float baseDuration = 0.5f;
        public static int jellyCount = 5;
        public static float jellyDropRadius = 5;
        public static GameObject masterPrefab;

        private bool hasExploded;
        private float duration;
        private float stopwatch;

        private GameObject chargeEffect;
        private PrintController printController;
        private uint soundID;
        private DeathRewards deathRewards;
        public override void OnEnter()
        {
            base.OnEnter();
            deathRewards = GetComponent<DeathRewards>();
            this.stopwatch = 0f;
            this.duration = SpawnNova.baseDuration / this.attackSpeedStat;
            masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(characterBody.bodyIndex));
            Transform modelTransform = base.GetModelTransform();

            base.PlayCrossfade("Body", "Nova", "Nova.playbackRate", this.duration, 0.1f);
            this.soundID = Util.PlaySound(EntityStates.JellyfishMonster.JellyNova.chargingSoundString, base.gameObject);

            if (EntityStates.JellyfishMonster.JellyNova.chargingEffectPrefab)
            {
                this.chargeEffect = UnityEngine.Object.Instantiate<GameObject>(EntityStates.JellyfishMonster.JellyNova.chargingEffectPrefab, base.transform.position, base.transform.rotation);
                this.chargeEffect.transform.parent = base.transform;
                this.chargeEffect.transform.localScale = Vector3.one * NuclearNova.novaRadius;
                this.chargeEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = this.duration;
            }

            if (modelTransform)
            {
                this.printController = modelTransform.GetComponent<PrintController>();
                if (this.printController)
                {
                    this.printController.enabled = true;
                    this.printController.printTime = this.duration;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (this.chargeEffect) EntityState.Destroy(this.chargeEffect);
            if (this.printController) this.printController.enabled = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            if (this.stopwatch >= this.duration && base.isAuthority && !this.hasExploded)
            {
                this.Detonate();
                return;
            }
        }

        private void Detonate()
        {
            this.hasExploded = true;
            Util.PlaySound(EntityStates.JellyfishMonster.JellyNova.novaSoundString, base.gameObject);

            if (base.modelLocator)
            {
                if (base.modelLocator.modelBaseTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelBaseTransform.gameObject);
                }
                if (base.modelLocator.modelTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelTransform.gameObject);
                }
            }

            if (this.chargeEffect)
            {
                EntityState.Destroy(this.chargeEffect);
            }

            if (EntityStates.JellyfishMonster.JellyNova.novaEffectPrefab)
            {
                EffectManager.SpawnEffect(EntityStates.JellyfishMonster.JellyNova.novaEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    scale = NuclearNova.novaRadius
                }, true);
            }

            if (NetworkServer.active)
            {
                SpawnJellies();
            }

            if (base.healthComponent) base.healthComponent.Suicide(null, null, DamageType.Generic);
        }

        private void SpawnJellies()
        {
            for (int i = 0; i < SpawnNova.jellyCount; i++)
            {
                Vector3 position = base.characterBody.corePosition + (SpawnNova.jellyDropRadius * UnityEngine.Random.insideUnitSphere);

                VariantSummon summon = new VariantSummon
                {
                    applyOnStart = false,
                    variantDefs = Array.Empty<VariantDef>(),
                    useAmbientLevel = false,
                    teamIndexOverride = characterBody.teamComponent.teamIndex,
                    ignoreTeamMemberLimit = true,
                    position = position,
                    masterPrefab = masterPrefab,
                    preSpawnSetupCallback = GiveEquipmentIndex,
                    summonerBodyObject = null
                };

                if(deathRewards)
                {
                    summon.summonerDeathRewards = deathRewards;
                    summon.deathRewardsCoefficient = 0.2f;
                }

                summon.Perform();
            }
        }
        
        private void GiveEquipmentIndex(CharacterMaster master)
        {
            if(characterBody.inventory)
                master.inventory.SetEquipmentIndex(characterBody.inventory.currentEquipmentIndex);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
