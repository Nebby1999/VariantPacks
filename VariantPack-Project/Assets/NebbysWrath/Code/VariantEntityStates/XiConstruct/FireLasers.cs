using MSU.Config;
using NW.PrefabClones;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.MegaConstruct.Omicron
{
    public class FireLasers : BaseState
    {
        private static float _duration;
        private static string _animationLayerName;
        private static string _animationStateName;
        private static string _animationPlaybackRateParam;
        private static bool _obtainedOrigs;

        private float _fireStopwatch;
        private float _timeBetweenFires;
        private Transform[] _firePoints;
        public override void OnEnter()
        {
            if (!_obtainedOrigs)
            {
                _obtainedOrigs = true;
                var orig = new SpawnMinorConstructs();
                _duration = orig.duration / 2;
                _animationLayerName = orig.animationLayerName;
                _animationStateName = orig.animationStateName;
                _animationPlaybackRateParam = orig.animationPlaybackRateParam;
            }
            base.OnEnter();
            PlayAnimation(_animationLayerName, _animationStateName, _animationPlaybackRateParam, _duration);
            _timeBetweenFires = _duration / 4;
            ChildLocator loc = GetModelChildLocator();
            _firePoints = new Transform[]
            {
                loc.FindChild("AttachmentPoint0"),
                loc.FindChild("AttachmentPoint1"),
                loc.FindChild("AttachmentPoint2"),
                loc.FindChild("AttachmentPoint3")
            };
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > _duration && isAuthority)
                outer.SetNextStateToMain();

            _fireStopwatch -= Time.fixedDeltaTime;
            if(_fireStopwatch <= 0)
            {
                _fireStopwatch += _timeBetweenFires;
                Fire();
            }
        }

        private void Fire()
        {
            if (!isAuthority)
                return;

            var targetTransform = _firePoints[Run.instance.stageRng.RangeInt(0, _firePoints.Length)];

            FireProjectileInfo info = new FireProjectileInfo
            {
                crit = RollCrit(),
                damage = damageStat,
                damageColorIndex = DamageColorIndex.Default,
                force = 30,
                owner = gameObject,
                position = targetTransform.position,
                rotation = Quaternion.LookRotation(GetAimRay().direction),
                projectilePrefab = OmicronProjectile.omicronProjectile,
            };
            ProjectileManager.instance.FireProjectile(info);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}