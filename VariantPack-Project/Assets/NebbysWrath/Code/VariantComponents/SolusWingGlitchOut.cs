using RoR2;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
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