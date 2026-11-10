using BepInEx.Configuration;
using NW.PrefabClones;
using RoR2;
using RoR2.AddressableAssets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Gup.Explosive
{
    public class ExplosiveGupAttachmentBaseState : BaseState
    {
        protected NetworkedBodyAttachment networkedBodyAttachment { get; private set; }

        protected HealthComponent attachedHealthComponent { get; private set; }

        protected CharacterModel attachedCharacterModel { get; private set; }

        protected Transform[] displays { get; private set; } = Array.Empty<Transform>();

        public override void OnEnter()
        {
            base.OnEnter();
            networkedBodyAttachment = GetComponent<NetworkedBodyAttachment>();
            if (!networkedBodyAttachment || !networkedBodyAttachment.attachedBody)
            {
                return;
            }
            attachedHealthComponent = networkedBodyAttachment.attachedBody.healthComponent;
            ModelLocator modelLocator = networkedBodyAttachment.attachedBody.modelLocator;
            if ((bool)modelLocator)
            {
                Transform modelTransform = modelLocator.modelTransform;
                if ((bool)modelTransform)
                {
                    attachedCharacterModel = modelTransform.GetComponent<CharacterModel>();
                }
            }
        }
    }

    public class Monitor : ExplosiveGupAttachmentBaseState
    {
        private float previousHealthFraction;

        private static readonly float healthFractionDetonationThreshold = 0.5f;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active)
            {
                FixedUpdateServer();
            }
        }

        private void FixedUpdateServer()
        {
            if ((bool)base.attachedHealthComponent)
            {
                float combinedHealthFraction = base.attachedHealthComponent.combinedHealthFraction;
                if (combinedHealthFraction <= healthFractionDetonationThreshold && healthFractionDetonationThreshold < previousHealthFraction)
                {
                    outer.SetNextState(new CountDown());
                }
                previousHealthFraction = combinedHealthFraction;
            }
        }
    }

    public class CountDown : ExplosiveGupAttachmentBaseState
    {
        public static float duration => EntityStates.QuestVolatileBattery.CountDown.duration * 1.5f;

        public static GameObject vfxPrefab => ExplosiveGupPreDetonation.explosiveVFX;

        public static float explosionRadius => EntityStates.QuestVolatileBattery.CountDown.explosionRadius;

        public static GameObject explosionEffectPrefab => QuestVolatileBattery.CountDown.explosionEffectPrefab;

        private GameObject vfxInstance;
        private GameObject areaIndicatorInstance;

        private bool detonated;

        public override void OnEnter()
        {
            base.OnEnter();
            if (!vfxPrefab || !base.attachedCharacterModel)
            {
                return;
            }

            Transform rootTransform = GetVFXTransformParent();
            vfxInstance = Instantiate(vfxPrefab, rootTransform);
            vfxInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            areaIndicatorInstance = Instantiate(ExplosiveGupPreDetonation.areaIndicator);
            areaIndicatorInstance.transform.SetParent(rootTransform, true); //Keeps the scale accurate
            areaIndicatorInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private Transform GetVFXTransformParent()
        {
            if(!attachedCharacterModel)
            {
                return networkedBodyAttachment.attachedBody.transform;
            }

            foreach(var customActivation in attachedCharacterModel.customGameObjectActivationTransforms)
            {
                if(customActivation.name.StartsWith("DisplayBatteryArray") && customActivation.TryGetComponent<AddressablePrefabInstance>(out _))
                {
                    return customActivation.transform;
                }
            }

            return attachedCharacterModel.transform;
        }

        public override void OnExit()
        {
            Destroy(vfxInstance);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active)
            {
                FixedUpdateServer();
            }
        }

        private void FixedUpdateServer()
        {
            if (base.fixedAge >= duration && !detonated)
            {
                detonated = true;
                Detonate();
            }
        }

        public void Detonate()
        {
            if ((bool)base.networkedBodyAttachment.attachedBody && networkedBodyAttachment.attachedBody.healthComponent && networkedBodyAttachment.attachedBody.healthComponent.alive)
            {
                Vector3 corePosition = base.networkedBodyAttachment.attachedBody.corePosition;
                float baseDamage = 0f;
                if ((bool)base.attachedHealthComponent)
                {
                    baseDamage = base.attachedHealthComponent.health;
                }
                EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
                {
                    origin = corePosition,
                    scale = explosionRadius
                }, transmit: true);
                BlastAttack blastAttack = new BlastAttack
                {
                    position = corePosition + UnityEngine.Random.onUnitSphere,
                    radius = explosionRadius,
                    falloffModel = BlastAttack.FalloffModel.SweetSpot,
                    attacker = base.networkedBodyAttachment.attachedBodyObject,
                    inflictor = base.networkedBodyAttachment.gameObject,
                    damageColorIndex = DamageColorIndex.Item,
                    baseDamage = baseDamage,
                    baseForce = 5000f,
                    bonusForce = Vector3.zero,
                    attackerFiltering = AttackerFiltering.AlwaysHit,
                    crit = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 0f,
                    teamIndex = base.networkedBodyAttachment.attachedBody.teamComponent.teamIndex
                };
                blastAttack.Fire();
                outer.SetNextState(new Idle());

                networkedBodyAttachment.attachedBody.healthComponent.Suicide();
            }
        }
    }
}