using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EntityStates.TitanMonster;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.ArchWispMonster.Stone
{
    public class FireDoubleMegaLaser : BaseState
    {
        [SerializeField]
        public GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/MuzzleflashGolem.prefab").WaitForCompletion();
        [SerializeField]
        public GameObject hitEffectPrefab;
        [SerializeField]
        public GameObject laserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/LaserTitan.prefab").WaitForCompletion();
        public static string playAttackSoundString = FireMegaLaser.playAttackSoundString;
        public static string playLoopSoundString = FireMegaLaser.playLoopSoundString;
        public static string stopLoopSoundString = FireMegaLaser.stopLoopSoundString;
        public static float damageCoefficient = FireMegaLaser.damageCoefficient;
        public static float force = FireMegaLaser.force;
        public static float minSpread = FireMegaLaser.minSpread;
        public static float maxSpread = FireMegaLaser.maxSpread;
        public static int bulletCount = FireMegaLaser.bulletCount;
        public static float fireFrequency = FireMegaLaser.fireFrequency;
        public static float maxDistance = FireMegaLaser.maxDistance;
        public static float minimumDuration = FireMegaLaser.minimumDuration;
        public static float maximumDuration = FireMegaLaser.maximumDuration;
        public static float lockOnAngle = FireMegaLaser.lockOnAngle;
        public static float procCoefficientPerTick = FireMegaLaser.procCoefficientPerTick;

        private float fireStopwatch;
        private float stopwatch;
        private Transform modelTransform;
        //Left Hand Variables
        private BullseyeSearch leftEnemyFinder;
        private Ray leftAimRay;
        protected Transform leftMuzzleTransform;
        private GameObject leftLaserEffect;
        private ChildLocator leftLaserChildLocator;
        private Transform leftLaserEffectEnd;
        private bool leftFoundAnyTarget;
        private HurtBox leftLockedOnHurtBox;
        //Right Hand Variables
        private BullseyeSearch rightEnemyFinder;
        private Ray rightAimRay;
        protected Transform rightMuzzleTransform;
        private GameObject rightLaserEffect;
        private ChildLocator rightLaserChildLocator;
        private Transform rightLaserEffectEnd;
        private bool rightFoundAnyTarget;
        private HurtBox rightLockedOnHurtBox;
        private bool foundAnyTarget;

        public override void OnEnter()
        {
            //Gather original values
            FireMegaLaser titanLaser = new FireMegaLaser();
            effectPrefab = titanLaser.effectPrefab;
            laserPrefab = titanLaser.laserPrefab;
            hitEffectPrefab = titanLaser.hitEffectPrefab;
            playAttackSoundString = FireMegaLaser.playAttackSoundString;
            playLoopSoundString = FireMegaLaser.playLoopSoundString;
            stopLoopSoundString = FireMegaLaser.stopLoopSoundString;
            damageCoefficient = FireMegaLaser.damageCoefficient;
            force = FireMegaLaser.force;
            minSpread = FireMegaLaser.minSpread;
            maxSpread = FireMegaLaser.maxSpread;
            bulletCount = FireMegaLaser.bulletCount;
            fireFrequency = FireMegaLaser.fireFrequency;
            maxDistance = FireMegaLaser.maxDistance;
            minimumDuration = FireMegaLaser.minimumDuration;
            maximumDuration = FireMegaLaser.maximumDuration;
            lockOnAngle = FireMegaLaser.lockOnAngle;
            procCoefficientPerTick = FireMegaLaser.procCoefficientPerTick;

            base.OnEnter();
            base.characterBody.SetAimTimer(maximumDuration);
            Util.PlaySound(playAttackSoundString, base.gameObject);
            Util.PlaySound(playLoopSoundString, base.gameObject);
            //Left Hand
            leftEnemyFinder = new BullseyeSearch();
            leftEnemyFinder.viewer = base.characterBody;
            leftEnemyFinder.maxDistanceFilter = maxDistance;
            leftEnemyFinder.maxAngleFilter = lockOnAngle;
            leftEnemyFinder.searchOrigin = leftAimRay.origin;
            leftEnemyFinder.searchDirection = leftAimRay.direction;
            leftEnemyFinder.filterByLoS = false;
            leftEnemyFinder.sortMode = BullseyeSearch.SortMode.Angle;
            leftEnemyFinder.teamMaskFilter = TeamMask.allButNeutral;
            //Right Hand
            rightEnemyFinder = new BullseyeSearch();
            rightEnemyFinder.viewer = base.characterBody;
            rightEnemyFinder.maxDistanceFilter = maxDistance;
            rightEnemyFinder.maxAngleFilter = lockOnAngle;
            rightEnemyFinder.searchOrigin = rightAimRay.origin;
            rightEnemyFinder.searchDirection = rightAimRay.direction;
            rightEnemyFinder.filterByLoS = false;
            rightEnemyFinder.sortMode = BullseyeSearch.SortMode.Angle;
            rightEnemyFinder.teamMaskFilter = TeamMask.allButNeutral;
            if ((bool)base.teamComponent)
            {
                leftEnemyFinder.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
                rightEnemyFinder.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
            }
            leftAimRay = GetAimRay();
            rightAimRay = GetAimRay();
            modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                ChildLocator leftComponent = modelTransform.GetComponent<ChildLocator>();
                ChildLocator rightComponent = modelTransform.GetComponent<ChildLocator>();
                if ((bool)leftComponent && (bool)rightComponent)
                {
                    leftMuzzleTransform = leftComponent.FindChild("MuzzleLeft");
                    rightMuzzleTransform = rightComponent.FindChild("MuzzleRight");
                    //Left
                    if ((bool)leftMuzzleTransform && (bool)laserPrefab)
                    {
                        leftLaserEffect = Object.Instantiate(laserPrefab, leftMuzzleTransform.position, leftMuzzleTransform.rotation);
                        leftLaserEffect.transform.parent = leftMuzzleTransform;
                        leftLaserChildLocator = leftLaserEffect.GetComponent<ChildLocator>();
                        leftLaserEffectEnd = leftLaserChildLocator.FindChild("LaserEnd");
                    }
                    //right
                    if ((bool)rightMuzzleTransform && (bool)laserPrefab)
                    {
                        rightLaserEffect = Object.Instantiate(laserPrefab, rightMuzzleTransform.position, rightMuzzleTransform.rotation);
                        rightLaserEffect.transform.parent = rightMuzzleTransform;
                        rightLaserChildLocator = rightLaserEffect.GetComponent<ChildLocator>();
                        rightLaserEffectEnd = rightLaserChildLocator.FindChild("LaserEnd");
                    }
                }
            }
            UpdateLockOn();
        }

        public override void OnExit()
        {
            if ((bool)leftLaserEffect)
            {
                EntityState.Destroy(leftLaserEffect);
            }
            if ((bool)rightLaserEffect)
            {
                EntityState.Destroy(rightLaserEffect);
            }
            base.characterBody.SetAimTimer(2f);
            Util.PlaySound(stopLoopSoundString, base.gameObject);
            PlayAnimation("Gesture", "FireCannons", "FireCannons.playbackRate", 2 / base.attackSpeedStat);
            base.OnExit();
        }

        private void UpdateLockOn()
        {
            if (base.isAuthority)
            {
                //Left
                leftEnemyFinder.searchOrigin = leftAimRay.origin;
                leftEnemyFinder.searchDirection = leftAimRay.direction;
                leftEnemyFinder.RefreshCandidates();
                leftFoundAnyTarget = (leftLockedOnHurtBox = leftEnemyFinder.GetResults().FirstOrDefault());
                //Right
                rightEnemyFinder.searchOrigin = rightAimRay.origin;
                rightEnemyFinder.searchDirection = rightAimRay.direction;
                rightEnemyFinder.RefreshCandidates();
                rightFoundAnyTarget = (rightLockedOnHurtBox = rightEnemyFinder.GetResults().FirstOrDefault());
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            fireStopwatch += Time.fixedDeltaTime;
            stopwatch += Time.fixedDeltaTime;
            //Left Hand
            leftAimRay = GetAimRay();
            Vector3 leftVector = leftAimRay.origin;
            if ((bool)leftMuzzleTransform)
            {
                leftVector = leftMuzzleTransform.position;
            }
            RaycastHit leftHitInfo;
            Vector3 leftVector2 = (leftLockedOnHurtBox ? leftLockedOnHurtBox.transform.position : ((!Util.CharacterRaycast(base.gameObject, leftAimRay, out leftHitInfo, maxDistance, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore)) ? leftAimRay.GetPoint(maxDistance) : leftHitInfo.point));
            Ray leftRay = new Ray(leftVector, leftVector2 - leftVector);
            bool leftFlag = false;
            if ((bool)leftLaserEffect && (bool)leftLaserChildLocator)
            {
                if (Util.CharacterRaycast(base.gameObject, leftRay, out var leftHitInfo2, (leftVector2 - leftVector).magnitude, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                {
                    leftVector2 = leftHitInfo2.point;
                    if (Util.CharacterRaycast(base.gameObject, new Ray(leftVector2 - leftRay.direction * 0.1f, -leftRay.direction), out var _, leftHitInfo2.distance, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                    {
                        leftVector2 = leftRay.GetPoint(0.1f);
                        leftFlag = true;
                    }
                }
                leftLaserEffect.transform.rotation = Util.QuaternionSafeLookRotation(leftVector2 - leftVector);
                leftLaserEffectEnd.transform.position = leftVector2;
            }
            //Right Hand
            rightAimRay = GetAimRay();
            Vector3 rightVector = rightAimRay.origin;
            if ((bool)rightMuzzleTransform)
            {
                rightVector = rightMuzzleTransform.position;
            }
            RaycastHit rightHitInfo;
            Vector3 rightVector2 = (rightLockedOnHurtBox ? rightLockedOnHurtBox.transform.position : ((!Util.CharacterRaycast(base.gameObject, rightAimRay, out rightHitInfo, maxDistance, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore)) ? rightAimRay.GetPoint(maxDistance) : rightHitInfo.point));
            Ray rightRay = new Ray(rightVector, rightVector2 - rightVector);
            bool rightFlag = false;
            if ((bool)rightLaserEffect && (bool)rightLaserChildLocator)
            {
                if (Util.CharacterRaycast(base.gameObject, rightRay, out var rightHitInfo2, (rightVector2 - rightVector).magnitude, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                {
                    rightVector2 = rightHitInfo2.point;
                    if (Util.CharacterRaycast(base.gameObject, new Ray(rightVector2 - rightRay.direction * 0.1f, -rightRay.direction), out var _, rightHitInfo2.distance, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                    {
                        rightVector2 = rightRay.GetPoint(0.1f);
                        rightFlag = true;
                    }
                }
                rightLaserEffect.transform.rotation = Util.QuaternionSafeLookRotation(rightVector2 - rightVector);
                rightLaserEffectEnd.transform.position = rightVector2;
            }
            if (fireStopwatch > 1f / fireFrequency)
            {
                string leftTargetMuzzle = "MuzzleLeft";
                string rightTargetMuzzle = "MuzzleRight";
                if (!leftFlag)
                {
                    FireBullet(modelTransform, leftRay, leftTargetMuzzle, (leftVector2 - leftRay.origin).magnitude + 0.1f);
                }
                if (!rightFlag)
                {
                    FireBullet(modelTransform, rightRay, rightTargetMuzzle, (rightVector2 - rightRay.origin).magnitude + 0.1f);
                }
                UpdateLockOn();
                fireStopwatch -= 1f / fireFrequency;
            }
            if (base.isAuthority && (((!base.inputBank || !base.inputBank.skill4.down) && stopwatch > minimumDuration) || stopwatch > maximumDuration))
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        private void FireBullet(Transform modelTransform, Ray aimRay, string targetMuzzle, float maxDistance)
        {
            if ((bool)effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
            }
            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = base.gameObject;
                bulletAttack.weapon = base.gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = minSpread;
                bulletAttack.maxSpread = maxSpread;
                bulletAttack.bulletCount = 1u;
                bulletAttack.damage = damageCoefficient * damageStat / fireFrequency;
                bulletAttack.force = force;
                bulletAttack.muzzleName = targetMuzzle;
                bulletAttack.hitEffectPrefab = hitEffectPrefab;
                bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
                bulletAttack.procCoefficient = procCoefficientPerTick;
                bulletAttack.HitEffectNormal = false;
                bulletAttack.radius = 0f;
                bulletAttack.maxDistance = maxDistance;
                bulletAttack.Fire();
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(HurtBoxReference.FromHurtBox(leftLockedOnHurtBox));
            writer.Write(HurtBoxReference.FromHurtBox(rightLockedOnHurtBox));
            writer.Write(stopwatch);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            HurtBoxReference leftHurtBoxReference = reader.ReadHurtBoxReference();
            HurtBoxReference rightHurtBoxReference = reader.ReadHurtBoxReference();
            stopwatch = reader.ReadSingle();
            leftLockedOnHurtBox = leftHurtBoxReference.ResolveGameObject()?.GetComponent<HurtBox>();
            rightLockedOnHurtBox = rightHurtBoxReference.ResolveGameObject()?.GetComponent<HurtBox>();
        }
    }
}