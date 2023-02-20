using EntityStates;
using EntityStates.MiniMushroom;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;

namespace EntityStates.MiniMushroom.AD
{
    public class ArmorBreakerGrenade : BaseState
    {
        public static GameObject chargeEffectPrefab = SporeGrenade.chargeEffectPrefab;

        public static string attackSoundString = "Play_minimushroom_spore_shoot";

        public static string chargeUpSoundString = "Play_minimushroom_spore_chargeUp";

        public static float recoilAmplitude = 1f;

        public static GameObject projectilePrefab = NW.PrefabClones.ArmorBreakerGrenade.projectile;

        public static float baseDuration = 4.4f;

        public static string muzzleString = "Muzzle";

        public static float damageCoefficient = 1;

        public static float timeToTarget = 1.5f;

        public static float projectileVelocity = 20f;

        public static float minimumDistance = 5;

        public static float maximumDistance = 60;

        public static float baseChargeTime = 2.6f;

        private uint chargeupSoundID;

        private Ray projectileRay;

        private Transform modelTransform;

        private float duration;

        private float chargeTime;

        private bool hasFired;

        private Animator modelAnimator;

        private GameObject chargeEffectInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            chargeTime = baseChargeTime / attackSpeedStat;
            modelAnimator = GetModelAnimator();
            if ((bool)modelAnimator)
            {
                modelAnimator.SetBool("isCharged", value: false);
                PlayAnimation("Gesture, Additive", "Charge");
                chargeupSoundID = Util.PlaySound(chargeUpSoundString, base.characterBody.modelLocator.modelTransform.gameObject);
            }
            Transform transform = FindModelChild("ChargeSpot");
            if ((bool)transform)
            {
                chargeEffectInstance = Object.Instantiate(chargeEffectPrefab, transform);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!(base.fixedAge >= chargeTime))
            {
                return;
            }
            if (!hasFired)
            {
                hasFired = true;
                modelAnimator?.SetBool("isCharged", value: true);
                if (base.isAuthority)
                {
                    FireGrenade(muzzleString);
                }
            }
            if (base.isAuthority && base.fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            PlayAnimation("Gesture, Additive", "Empty");
            AkSoundEngine.StopPlayingID(chargeupSoundID);
            if ((bool)chargeEffectInstance)
            {
                EntityState.Destroy(chargeEffectInstance);
            }
            base.OnExit();
        }

        private void FireGrenade(string targetMuzzle)
        {
            Ray aimRay = GetAimRay();
            Ray ray = new Ray(aimRay.origin, Vector3.up);
            Transform transform = FindModelChild(targetMuzzle);
            if ((bool)transform)
            {
                ray.origin = transform.position;
            }
            BullseyeSearch bullseyeSearch = new BullseyeSearch();
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
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
            else if (Physics.Raycast(aimRay, out hitInfo, 1000f, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
            {
                vector = hitInfo.point;
                flag = true;
            }
            float magnitude = projectileVelocity;
            if (flag)
            {
                Vector3 vector2 = vector - ray.origin;
                Vector2 vector3 = new Vector2(vector2.x, vector2.z);
                float magnitude2 = vector3.magnitude;
                Vector2 vector4 = vector3 / magnitude2;
                if (magnitude2 < minimumDistance)
                {
                    magnitude2 = minimumDistance;
                }
                if (magnitude2 > maximumDistance)
                {
                    magnitude2 = maximumDistance;
                }
                float y = Trajectory.CalculateInitialYSpeed(timeToTarget, vector2.y);
                float num = magnitude2 / timeToTarget;
                Vector3 direction = new Vector3(vector4.x * num, y, vector4.y * num);
                magnitude = direction.magnitude;
                ray.direction = direction;
            }
            Quaternion rotation = Util.QuaternionSafeLookRotation(ray.direction + Random.insideUnitSphere * 0.05f);
            ProjectileManager.instance.FireProjectile(projectilePrefab, ray.origin, rotation, base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, magnitude);
        }
    }
}
