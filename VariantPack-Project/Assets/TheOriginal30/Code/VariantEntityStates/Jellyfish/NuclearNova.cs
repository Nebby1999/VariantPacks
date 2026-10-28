using RoR2;
using UnityEngine;

namespace EntityStates.JellyfishMonster.Nuclear
{
    public class NuclearNova : BaseState
    {
        public static float baseDuration = 1.5f;
        public static float novaRadius = 24;
        public static float novaForce = 25000;

        private bool hasExploded;
        private float duration;
        private float stopwatch;

        private GameObject chargeEffect;
        private PrintController printController;
        private uint soundID;

        public override void OnEnter()
        {
            base.OnEnter();
            stopwatch = 0f;
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();

            PlayCrossfade("Body", "Nova", "Nova.playbackRate", duration, 0.1f);
            soundID = Util.PlaySound(JellyNova.chargingSoundString, gameObject);

            if (JellyNova.chargingEffectPrefab)
            {
                chargeEffect = Object.Instantiate<GameObject>(JellyNova.chargingEffectPrefab, transform.position, transform.rotation);
                chargeEffect.transform.parent = transform;
                chargeEffect.transform.localScale = Vector3.one * novaRadius;
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
                    scale = novaRadius
                }, true);
            }

            new BlastAttack
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                baseDamage = damageStat * JellyNova.novaDamageCoefficient,
                baseForce = novaForce,
                position = transform.position,
                radius = novaRadius,
                procCoefficient = 2f,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                damageType = DamageType.CrippleOnHit
            }.Fire();

            if (healthComponent) healthComponent.Suicide(null, null, DamageType.Generic);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}