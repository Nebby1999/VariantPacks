using NW.PrefabClones;
using RoR2;
using RoR2.Projectile;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MegaConstruct.Omicron
{
    public class LaserBarrage : FlyState
    {
        private static string _animationLayerName;
        private static string _animationEnterStateName;
        private static float _duration;
        private static bool _obtainedOrigs;

        private static float _baseTimeBetweenLasers = 0.5f;
        private static float _baseDurationUntilFullSpeed;
        private static float _fullSpeedMultiplier = 2;

        private float _timeUntilFullSpeed;
        private float _fullSpeedStopwatch;

        private float _baseTimeBetweenLasersForThisState;
        private float _actualTimeBetweenLasers;
        private float _fireStopwatch;
        private Transform[] _firePoints;
        public override void OnEnter()
        {
            if(!_obtainedOrigs)
            {
                _obtainedOrigs = true;
                var orig = new RaiseShield();
                _animationLayerName = orig.animationLayerName;
                _animationEnterStateName = orig.animationEnterStateName;
                _duration = orig.duration;
                _baseDurationUntilFullSpeed = _duration / 2;
            }
            base.OnEnter();

            PlayAnimation(_animationLayerName, _animationEnterStateName);

            _baseTimeBetweenLasersForThisState = _baseTimeBetweenLasers / attackSpeedStat;
            _actualTimeBetweenLasers = _baseTimeBetweenLasersForThisState;
            _timeUntilFullSpeed = _baseDurationUntilFullSpeed / attackSpeedStat;

            ChildLocator loc = GetModelChildLocator();
            _firePoints = new Transform[]
            {
                loc.FindChild("BarragePoint0"),
                loc.FindChild("BarragePoint1"),
                loc.FindChild("BarragePoint2"),
                loc.FindChild("BarragePoint3")
            };
        }


        public override bool CanExecuteSkill(GenericSkill skillSlot)
        {
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > _duration && isAuthority)
                outer.SetNextState(new ExitShield());

            //Gradually speed up
            if(_fullSpeedStopwatch < _timeUntilFullSpeed)
            {
                _fullSpeedStopwatch += Time.fixedDeltaTime;

                var dividend = Util.Remap(_fullSpeedStopwatch, 0, _timeUntilFullSpeed, 1, _fullSpeedMultiplier);
                _actualTimeBetweenLasers = _baseTimeBetweenLasersForThisState / dividend;
            }

            _fireStopwatch += Time.fixedDeltaTime;
            if(_fireStopwatch > _actualTimeBetweenLasers)
            {
                _fireStopwatch -= _actualTimeBetweenLasers;
                FireLaser();
            }
        }

        private void FireLaser()
        {
            if (isAuthority)
            {
                FireProjectileInfo info = new FireProjectileInfo
                {
                    crit = RollCrit(),
                    damage = damageStat,
                    damageColorIndex = DamageColorIndex.Default,
                    force = 30,
                    owner = gameObject,
                    projectilePrefab = OmicronProjectile.omicronProjectile,
                };

                for(int i = 0; i < _firePoints.Length; i++)
                {
                    info.rotation = Quaternion.LookRotation(_firePoints[i].forward);
                    info.position = _firePoints[i].position;
                    ProjectileManager.instance.FireProjectile(info);
                }
            }
        }
    }
}