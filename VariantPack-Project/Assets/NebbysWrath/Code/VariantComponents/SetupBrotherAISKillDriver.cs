using RoR2.CharacterAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI.Components;

namespace NW.Components
{
    public class SetupBrotherAISkillDriver : VariantComponent
    {
        public void Awake()
        {
            var aiSkillDriver = gameObject.AddComponent<AISkillDriver>();
            aiSkillDriver.customName = "SummoningRoar";
            aiSkillDriver.skillSlot = RoR2.SkillSlot.Utility;
            aiSkillDriver.requireSkillReady = true;
            aiSkillDriver.minDistance = 15;
            aiSkillDriver.maxDistance = 100;
            aiSkillDriver.selectionRequiresTargetLoS = true;
            aiSkillDriver.selectionRequiresOnGround = true;
            aiSkillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            aiSkillDriver.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            aiSkillDriver.moveInputScale = 1;
            aiSkillDriver.aimType = AISkillDriver.AimType.AtMoveTarget;
            aiSkillDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            var baseAI = gameObject.GetComponent<BaseAI>();
            var currentAISkillDrivers = baseAI.skillDrivers;
            HG.ArrayUtils.ArrayInsert(ref currentAISkillDrivers, 1, aiSkillDriver);
            baseAI.skillDrivers = currentAISkillDrivers;
            Destroy(this);
        }
    }
}