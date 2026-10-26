using RoR2;

namespace EntityStates.ScavMonster.Dream
{
    public class EnterLuckySit : BaseSitState
    {
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = EnterSit.baseDuration / attackSpeedStat;

            Util.PlaySound(EnterSit.soundString, gameObject);
            PlayCrossfade("Body", "EnterSit", "Sit.playbackRate", duration, 0.1f);

            modelLocator.normalizeToFloor = true;
            modelLocator.modelTransform.GetComponent<AimAnimator>().enabled = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration)
            {
                outer.SetNextState(new DreamLuck());
            }
        }
    }
}