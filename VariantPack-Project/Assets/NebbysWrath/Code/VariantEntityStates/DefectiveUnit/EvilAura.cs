using EntityStates;
using RoR2;
using RoR2.AsyncManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.DefectiveUnit.EvilBuddy
{
    public class EvilAura : BaseSkillState
    {
        public static float minimumDuration = 1f;

        public static float speedMultiplier = 1.25f;

        public static float chargeBeforeLaunch = 0.5f;

        public static float chargeDamageCoefficient = 1;

        public static float awayForceMagnitude = 2100;

        public static float upwardForceMagnitude = 2100;

        public static EntityStateGameObject impactEffectPrefab => EntityStates.FriendUnit.KineticAura.impactEffectPrefab;

        public static float hitPauseDuration = 0.04f;

        public static string impactSoundString = "Play_Item_FriendUnit_Hit";

        public static float initialHopVelocity = 1;

        public static float recoilAmplitude = 1;

        public static string startSoundString = "Play_Item_FriendUnit_Start";

        public static string launchSoundString = "Play_Item_FriendUnit_Launch";

        public static string endSoundString = "Play_Item_FriendUnit_Aura_End";

        public static EntityStateGameObject knockbackEffectPrefab => EntityStates.FriendUnit.KineticAura.knockbackEffectPrefab;

        public static float knockbackDamageCoefficient = 1;

        public static float massThresholdForKnockback = 250;

        public static float knockbackForce = 800;

        public static float refreshTime = 1.5f;

        public static string EffectChildName = "ChargeFX";

        public static string shriekSoundName = "Play_Item_FriendUnit_VO_Attack_Med";

        public static float shriekSoundChance = 10;

        public static float lockonAngle = 180;

        public static float lockonDistance = 30;

        public static EntityStateGameObject startEffectPrefab;

        public static EntityStateGameObject launchEffectPrefab;

        public static EntityStateGameObject endEffectPrefab;

        private uint soundID;

        private float hitPauseTimer;

        private bool hasLaunched;

        private Vector3 idealDirection;

        private OverlapAttack attack;

        private float resetTimer;

        private bool inHitPause;

        private float baseTurnSpeed;

        private HurtBox currentTarget;

        private readonly BullseyeSearch search = new BullseyeSearch();

        private float currentMult;

        private List<HurtBox> victimsStruck = new List<HurtBox>();

        private static bool _initFromOrig;

        public override void OnEnter()
        {
            base.OnEnter();
            if(!_initFromOrig)
            {
                var orig = new EntityStates.FriendUnit.KineticAura();
                _initFromOrig = true;

                startEffectPrefab = orig.startEffectPrefab;
                launchEffectPrefab = orig.launchEffectPrefab;
                endEffectPrefab = orig.endEffectPrefab;
            }

            if (base.isAuthority)
            {
                UpdateDirection();
            }
            if ((bool)startEffectPrefab && (bool)base.characterBody)
            {
                EffectData effectData = new EffectData();
                effectData.origin = base.characterBody.corePosition;
                effectData.SetHurtBoxReference(base.characterBody.mainHurtBox);
                EffectManager.SpawnEffect(startEffectPrefab, effectData, transmit: false);
            }
            if ((bool)base.characterDirection)
            {
                baseTurnSpeed = base.characterDirection.turnSpeed;
                float num = (base.characterBody ? (base.characterBody.baseMoveSpeed * base.characterBody.sprintingSpeedMultiplier) : 17.4f);
                baseTurnSpeed *= Mathf.Max(1f, moveSpeedStat / num);
                base.characterDirection.forward = idealDirection;
            }
            if ((bool)base.characterMotor && base.characterMotor.isGrounded)
            {
                SmallHop(base.characterMotor, base.characterBody.moveSpeed * initialHopVelocity);
            }
            Transform transform = FindModelChild(EffectChildName);
            if ((bool)transform)
            {
                transform.gameObject.SetActive(value: true);
            }
            hasLaunched = false;
            soundID = Util.PlaySound(startSoundString, base.gameObject);
            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.SmallArmorBoost);
            }
            HitBoxGroup hitBoxGroup = null;
            Transform modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                hitBoxGroup = modelTransform.GetComponent<HitBoxGroup>();
            }
            attack = new OverlapAttack();
            attack.attacker = base.gameObject;
            attack.inflictor = base.gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = chargeDamageCoefficient * damageStat;
            attack.damageColorIndex = DamageColorIndex.Item;
            attack.hitEffectPrefab = impactEffectPrefab;
            attack.forceVector = Vector3.up * upwardForceMagnitude;
            attack.pushAwayForce = awayForceMagnitude;
            attack.hitBoxGroup = hitBoxGroup;
            attack.retriggerTimeout = refreshTime;
            attack.damageType.damageSource = DamageSource.Primary;
            attack.isCrit = RollCrit();
            currentMult = 1f;
        }

        public override void OnExit()
        {
            base.OnExit();
            Transform transform = FindModelChild(EffectChildName);
            if ((bool)transform)
            {
                transform.gameObject.SetActive(value: false);
            }
            AkSoundEngine.StopPlayingID(soundID);
            Util.PlaySound(endSoundString, base.gameObject);
            if ((bool)base.characterDirection)
            {
                base.characterDirection.turnSpeed = baseTurnSpeed;
            }
            if (!outer.destroying && (bool)base.characterBody)
            {
                if ((bool)endEffectPrefab)
                {
                    EffectManager.SpawnEffect(endEffectPrefab, new EffectData
                    {
                        origin = base.characterBody.corePosition
                    }, transmit: false);
                }
                base.characterBody.isSprinting = false;
                if (NetworkServer.active)
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.SmallArmorBoost);
                }
            }
            if ((bool)base.characterMotor && !base.characterMotor.disableAirControlUntilCollision)
            {
                base.characterMotor.velocity += GetIdealVelocity();
            }
        }

        private float GetDamageBoostFromSpeed()
        {
            return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
        }

        private void UpdateDirection()
        {
            if ((bool)currentTarget && (bool)currentTarget.healthComponent && !currentTarget.healthComponent.alive)
            {
                if (!victimsStruck.Contains(currentTarget))
                {
                    victimsStruck.Add(currentTarget);
                }
                currentTarget = null;
            }
            if (!currentTarget)
            {
                currentTarget = GetBestTarget();
            }
            if ((bool)currentTarget)
            {
                Ray aimRay = GetAimRay();
                idealDirection = (currentTarget.gameObject.transform.position - aimRay.origin).normalized;
            }
            else if ((bool)base.inputBank)
            {
                idealDirection = base.inputBank.aimDirection.normalized;
            }
        }

        private HurtBox GetBestTarget()
        {
            Ray aimRay = GetAimRay();
            search.teamMaskFilter = TeamMask.all;
            search.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
            search.filterByLoS = true;
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
            search.maxDistanceFilter = lockonDistance;
            search.maxAngleFilter = lockonAngle;
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            foreach (HurtBox item in victimsStruck)
            {
                if (item != null && (bool)item.gameObject)
                {
                    search.FilterOutGameObject(item.gameObject);
                }
            }
            return (from hurtbox in search.GetResults()
                    where hurtbox != null && (bool)hurtbox.healthComponent && hurtbox.healthComponent.alive
                    select hurtbox).FirstOrDefault();
        }

        private Vector3 GetIdealVelocity()
        {
            return idealDirection * base.characterBody.moveSpeed * speedMultiplier;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!base.isAuthority)
            {
                return;
            }
            if (hasLaunched)
            {
                if (base.fixedAge >= chargeBeforeLaunch + minimumDuration && !IsKeyDownAuthority())
                {
                    outer.SetNextStateToMain();
                    return;
                }
                resetTimer += Time.deltaTime;
                if (resetTimer >= refreshTime)
                {
                    victimsStruck = new List<HurtBox>();
                    resetTimer -= refreshTime;
                }
                if ((bool)base.characterBody)
                {
                    base.characterBody.isSprinting = true;
                }
                if ((bool)base.characterDirection)
                {
                    base.characterDirection.turnSpeed = baseTurnSpeed * base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed;
                }
                UpdateDirection();
                if (base.isAuthority && HGPhysics.DoesOverlapSphere(base.characterBody.corePosition + idealDirection.normalized * base.characterBody.radius, base.characterBody.radius * 0.5f, LayerIndex.world.mask))
                {
                    outer.SetNextState(new EvilAuraImpact
                    {
                        idealDirection = idealDirection,
                        damageBoostFromSpeed = GetDamageBoostFromSpeed(),
                        isCrit = attack.isCrit
                    });
                }
                else if (!inHitPause)
                {
                    if ((bool)base.characterDirection)
                    {
                        base.characterDirection.moveVector = idealDirection;
                        if ((bool)base.characterMotor && !base.characterMotor.disableAirControlUntilCollision)
                        {
                            base.characterMotor.rootMotion += GetIdealVelocity() * GetDeltaTime();
                        }
                    }
                    attack.damage = damageStat * (chargeDamageCoefficient * GetDamageBoostFromSpeed());
                    if (!attack.Fire(victimsStruck))
                    {
                        return;
                    }
                    if (victimsStruck.Contains(currentTarget))
                    {
                        currentTarget = null;
                    }
                    Util.PlaySound(impactSoundString, base.gameObject);
                    inHitPause = true;
                    hitPauseTimer = hitPauseDuration;
                    AddRecoil(-0.5f * recoilAmplitude, -0.5f * recoilAmplitude, -0.5f * recoilAmplitude, 0.5f * recoilAmplitude);
                    for (int i = 0; i < victimsStruck.Count; i++)
                    {
                        float num = 0f;
                        HurtBox hurtBox = victimsStruck[i];
                        if (!hurtBox.healthComponent)
                        {
                            continue;
                        }
                        CharacterMotor component = hurtBox.healthComponent.GetComponent<CharacterMotor>();
                        if ((bool)component)
                        {
                            num = component.mass;
                        }
                        else
                        {
                            Rigidbody component2 = hurtBox.healthComponent.GetComponent<Rigidbody>();
                            if ((bool)component2)
                            {
                                num = component2.mass;
                            }
                        }
                        bool victimPlayerControlled = (hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body.isPlayerControlled);

                        if (num >= massThresholdForKnockback)
                        {
                            outer.SetNextState(new EvilAuraImpact
                            {
                                victimHealthComponent = hurtBox.healthComponent,
                                victimHurtBox = hurtBox,
                                idealDirection = idealDirection,
                                damageBoostFromSpeed = GetDamageBoostFromSpeed(),
                                isCrit = attack.isCrit
                            });
                            break;
                        }
                        else if(victimPlayerControlled)
                        {
                            outer.SetNextState(new EvilAuraImpact
                            {
                                victimHealthComponent = hurtBox.healthComponent,
                                victimHurtBox = hurtBox,
                                idealDirection = idealDirection,
                                damageBoostFromSpeed = 1,
                                isCrit = attack.isCrit
                            });
                            break;
                        }
                    }
                }
                else
                {
                    base.characterMotor.velocity = Vector3.zero;
                    hitPauseTimer -= GetDeltaTime();
                    if (hitPauseTimer < 0f)
                    {
                        inHitPause = false;
                    }
                }
            }
            else if (base.fixedAge >= chargeBeforeLaunch)
            {
                EffectData effectData = new EffectData
                {
                    origin = base.characterBody.corePosition,
                    scale = base.characterBody.radius,
                    rotation = Util.QuaternionSafeLookRotation(base.inputBank.aimDirection)
                };
                effectData.SetHurtBoxReference(base.characterBody.mainHurtBox);
                Util.PlaySound(launchSoundString, base.gameObject);
                EffectManager.SpawnEffect(launchEffectPrefab, effectData, transmit: true);
                hasLaunched = true;
                if (base.isAuthority && Random.Range(0f, 100f) <= shriekSoundChance)
                {
                    Util.PlaySound(shriekSoundName, outer.gameObject);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}

