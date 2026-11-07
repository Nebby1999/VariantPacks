// RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// EntityStates.DefectiveUnit.Detonate
using EntityStates;
using EntityStates.DefectiveUnit;
using RoR2;
using RoR2.AsyncManagement;
using UnityEngine;

namespace EntityStates.DefectiveUnit.Slammer
{
    public class DetonateSlammer : DefectiveUniBaseState
    {
        public float chargeTime = 0.5f;

        public static EntityStateGameObject chargeEffectPrefab => Detonate.chargeEffectPrefab;

        public static EntityStateGameObject detonateVFX => Detonate.detonateVFX;

        public float cooldownTime = 0.5f;

        public static float damageCoefficient = 0.5f;

        public static float soundTime = 0.5f;

        private bool _hasDetonated;

        private bool _hasPlayedSound;

        public override float AccumulatedSkillSpan => chargeTime + cooldownTime;

        public override void OnEnter()
        {
            base.OnEnter();
            EffectData effectData = new EffectData();
            effectData.origin = base.gameObject.transform.position;
            if ((bool)chargeEffectPrefab)
            {
                EffectManager.SpawnEffect(chargeEffectPrefab, effectData, transmit: false);
            }
            base.characterMotor.velocity = Vector3.zero;
            PlayCrossfade("Gesture, Override", "DetonateCharge", "DetonateCharge.playbackRate", chargeTime, 0.1f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge > chargeTime && !_hasDetonated)
            {
                _hasDetonated = true;
                EffectData effectData = new EffectData();
                effectData.origin = base.gameObject.transform.position;
                EffectManager.SpawnEffect(detonateVFX, effectData, transmit: false);
                Util.PlaySound("Play_defectiveUnit_attack_melee_explosion", base.gameObject);
                if (base.isAuthority)
                {
                    BlastAttack blastAttack = new BlastAttack();
                    blastAttack.position = base.gameObject.transform.position;
                    blastAttack.attacker = base.gameObject;
                    blastAttack.teamIndex = base.characterBody.teamComponent.teamIndex;
                    blastAttack.radius = 12f;
                    blastAttack.baseDamage = damageCoefficient * damageStat;
                    blastAttack.damageType = DamageTypeExtended.InventoryDisable;
                    blastAttack.damageType.damageSource |= DamageSource.Secondary;
                    blastAttack.procCoefficient = 1f;
                    blastAttack.crit = RollCrit();
                    blastAttack.baseForce = 0f;
                    blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                    blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                    blastAttack.damageColorIndex = DamageColorIndex.Default;
                    blastAttack.Fire();
                }
                PlayCrossfade("Gesture, Override", "DetonateCooldown", "DetonateCooldown.playbackRate", cooldownTime, 0.1f);
            }
            if (base.fixedAge > soundTime && !_hasPlayedSound)
            {
                _hasPlayedSound = true;
                Util.PlaySound("Play_defectiveUnit_jump", base.gameObject);
            }
            if (base.fixedAge > AccumulatedSkillSpan)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}
