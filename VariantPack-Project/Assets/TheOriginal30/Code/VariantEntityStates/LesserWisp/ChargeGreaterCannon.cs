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
            if (goodState == null) goodState = new ChargeCannons();
            attackString = goodState.attackString;
            effectPrefab = goodState.effectPrefab;

            base.OnEnter();
            Util.PlayAttackSpeedSound(attackString, gameObject, attackSpeedStat * (2f / baseDuration));
            duration = baseDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();

            if (modelTransform)
            {
                ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    Transform muzzleTransform = childLocator.FindChild("Muzzle");
                    if (muzzleTransform)
                    {
                        chargeEffect = Object.Instantiate<GameObject>(effectPrefab, transform.position, transform.rotation);
                        chargeEffect.transform.parent = transform;
                        ScaleParticleSystemDuration scaleParticleSystemDuration = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
                        if (scaleParticleSystemDuration) scaleParticleSystemDuration.newDuration = duration;
                    }
                }
            }

            if (characterBody)
            {
                characterBody.SetAimTimer(duration);
            }

            PlayAnimation("Body", "ChargeAttack1", "ChargeAttack1.playbackRate", duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            Destroy(chargeEffect);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                FireGreaterCannon nextState = new FireGreaterCannon();
                outer.SetNextState(nextState);
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
