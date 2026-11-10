using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.DefectiveUnit.EvilBuddy
{
    public class EvilAuraImpact : BaseState
    {
        public HealthComponent victimHealthComponent;

        public Vector3 idealDirection;

        public float damageBoostFromSpeed;

        public bool isCrit;

        public HurtBox victimHurtBox;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                if ((bool)victimHealthComponent)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = base.gameObject,
                        damage = damageStat * EvilAura.knockbackDamageCoefficient * damageBoostFromSpeed,
                        crit = isCrit,
                        procCoefficient = 1f,
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = new DamageTypeCombo(DamageType.Stun1s, DamageTypeExtended.Generic, DamageSource.Primary),
                        position = base.characterBody.corePosition,
                        inflictedHurtbox = victimHurtBox
                    };
                    victimHealthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, victimHealthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, victimHealthComponent.gameObject);
                }
                PhysForceInfo forceInfo = default(PhysForceInfo);
                forceInfo.force = -idealDirection * EvilAura.knockbackForce * damageBoostFromSpeed;
                forceInfo.resetVelocity = true;
                forceInfo.disableAirControlUntilCollision = true;
                forceInfo.ignoreGroundStick = true;
                base.characterMotor.ApplyForceImpulse(in forceInfo);
            }
            if (base.isAuthority)
            {
                AddRecoil(-0.5f * EvilAura.recoilAmplitude * 3f, -0.5f * EvilAura.recoilAmplitude * 3f, -0.5f * EvilAura.recoilAmplitude * 8f, 0.5f * EvilAura.recoilAmplitude * 3f);
                EffectManager.SimpleImpactEffect(EvilAura.knockbackEffectPrefab, base.characterBody.corePosition, base.characterDirection.forward, transmit: true);
                outer.SetNextStateToMain();
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(victimHealthComponent ? victimHealthComponent.gameObject : null);
            writer.Write(idealDirection);
            writer.Write(damageBoostFromSpeed);
            writer.Write(isCrit);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            GameObject gameObject = reader.ReadGameObject();
            victimHealthComponent = (gameObject ? gameObject.GetComponent<HealthComponent>() : null);
            idealDirection = reader.ReadVector3();
            damageBoostFromSpeed = reader.ReadSingle();
            isCrit = reader.ReadBoolean();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}