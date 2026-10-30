using MSU;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Skills;
using System.Collections;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterMaster, useOnClient = false, useOnServer = true)]
    public class SetupLemurianSpouseAISkillDriver : VariantComponent
    {
        public void Awake()
        {
            var aiSkillDriver = gameObject.AddComponent<AISkillDriver>();
            aiSkillDriver.customName = "SummoningRoar";
            aiSkillDriver.skillSlot = RoR2.SkillSlot.Special;
            aiSkillDriver.requireSkillReady = true;
            aiSkillDriver.requiredSkill = NWContent.readOnlyContentPack.skillDefs.Find("SummoningRoar");
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
        }
    }

    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnServer = true, useOnClient = true)]
    public class SetupLemurianSpouseGenericSkill : VariantComponent
    {
        private void Awake()
        {
            SkillFamily skillFamily = NWContent.readOnlyContentPack.skillFamilies.Find("SummoningRoarFamily");
            SkillLocator locator = GetComponent<SkillLocator>();
            gameObject.SetActive(false);

            GenericSkill specialSkill = locator.GetOrFindSkill(SkillSlot.Special, null);
            if(specialSkill == null)
            {
                specialSkill = gameObject.AddComponent<GenericSkill>();
            }

            specialSkill._skillFamily = skillFamily;
            HG.ArrayUtils.ArrayAppend(ref locator.allSkills, specialSkill);
            locator.special = specialSkill;
            var esm = EntityStateMachine.FindByCustomName(gameObject, "Body");
            esm.SetNextState(EntityStateCatalog.InstantiateState(esm.initialStateType.stateType));
            gameObject.SetActive(true);
        }
    }
}