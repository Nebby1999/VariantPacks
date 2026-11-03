using RoR2;
using VAPI;

namespace NW.Components
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = true, useOnServer = true)]
    public class SolusWingGlitchOut : VariantComponent
    {
        private void Start()
        {
            if(Util.HasEffectiveAuthority(gameObject) && EntityStateMachine.TryFindByCustomName(gameObject, "Glitch", out var machine))
            {
                machine.SetNextState(new EntityStates.SolusWing.GlitchState());
            }
        }
    }
}