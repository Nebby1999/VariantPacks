using RoR2;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnServer = true)]
    public class CloakWhileOutOfCombat : VariantComponent
    {
        CharacterBody body;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
        }

        private void FixedUpdate()
        {
            if(body.outOfCombat == true && body.GetBuffCount(RoR2Content.Buffs.Cloak) == 0)
            {
                body.AddBuff(RoR2Content.Buffs.Cloak);
            }
            else if(body.outOfCombat == false && body.GetBuffCount(RoR2Content.Buffs.Cloak) >= 1)
            {
                body.RemoveBuff(RoR2Content.Buffs.Cloak);
            }
        }
    }
}