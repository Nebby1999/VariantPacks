using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityStates.TitanMonster;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace EntityStates.ArchWispMonster.Stone
{
    public class ChargeDoubleMegaLaser : BaseState
    {
        public static float baseDuration = ChargeMegaLaser.baseDuration;
        public static float laserMaxWidth = ChargeMegaLaser.laserMaxWidth;
        public static GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/ChargeGolem.prefab").WaitForCompletion();
        public static GameObject laserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/LaserGolem.prefab").WaitForCompletion();
        public static string chargeAttackSoundString = ChargeMegaLaser.chargeAttackSoundString;
        public static float lockOnAngle = ChargeMegaLaser.lockOnAngle;

        public float duration;
        private BullseyeSearch leftEnemyFinder;
        private GameObject leftChargeEffectInstance;
        private GameObject leftLaserEffectInstance;
        private LineRenderer leftLaserLineComponent;
        private HurtBox leftLockedOnHurtBox;
        private BullseyeSearch rightEnemyFinder;
        private GameObject rightChargeEffectInstance;
        private GameObject rightLaserEffectInstance;
        private LineRenderer rightLaserLineComponent;
        private HurtBox rightLockedOnHurtBox;
        private Vector3 visualEndPosition;
        private float flashTimer;
        private bool laserOn;
        private const float originalSoundDuration = 2.1f;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            Util.PlayAttackSpeedSound(chargeAttackSoundString, base.gameObject, 2.1f / duration);
            Ray leftAimRay = GetAimRay();
            leftEnemyFinder = new BullseyeSearch();
            leftEnemyFinder.maxDistanceFilter = 2000f;
            leftEnemyFinder.maxAngleFilter = lockOnAngle;
            leftEnemyFinder.searchOrigin = leftAimRay.origin;
            leftEnemyFinder.searchDirection = leftAimRay.direction;
            leftEnemyFinder.filterByLoS = false;
            leftEnemyFinder.sortMode = BullseyeSearch.SortMode.Angle;
            leftEnemyFinder.teamMaskFilter = TeamMask.allButNeutral;
            Ray rightAimRay = GetAimRay();
            rightEnemyFinder = new BullseyeSearch();
            rightEnemyFinder.maxDistanceFilter = 2000f;
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
            GetModelAnimator();
            PlayAnimation("Gesture", "ChargeCannons", "ChargeCannons.playbackRate", duration);
            if ((bool)modelTransform)
            {
                ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                if ((bool)component)
                {
                    Transform leftHandTransform = component.FindChild("MuzzleLeft");
                    Transform rightHandTransform = component.FindChild("MuzzleRight");
                    //Left Hand
                    if ((bool)leftHandTransform)
                    {
                        if ((bool)effectPrefab)
                        {
                            leftChargeEffectInstance = UnityEngine.Object.Instantiate(effectPrefab, leftHandTransform.position, leftHandTransform.rotation);
                            leftChargeEffectInstance.transform.parent = leftHandTransform;
                            ScaleParticleSystemDuration component2 = leftChargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserPrefab)
                        {
                            leftLaserEffectInstance = UnityEngine.Object.Instantiate(laserPrefab, leftHandTransform.position, leftHandTransform.rotation);
                            leftLaserEffectInstance.transform.parent = leftHandTransform;
                            leftLaserLineComponent = leftLaserEffectInstance.GetComponent<LineRenderer>();
                        }
                    }
                    //Right Hand
                    if ((bool)rightHandTransform)
                    {
                        if ((bool)effectPrefab)
                        {
                            rightChargeEffectInstance = UnityEngine.Object.Instantiate(effectPrefab, rightHandTransform.position, rightHandTransform.rotation);
                            rightChargeEffectInstance.transform.parent = rightHandTransform;
                            ScaleParticleSystemDuration component2 = rightChargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                            if ((bool)component2)
                            {
                                component2.newDuration = duration;
                            }
                        }
                        if ((bool)laserPrefab)
                        {
                            rightLaserEffectInstance = UnityEngine.Object.Instantiate(laserPrefab, rightHandTransform.position, rightHandTransform.rotation);
                            rightLaserEffectInstance.transform.parent = rightHandTransform;
                            rightLaserLineComponent = rightLaserEffectInstance.GetComponent<LineRenderer>();
                        }
                    }
                }
            }
            if ((bool)base.characterBody)
            {
                base.characterBody.SetAimTimer(duration);
            }
            flashTimer = 0f;
            laserOn = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            if ((bool)leftChargeEffectInstance)
            {
                EntityState.Destroy(leftChargeEffectInstance);
            }
            if ((bool)leftLaserEffectInstance)
            {
                EntityState.Destroy(leftLaserEffectInstance);
            }
            if ((bool)rightChargeEffectInstance)
            {
                EntityState.Destroy(rightChargeEffectInstance);
            }
            if ((bool)rightLaserEffectInstance)
            {
                EntityState.Destroy(rightLaserEffectInstance);
            }
        }

        public override void Update()
        {
            base.Update();
            if (!(leftLaserEffectInstance && rightLaserEffectInstance) || !(rightLaserLineComponent && leftLaserLineComponent))
            {
                return;
            }
            //Left Laser
            {
                float num = 1000f;
                Ray leftAimRay = GetAimRay();
                leftEnemyFinder.RefreshCandidates();
                leftLockedOnHurtBox = leftEnemyFinder.GetResults().FirstOrDefault();
                if ((bool)leftLockedOnHurtBox)
                {
                    leftAimRay.direction = leftLockedOnHurtBox.transform.position - leftAimRay.origin;
                }
                Vector3 leftPosition = leftLaserEffectInstance.transform.parent.position;
                Vector3 leftPoint = leftAimRay.GetPoint(num);
                if (Physics.Raycast(leftAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask))
                {
                    leftPoint = hitInfo.point;
                }
                leftLaserLineComponent.SetPosition(0, leftPosition);
                leftLaserLineComponent.SetPosition(1, leftPoint);
                float num2;
                if (duration - base.age > 0.5f)
                {
                    num2 = base.age / duration;
                }
                else
                {
                    flashTimer -= Time.deltaTime;
                    if (flashTimer <= 0f)
                    {
                        laserOn = !laserOn;
                        flashTimer = 71f / (678f * (float)Mathf.PI);
                    }
                    num2 = (laserOn ? 1f : 0f);
                }
                num2 *= laserMaxWidth;
                leftLaserLineComponent.startWidth = num2;
                leftLaserLineComponent.endWidth = num2;
            }
            //Right Laser
            {
                float num = 1000f;
                Ray rightAimRay = GetAimRay();
                rightEnemyFinder.RefreshCandidates();
                rightLockedOnHurtBox = rightEnemyFinder.GetResults().FirstOrDefault();
                if ((bool)rightLockedOnHurtBox)
                {
                    rightAimRay.direction = rightLockedOnHurtBox.transform.position - rightAimRay.origin;
                }
                Vector3 rightPosition = rightLaserEffectInstance.transform.parent.position;
                Vector3 rightPoint = rightAimRay.GetPoint(num);
                if (Physics.Raycast(rightAimRay, out var hitInfo, num, (int)LayerIndex.world.mask | (int)LayerIndex.defaultLayer.mask))
                {
                    rightPoint = hitInfo.point;
                }
                rightLaserLineComponent.SetPosition(0, rightPosition);
                rightLaserLineComponent.SetPosition(1, rightPoint);
                float num2;
                if (duration - base.age > 0.5f)
                {
                    num2 = base.age / duration;
                }
                else
                {
                    flashTimer -= Time.deltaTime;
                    if (flashTimer <= 0f)
                    {
                        laserOn = !laserOn;
                        flashTimer = 71f / (678f * (float)Mathf.PI);
                    }
                    num2 = (laserOn ? 1f : 0f);
                }
                num2 *= laserMaxWidth;
                rightLaserLineComponent.startWidth = num2;
                rightLaserLineComponent.endWidth = num2;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= duration && base.isAuthority)
            {
                FireDoubleMegaLaser nextState = new FireDoubleMegaLaser();
                outer.SetNextState(nextState);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}