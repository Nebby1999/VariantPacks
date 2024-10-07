using MSU;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.Scorchling.Electric
{
    public class ScorchlingElectricBomb : BaseState
    {
        public static float breachToSpitTime = 1f;
        public static float spitToLaunchTime = 0.3f;
        public static float spitToBurrowTime = 5f;
        public static float burrowToEndOfTime = 1f;
        public static float animDurationBreach = 1f;
        public static float animDurationSpit = 1f;
        public static float animDurationBurrow = 1f;
        public static float animDurationPostSpit = 0.75f;
        public static float percentageToFireProjectile = 0.75f;
        public static GameObject burrowEffectPrefab;
        public static float burrowRadius = 1f;
        public static string breachSoundString;
        public static string spitSoundString;
        public static string burrowSoundString;
        public static string burrowLoopSoundString;
        public static string burrowStopLoopSoundString;
        public static GameObject mortarProjectilePrefab;
        public static GameObject mortarMuzzleflashEffect;
        public static int mortarCount;
        public static string mortarMuzzleName;
        public static string mortarSoundString;
        public static float mortarDamageCoefficient;
        public static float timeToTarget = 3f;
        public static float projectileVelocity = 55f;
        public static float minimumDistance;

        private bool spitStarted;
        private bool firedProjectile;
        private bool earlyExit;
        private ScorchlingController sController;
        private bool _gotOrig;

        [AsyncAssetLoad]
        private static IEnumerator Load()
        {
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbProjectile.prefab");

            while (!request.IsDone)
                yield return null;

            mortarProjectilePrefab = request.Result;
        }
        public override void OnEnter()
        {
            if(!_gotOrig)
            {
                var orig = new ScorchlingLavaBomb();
                breachToSpitTime = orig.breachToSpitTime;
                spitToLaunchTime = orig.spitToLaunchTime;
                spitToBurrowTime = orig.spitToBurrowTime;
                burrowToEndOfTime = orig.burrowToEndOfTime;
                animDurationBreach = orig.animDurationBreach;
                animDurationSpit = orig.animDurationSpit;
                animDurationBurrow = orig.animDurationBurrow;
                animDurationPostSpit = orig.animDurationPostSpit;
                percentageToFireProjectile = orig.percentageToFireProjectile;
                burrowEffectPrefab = orig.burrowEffectPrefab;
                burrowRadius = orig.burrowRadius;
                breachSoundString = orig.breachSoundString;
                spitSoundString = orig.spitSoundString;
                burrowSoundString = orig.burrowSoundString;
                burrowLoopSoundString = orig.burrowLoopSoundString;
                burrowStopLoopSoundString = orig.burrowStopLoopSoundString;
                mortarMuzzleflashEffect = ScorchlingLavaBomb.mortarMuzzleflashEffect;
                mortarCount = ScorchlingLavaBomb.mortarCount;
                mortarMuzzleName = ScorchlingLavaBomb.mortarMuzzleName;
                mortarSoundString = ScorchlingLavaBomb.mortarSoundString;
                mortarDamageCoefficient = ScorchlingLavaBomb.mortarDamageCoefficient;
                timeToTarget = ScorchlingLavaBomb.timeToTarget;
                projectileVelocity = ScorchlingLavaBomb.projectileVelocity;
                minimumDistance = ScorchlingLavaBomb.minimumDistance;
            }
            base.OnEnter();
            sController = base.characterBody.GetComponent<ScorchlingController>();
            animDurationBreach = (sController.isBurrowed ? animDurationBreach : 0f);
            spitToBurrowTime += animDurationBreach + animDurationSpit;
            burrowToEndOfTime += spitToBurrowTime;
            if (sController.isBurrowed)
            {
                earlyExit = true;
                if (Util.HasEffectiveAuthority(base.characterBody.networkIdentity))
                {
                    outer.SetNextState(new ScorchlingElectricBreach
                    {
                        proceedImmediatelyToElectricBomb = true,
                        breachToBurrow = breachToSpitTime
                    });
                }
            }
            else
            {
                base.characterBody.SetAimTimer(burrowToEndOfTime);
            }
        }

        public override void FixedUpdate()
        {
            if (earlyExit)
            {
                return;
            }
            base.FixedUpdate();
            if (!spitStarted && base.fixedAge > animDurationBreach)
            {
                spitStarted = true;
                PlayAnimation("FullBody, Override", "Spit", "Spit.playbackRate", animDurationSpit);
            }
            if (spitStarted && !firedProjectile && base.fixedAge > animDurationSpit * percentageToFireProjectile + animDurationBreach)
            {
                firedProjectile = true;
                Util.PlaySound(spitSoundString, base.gameObject);
                EffectManager.SimpleMuzzleFlash(mortarMuzzleflashEffect, base.gameObject, mortarMuzzleName, transmit: false);
                if (base.isAuthority)
                {
                    Spit();
                }
            }
            if (firedProjectile && base.fixedAge > animDurationBreach + animDurationSpit + animDurationPostSpit)
            {
                outer.SetNextStateToMain();
            }
        }

        public void Spit()
        {
            Transform transform = base.characterBody.modelLocator.modelTransform.GetComponent<ChildLocator>().FindChild("MuzzleFire");
            Ray ray = new Ray(transform.position, transform.forward);
            Ray ray2 = new Ray(ray.origin, Vector3.up);
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = ray.origin;
            bullseyeSearch.searchDirection = ray.direction;
            bullseyeSearch.filterByLoS = false;
            bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            if ((bool)base.teamComponent)
            {
                bullseyeSearch.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
            }
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.RefreshCandidates();
            HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
            bool flag = false;
            Vector3 vector = Vector3.zero;
            RaycastHit hitInfo;
            if ((bool)hurtBox)
            {
                vector = hurtBox.transform.position;
                flag = true;
            }
            else if (Physics.Raycast(ray, out hitInfo, 1000f, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
            {
                vector = hitInfo.point;
                flag = true;
            }
            float magnitude = projectileVelocity;
            if (flag)
            {
                Vector3 vector2 = vector - ray2.origin;
                Vector2 vector3 = new Vector2(vector2.x, vector2.z);
                float magnitude2 = vector3.magnitude;
                Vector2 vector4 = vector3 / magnitude2;
                if (magnitude2 < minimumDistance)
                {
                    magnitude2 = minimumDistance;
                }
                float y = Trajectory.CalculateInitialYSpeed(timeToTarget, vector2.y);
                float num = magnitude2 / timeToTarget;
                Vector3 direction = new Vector3(vector4.x * num, y, vector4.y * num);
                magnitude = direction.magnitude;
                ray2.direction = direction;
            }
            Quaternion rotation = Util.QuaternionSafeLookRotation(ray2.direction);
            ProjectileManager.instance.FireProjectile(mortarProjectilePrefab, ray2.origin, rotation, base.gameObject, damageStat * mortarDamageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, magnitude);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}