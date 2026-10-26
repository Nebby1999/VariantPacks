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
            stopwatch = 0f;
            duration = baseDuration / attackSpeedStat;
            masterPrefab = MasterCatalog.GetMasterPrefab(MasterCatalog.FindAiMasterIndexForBody(characterBody.bodyIndex));
            Transform modelTransform = GetModelTransform();

            PlayCrossfade("Body", "Nova", "Nova.playbackRate", duration, 0.1f);
            soundID = Util.PlaySound(JellyNova.chargingSoundString, gameObject);

            if (JellyNova.chargingEffectPrefab)
            {
                chargeEffect = UnityEngine.Object.Instantiate<GameObject>(JellyNova.chargingEffectPrefab, transform.position, transform.rotation);
                chargeEffect.transform.parent = transform;
                chargeEffect.transform.localScale = Vector3.one * NuclearNova.novaRadius;
                chargeEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
            }

            if (modelTransform)
            {
                printController = modelTransform.GetComponent<PrintController>();
                if (printController)
                {
                    printController.enabled = true;
                    printController.printTime = duration;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            if (chargeEffect) Destroy(chargeEffect);
            if (printController) printController.enabled = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= duration && isAuthority && !hasExploded)
            {
                Detonate();
                return;
            }
        }

        private void Detonate()
        {
            hasExploded = true;
            Util.PlaySound(JellyNova.novaSoundString, gameObject);

            if (modelLocator)
            {
                if (modelLocator.modelBaseTransform)
                {
                    Destroy(modelLocator.modelBaseTransform.gameObject);
                }
                if (modelLocator.modelTransform)
                {
                    Destroy(modelLocator.modelTransform.gameObject);
                }
            }

            if (chargeEffect)
            {
                Destroy(chargeEffect);
            }

            if (JellyNova.novaEffectPrefab)
            {
                EffectManager.SpawnEffect(JellyNova.novaEffectPrefab, new EffectData
                {
                    origin = transform.position,
                    scale = NuclearNova.novaRadius
                }, true);
            }

            if (NetworkServer.active)
            {
                SpawnJellies();
            }

            if (healthComponent) healthComponent.Suicide(null, null, DamageType.Generic);
        }

        private void SpawnJellies()
        {
            for (int i = 0; i < jellyCount; i++)
            {
                Vector3 position = characterBody.corePosition + (jellyDropRadius * UnityEngine.Random.insideUnitSphere);

                MasterSummon masterSummon = new MasterSummon
                {
                    useAmbientLevel = false,
                    teamIndexOverride = characterBody.teamComponent.teamIndex,
                    ignoreTeamMemberLimit = true,
                    position = position,
                    masterPrefab = masterPrefab,
                    preSpawnSetupCallback = GiveEquipmentIndex,
                    summonerBodyObject = null,
                };

                VariantMasterSummon variantSummon = new VariantMasterSummon(masterSummon)
                {
                    deathRewardsCoefficient = deathRewards ? 0.2f : 1,
                    summonerDeathRewards = deathRewards,
                    variantDefs = Array.Empty<CharacterVariantDef>()
                };

                variantSummon.Perform();
            }
        }

        private void GiveEquipmentIndex(CharacterMaster master)
        {
            if (characterBody.inventory)
                master.inventory.SetEquipmentIndex(characterBody.inventory.currentEquipmentIndex, false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
