using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LemurianMonster.Flamethrower
{
    public class FireConstantFireballs : BaseState
    {
		public static GameObject projectilePrefab = FireFireball.projectilePrefab;

		public static GameObject effectPrefab = FireFireball.effectPrefab;

		public static float baseDuration = FireFireball.baseDuration;

		public static float damageCoefficient = FireFireball.damageCoefficient;

		public static float force = FireFireball.force;

		public static string attackString = FireFireball.attackString;

		private float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = baseDuration / attackSpeedStat;
			PlayAnimation("Gesture", "FireFireball", "FireFireball.playbackRate", duration);
			Util.PlaySound(attackString, base.gameObject);
			Ray aimRay = GetAimRay();
			string muzzleName = "MuzzleMouth";
			if ((bool)effectPrefab)
			{
				EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
			}
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, damageStat * damageCoefficient, force, Util.CheckRoll(critStat, base.characterBody.master));
			}
		}

		public override void FixedUpdate()
        {
			base.FixedUpdate();
            if(fixedAge >= duration && isAuthority)
            {
                if(inputBank && inputBank.skill1.down)
                {
                    FireConstantFireballs nextState = new FireConstantFireballs();
                    outer.SetNextState(nextState);
                }
                else
                {
                    outer.SetNextStateToMain();
                }
            }
        }

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}
	}
}