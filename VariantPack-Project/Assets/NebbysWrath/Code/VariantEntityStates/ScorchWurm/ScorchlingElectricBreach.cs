using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.CharacterAI;

namespace EntityStates.Scorchling.Electric
{
    public class ScorchlingElectricBreach : BaseState
    {
        public static float crackToBreachTime = 1f;
        public float breachToBurrow = 5f;
        public static float burrowToEndOfTime = 1f;
        public static float animDuration = 1f;
        public static float blastProcCoefficient;
        public static float blastDamageCoefficient;
        public static float blastForce;
        public static Vector3 blastBonusForce;
        public static float knockbackForce;
        public static GameObject crackEffectPrefab;
        public static float crackRadius = 1f;
        public static GameObject blastEffectPrefab;
        public static GameObject blastImpactEffectPrefab;
        public static float blastRadius = 1f;
        public static GameObject burrowEffectPrefab;
        public static float burrowRadius = 1f;
        public static string preBreachSoundString;
        public static string breachSoundString;
        public static string burrowSoundString;
        public static string burrowLoopSoundString;
        public static string burrowStopLoopSoundString;
        private static bool _gotOrig;

        public bool proceedImmediatelyToElectricBomb;

        private bool breached;

        private bool burrowed;

        private bool amServer;

        private Vector3 breachPosition;

        private ScorchlingController scorchlingController;

        private CharacterBody enemyCBody;

        public override void OnEnter()
        {
            if(!_gotOrig)
            {
                _gotOrig = true;
                var orig = new ScorchlingBreach();

                crackToBreachTime = orig.crackToBreachTime;
                burrowToEndOfTime = orig.burrowToEndOfTime;
                animDuration = orig.animDuration;
                blastProcCoefficient = orig.blastProcCoefficient;
                blastDamageCoefficient = orig.blastDamageCoefficient;
                blastForce = orig.blastForce;
                blastBonusForce = orig.blastBonusForce;
                knockbackForce = orig.knockbackForce;
                crackEffectPrefab = orig.crackEffectPrefab;
                crackRadius = orig.crackRadius;
                blastEffectPrefab = orig.blastEffectPrefab;
                blastImpactEffectPrefab = orig.blastImpactEffectPrefab;
                blastRadius = orig.blastRadius;
                burrowEffectPrefab = orig.burrowEffectPrefab;
                burrowRadius = orig.burrowRadius;
                preBreachSoundString = orig.preBreachSoundString;
                breachSoundString = orig.breachSoundString;
                burrowSoundString = orig.burrowSoundString;
                burrowLoopSoundString = orig.burrowLoopSoundString;
                burrowStopLoopSoundString = orig.burrowStopLoopSoundString;
            }
            base.OnEnter();
            amServer = NetworkServer.active;
            scorchlingController = base.characterBody.GetComponent<ScorchlingController>();
            Util.PlaySound(preBreachSoundString, base.gameObject);
            if (amServer)
            {
                enemyCBody = base.characterBody.master.GetComponent<BaseAI>().currentEnemy?.characterBody;
                if (proceedImmediatelyToElectricBomb)
                {
                    breachToBurrow = 1f;
                }
                breachToBurrow += crackToBreachTime;
                burrowToEndOfTime += breachToBurrow;
                breachPosition = base.characterBody.footPosition;
                if (!proceedImmediatelyToElectricBomb && (bool)enemyCBody)
                {
                    breachPosition = enemyCBody.footPosition;
                }
                if ((bool)base.characterMotor)
                {
                    base.characterMotor.walkSpeedPenaltyCoefficient = 0f;
                }
                base.characterBody.SetAimTimer(breachToBurrow);
                TeleportHelper.TeleportBody(base.characterBody, breachPosition);
                EffectManager.SpawnEffect(crackEffectPrefab, new EffectData
                {
                    origin = breachPosition,
                    scale = crackRadius
                }, transmit: true);
                scorchlingController.SetTeleportPermission(b: false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (amServer && base.fixedAge < crackToBreachTime && enemyCBody != null)
            {
                Vector3 normalized = (enemyCBody.corePosition - base.characterBody.corePosition).normalized;
                base.characterBody.characterDirection.forward = normalized;
            }
            if (!breached && base.fixedAge > crackToBreachTime)
            {
                breached = true;
                scorchlingController.Breach();
                PlayAnimation("FullBody, Override", "Breach", "Breach.playbackRate", animDuration);
                Util.PlaySound(breachSoundString, base.gameObject);
                Util.PlaySound(burrowStopLoopSoundString, base.gameObject);
                if (amServer)
                {
                    DetonateAuthority();
                }
            }
            if (base.fixedAge > burrowToEndOfTime)
            {
                DoExit();
            }
        }

        private void DoExit()
        {
            if (proceedImmediatelyToElectricBomb)
            {
                outer.SetNextState(new ScorchlingElectricBomb());
            }
            else
            {
                outer.SetNextStateToMain();
            }
        }

        protected BlastAttack.Result DetonateAuthority()
        {
            EffectManager.SpawnEffect(blastEffectPrefab, new EffectData
            {
                origin = breachPosition,
                scale = blastRadius
            }, transmit: true);
            return new BlastAttack
            {
                attacker = base.gameObject,
                baseDamage = damageStat * blastDamageCoefficient,
                baseForce = blastForce,
                bonusForce = blastBonusForce,
                crit = RollCrit(),
                damageType = DamageType.Stun1s,
                falloffModel = BlastAttack.FalloffModel.None,
                procCoefficient = blastProcCoefficient,
                radius = blastRadius,
                position = breachPosition,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                impactEffect = EffectCatalog.FindEffectIndexFromPrefab(blastImpactEffectPrefab),
                teamIndex = base.teamComponent.teamIndex
            }.Fire();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (proceedImmediatelyToElectricBomb)
            {
                return InterruptPriority.Frozen;
            }
            return InterruptPriority.PrioritySkill;
        }
    }
}