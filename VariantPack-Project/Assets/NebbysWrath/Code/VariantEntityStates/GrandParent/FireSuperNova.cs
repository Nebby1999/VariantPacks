using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using NW;
using UnityEngine.Networking;

namespace EntityStates.GrandParentBoss.Great
{
    public class FireSuperNova : GenericCharacterDeath
    {
        [SerializeField]
        public float timeBeforeDestealth = 2f;

        [SerializeField]
        public float destealthDuration = 0.1f;

        [SerializeField]
        public Material destealthMaterial;

        [SerializeField]
        public GameObject effectPrefab;

        [SerializeField]
        public string effectMuzzleString = "SpawnMuzzle";

        public static GameObject novaEffectPrefab = NWAssets.LoadAsset<GameObject>("SuperNovaexplosion");
        public static GameObject novaImpactEffectPrefab = EntityStates.VagrantMonster.FireMegaNova.novaImpactEffectPrefab;
        public static string novaSoundString = "Play_vagrant_R_explode";
        public static float novaDamageCoefficient = 25;
        public static float novaForce = 5000;
        public float novaRadius;

        private bool destealth;

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
            var deathState = new EntityStates.GrandParentBoss.DeathState();
            destealthMaterial = deathState.destealthMaterial;
            effectPrefab = deathState.effectPrefab;
            base.OnEnter();
            Detonate();

        }

        private void Detonate()
        {
            Vector3 position = base.transform.position;
            Util.PlaySound(novaSoundString, base.gameObject);
            if ((bool)novaEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(novaEffectPrefab, base.gameObject, "NovaCenter", transmit: false);
            }
            Transform modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 3f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = Resources.Load<Material>("Materials/matVagrantEnergized");
                temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());
            }
            if (NetworkServer.active)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = base.gameObject;
                blastAttack.baseDamage = damageStat * novaDamageCoefficient;
                blastAttack.baseForce = novaForce;
                blastAttack.bonusForce = Vector3.zero;
                blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                blastAttack.crit = base.characterBody.RollCrit();
                blastAttack.damageColorIndex = DamageColorIndex.Default;
                blastAttack.damageType = DamageType.Generic;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.inflictor = base.gameObject;
                blastAttack.position = position;
                blastAttack.procChainMask = default(ProcChainMask);
                blastAttack.procCoefficient = 3f;
                blastAttack.radius = novaRadius;
                blastAttack.losType = BlastAttack.LoSType.NearestHit;
                blastAttack.teamIndex = TeamIndex.Neutral;
                blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(novaImpactEffectPrefab);
                blastAttack.Fire();
                var thing = blastAttack.CollectHits();
                List<CharacterBody> hurt = new List<CharacterBody>();
                for (int i = 0; i < thing.Length; i++)
                {
                    var current = thing[i];
                    if (!hurt.Contains(current.hurtBox.healthComponent.body))
                    {
                        hurt.Add(current.hurtBox.healthComponent.body);
                    }
                }
                foreach (CharacterBody body in hurt)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        body.AddTimedBuff(RoR2Content.Buffs.OnFire, 5);
                    }
                }
            }
        }

        public override void OnExit()
        {
            DestroyModel();
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > timeBeforeDestealth && !destealth)
            {
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
