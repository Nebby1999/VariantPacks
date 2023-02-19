using RoR2;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI.Components;

namespace NW.Components
{
    public class SetupBrotherGenericSkill : VariantComponent
    {
        private static SkillFamily summoningRoarFamily = NWAssets.LoadAsset<SkillFamily>("SummoningRoarFamily");
        private void Awake()
        {
            SkillLocator locator = GetComponent<SkillLocator>();
            gameObject.SetActive(false);
            GenericSkill utilitySkill = gameObject.AddComponent<GenericSkill>();
            utilitySkill._skillFamily = summoningRoarFamily;
            HG.ArrayUtils.ArrayAppend(ref locator.allSkills, utilitySkill);
            locator.utility = utilitySkill;
            var esm = EntityStateMachine.FindByCustomName(gameObject, "Body");
            esm.SetNextState(new EntityStates.LemurianBruiserMonster.SpawnState());
            gameObject.SetActive(true);
            Destroy(this);
        }
    }
}
