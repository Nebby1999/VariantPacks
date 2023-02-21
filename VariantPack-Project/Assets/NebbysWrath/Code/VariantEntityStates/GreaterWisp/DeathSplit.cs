using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VAPI;

namespace EntityStates.GreaterWispMonster.Amalgamated
{
    public class DeathSplit : GenericCharacterDeath
    {
        public static GameObject deathEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GreaterWisp/GreaterWispDeath.prefab").WaitForCompletion();
        public static SpawnCard wispSpawnCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Wisp/cscLesserWisp.asset").WaitForCompletion();
        public static float duration = 2f;
        //This is funny
        public static float fuckYouChance = 1f;

        private GameObject initialEffectInstance;
        private DeathRewards deathRewards;
        private bool funny = false;
        private bool hasSpawnedWisps = false;
        public override void OnEnter()
        {
            base.OnEnter();
            deathRewards = GetComponent<DeathRewards>();
            if(NetworkServer.active)
            {
                funny = Util.CheckRoll(fuckYouChance);
            }
            if (!base.modelLocator)
            {
                return;
            }
            ChildLocator component = base.modelLocator.modelTransform.GetComponent<ChildLocator>();
            if ((bool)component)
            {
                Transform transform = component.FindChild("Mask");
                transform.gameObject.SetActive(value: true);
                transform.GetComponent<AnimateShaderAlpha>().timeMax = duration;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && NetworkServer.active)
            {
                if ((bool)deathEffect)
                {
                    EffectManager.SpawnEffect(deathEffect, new EffectData
                    {
                        origin = base.transform.position
                    }, transmit: true);
                }
                if(funny)
                {
                    SpawnWisps();
                }
                else if(!hasSpawnedWisps)
                {
                    hasSpawnedWisps = true;
                    SpawnWisps();
                }
                EntityState.Destroy(base.gameObject);
            }
        }
        public void SpawnWisps()
        {
            if (NetworkServer.active)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 position = base.characterBody.corePosition + (5 * UnityEngine.Random.insideUnitSphere);

                    VariantDirectorSpawnRequest directorSpawnRequest = new VariantDirectorSpawnRequest(wispSpawnCard, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        minDistance = 0f,
                        maxDistance = 0f,
                        position = position
                    }, RoR2Application.rng);
                    directorSpawnRequest.applyOnStart = false;
                    directorSpawnRequest.teamIndexOverride = teamComponent.teamIndex;
                    directorSpawnRequest.variantDefs = Array.Empty<VariantDef>();
                    if(deathRewards)
                    {
                        directorSpawnRequest.deathRewardsBase = deathRewards;
                        directorSpawnRequest.deathRewardsCoefficient = 0.2f;
                    }

                    GameObject wisp = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                    if (wisp)
                    {
                        wisp.GetComponent<CharacterMaster>().GetBody().AddTimedBuff(RoR2Content.Buffs.Immune, 1);
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)initialEffectInstance)
            {
                EntityState.Destroy(initialEffectInstance);
            }
        }
    }
}