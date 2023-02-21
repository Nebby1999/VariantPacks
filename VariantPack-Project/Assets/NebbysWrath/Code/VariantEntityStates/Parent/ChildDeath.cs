using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.ParentMonster.Child
{
    public class ChildDeath : GenericCharacterDeath
    {
        public float timeBeforeDestealth = 2f;
        public float destealthDuration = 0.1f;
        public Material destealthMaterial;
        public GameObject effectPrefab;
        public string effectMuzzleString = "SlamZone";
        public static Vector3 SpawnPosition;
        public static CharacterSpawnCard parentSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Parent/cscParent.asset").WaitForCompletion();
        public static EntityStates.ParentMonster.DeathState og = new DeathState();
        private bool destealth;
        private DeathRewards deathRewards;

        public override bool shouldAutoDestroy
        {
            get
            {
                if (destealth)
                {
                    return base.fixedAge > timeBeforeDestealth + destealthDuration;
                }
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            SpawnPosition = characterBody.corePosition;
            effectPrefab = og.effectPrefab;
            destealthMaterial = og.destealthMaterial;
            deathRewards = GetComponent<DeathRewards>();
        }

        public override void OnExit()
        {
            DestroyModel();
            base.OnExit();
        }

        private void SpawnParents()
        {
            if (NetworkServer.active)
            {
                for (int i = 0; i < 2; i++)
                {
                    var spawnRequest = new VAPI.VariantDirectorSpawnRequest(parentSpawnCard, new DirectorPlacementRule
                    {
                        maxDistance = 20,
                        minDistance = 3,
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                        spawnOnTarget = transform
                    }, RoR2Application.rng);
                    spawnRequest.applyOnStart = false;
                    spawnRequest.ignoreTeamMemberLimit = true;
                    spawnRequest.teamIndexOverride = teamComponent.teamIndex;
                    spawnRequest.variantDefs = Array.Empty<VAPI.VariantDef>();
                    if(deathRewards)
                    {
                        spawnRequest.deathRewardsBase = deathRewards;
                        spawnRequest.deathRewardsCoefficient = 0.5f;
                    }
                    var parentMaster = DirectorCore.instance.TrySpawnObject(spawnRequest);
                    if (parentMaster)
                    {
                        var parentBody = parentMaster.GetComponent<CharacterMaster>().GetBody();
                        parentBody.AddBuff(RoR2Content.Buffs.WarCryBuff);
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > timeBeforeDestealth && !destealth)
            {
                SpawnParents();
                DoDestealth();
            }
            if (destealth && base.fixedAge > timeBeforeDestealth + destealthDuration)
            {
                DestroyModel();
            }
        }

        private void DoDestealth()
        {
            destealth = true;
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, effectMuzzleString, transmit: false);
            }
            Transform modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                CharacterModel component = modelTransform.gameObject.GetComponent<CharacterModel>();
                if ((bool)destealthMaterial)
                {
                    TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = destealthDuration;
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = destealthMaterial;
                    temporaryOverlay.inspectorCharacterModel = component;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.animateShaderAlpha = true;
                    PrintController component2 = base.modelLocator.modelTransform.gameObject.GetComponent<PrintController>();
                    component2.enabled = false;
                    component2.printTime = destealthDuration;
                    component2.startingPrintHeight = 0f;
                    component2.maxPrintHeight = 20f;
                    component2.startingPrintBias = 0f;
                    component2.maxPrintBias = 2f;
                    component2.disableWhenFinished = false;
                    component2.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    component2.enabled = true;
                }
                Transform transform = FindModelChild("CoreLight");
                if ((bool)transform)
                {
                    transform.gameObject.SetActive(value: false);
                }
            }
        }
    }
}