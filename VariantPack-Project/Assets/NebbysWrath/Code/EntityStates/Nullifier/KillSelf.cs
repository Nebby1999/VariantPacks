namespace EntityStates.NullifierMonster.Kamikaze
{
    public class KillSelf : EntityState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if(healthComponent)
            {
                healthComponent.Suicide(null, null, default);
            }
        }
    }
}