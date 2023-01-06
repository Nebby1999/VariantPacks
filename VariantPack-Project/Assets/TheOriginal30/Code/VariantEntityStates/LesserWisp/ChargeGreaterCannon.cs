using EntityStates;
using EntityStates.GreaterWispMonster;
using RoR2;
using UnityEngine;

namespace EntityStates.Wisp1Monster.AlmostGreat
{
    public class ChargeGreaterCannon : BaseState
    {
        public static float baseDuration = 3f;
        protected float duration;
        private GameObject chargeEffect;
        private const float soundDuration = 2f;
        private string attackString;
        private GameObject effectPrefab;

        private ChargeCannons goodState;

        public override void OnEnter()
        {
            if (this.goodState == null) this.goodState = new ChargeCannons();
            this.attackString = this.goodState.attackString;
            this.effectPrefab = this.goodState.effectPrefab;

            base.OnEnter();
            Util.PlayAttackSpeedSound(this.attackString, base.gameObject, this.attackSpeedStat * (2f / ChargeGreaterCannon.baseDuration));
            this.duration = ChargeGreaterCannon.baseDuration / this.attackSpeedStat;
            Transform modelTransform = base.GetModelTransform();

            if (modelTransform)
            {
                ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    Transform muzzleTransform = childLocator.FindChild("Muzzle");
                    if (muzzleTransform)
                    {
                        this.chargeEffect = UnityEngine.Object.Instantiate<GameObject>(this.effectPrefab, transform.position, transform.rotation);
                        this.chargeEffect.transform.parent = transform;
                        ScaleParticleSystemDuration scaleParticleSystemDuration = this.chargeEffect.GetComponent<ScaleParticleSystemDuration>();
                        if (scaleParticleSystemDuration) scaleParticleSystemDuration.newDuration = this.duration;
                    }
                }
            }

            if (base.characterBody)
            {
                base.characterBody.SetAimTimer(this.duration);
            }

            base.PlayAnimation("Body", "ChargeAttack1", "ChargeAttack1.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            EntityState.Destroy(this.chargeEffect);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                FireGreaterCannon nextState = new FireGreaterCannon();
                this.outer.SetNextState(nextState);
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
