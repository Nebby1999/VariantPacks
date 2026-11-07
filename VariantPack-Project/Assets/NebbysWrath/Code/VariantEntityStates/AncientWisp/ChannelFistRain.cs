using RoR2.Projectile;
using RoR2;
using System.Collections.ObjectModel;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using UnityEngine.AddressableAssets;
using MSU;

namespace EntityStates.MoffeinAncientWispSkills.AncientStoneWisp
{
    public class ChannelFistRain : BaseState
    {
        private float castTimer;

        public static float baseDuration = 7f;
        public static float explosionDelay = 1.3f;
        public static int explosionCount = 45;

        public static int maxExplosions = 120;

        public static float damageCoefficient = 2.1f;
        public static float randomRadius = 16f;
        public static float radius = 6f;
        public static GameObject projectilePrefab;


        private float _duration;
        private float _durationBetweenCast;
        private float _totalExplosions;
        private float _lastUpdateTime;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/TitanPreFistProjectile.prefab");

            while (!request.IsDone)
                yield return null;

            projectilePrefab = request.Result;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            _lastUpdateTime = Time.time;
            _duration = baseDuration;
            _durationBetweenCast = baseDuration / Mathf.Min(explosionCount * attackSpeedStat, maxExplosions);
            base.PlayCrossfade("Body", "ChannelRain", 0.3f);
            Util.PlaySound("Play_titanboss_shift_charge", base.gameObject);

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.Slow50);
            }
        }

        private void PlaceRain()
        {
            if (!base.isAuthority)
            {
                return;
            }
            Vector3 vector = Vector3.zero;
            Ray aimRay = base.GetAimRay();
            aimRay.origin += UnityEngine.Random.insideUnitSphere * randomRadius;
            RaycastHit raycastHit;
            if (Physics.Raycast(aimRay, out raycastHit, (float)LayerIndex.world.mask))
            {
                vector = raycastHit.point;
            }
            if (vector != Vector3.zero)
            {
                TeamIndex teamIndex = base.characterBody.GetComponent<TeamComponent>().teamIndex;
                TeamIndex enemyTeam;
                if (teamIndex != TeamIndex.Player)
                {
                    if (teamIndex == TeamIndex.Monster)
                    {
                        enemyTeam = TeamIndex.Player;
                    }
                    else
                    {
                        enemyTeam = TeamIndex.Neutral;
                    }
                }
                else
                {
                    enemyTeam = TeamIndex.Monster;
                }
                Transform transform = FindTargetClosest(vector, enemyTeam);
                Vector3 a = vector;
                if (transform)
                {
                    a = transform.transform.position;
                }
                a += UnityEngine.Random.insideUnitSphere * randomRadius;
                if (Physics.Raycast(new Ray
                {
                    origin = a + Vector3.up * randomRadius,
                    direction = Vector3.down
                }, out raycastHit, 500f, LayerIndex.world.mask))
                {
                    Vector3 point = raycastHit.point;
                    Quaternion rotation;
                    Vector3 rot = new Vector3(90f, 0f, 0f);
                    rotation = Quaternion.Euler(rot);
                    
                    DamageTypeCombo? damageTypeOverride = null;
                    if(projectilePrefab.TryGetComponent<ProjectileDamage>(out var pjDamage))
                    {
                        var combo = pjDamage.damageType;
                        combo.damageSource |= DamageSource.Primary;
                        damageTypeOverride = combo;
                    }

                    FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                    fireProjectileInfo.projectilePrefab = projectilePrefab;
                    fireProjectileInfo.position = point;
                    fireProjectileInfo.rotation = rotation;
                    fireProjectileInfo.owner = base.gameObject;
                    fireProjectileInfo.damage = damageStat * damageCoefficient;
                    fireProjectileInfo.force = 2000f;
                    fireProjectileInfo.crit = base.characterBody.RollCrit();
                    fireProjectileInfo.damageTypeOverride = damageTypeOverride;

                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            castTimer += Time.time - _lastUpdateTime;
            _lastUpdateTime = Time.time;
            if (castTimer >= _durationBetweenCast)
            {
                PlaceRain();
                castTimer -= _durationBetweenCast;
            }
            if (base.fixedAge >= _duration && base.isAuthority)
            {
                outer.SetNextState(new EndFistRain());
            }
        }

        private Transform FindTargetClosest(Vector3 point, TeamIndex enemyTeam)
        {
            ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(enemyTeam);
            float num = 99999f;
            Transform result = null;
            for (int i = 0; i < teamMembers.Count; i++)
            {
                float num2 = Vector3.SqrMagnitude(teamMembers[i].transform.position - point);
                if (num2 < num)
                {
                    num = num2;
                    result = teamMembers[i].transform;
                }
            }
            return result;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                if (base.characterBody.HasBuff(RoR2Content.Buffs.Slow50))
                {
                    base.characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
                }
            }
            base.OnExit();
        }

    }
}