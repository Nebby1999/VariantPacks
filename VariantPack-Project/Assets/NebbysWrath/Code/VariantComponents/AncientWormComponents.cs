using RoR2;
using RoR2.CharacterAI;
using System.Collections.Generic;
using VAPI;

namespace NW.Components
{
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterMaster, useOnClient = false, useOnServer = true)]
    public sealed class AncientWormModifyAI : VariantComponent
    {
        public CharacterMaster characterMaster { get; private set; }
        public BaseAI baseAI { get; private set; }
        private AISkillDriver[] _origSkillDriverArray;
        private float origMaxDistance;
        private bool origIgnoreNodeGraph;

        private void Awake()
        {
            characterMaster = GetComponent<CharacterMaster>();
            baseAI = GetComponent<BaseAI>();
        }

        private void Start()
        {
            if(baseAI)
            {
                _origSkillDriverArray = baseAI.skillDrivers;

                List<AISkillDriver> newSkillDrivers = new List<AISkillDriver>();
                foreach(var skillDriver in _origSkillDriverArray)
                {
                    switch(skillDriver.customName)
                    {
                        case "FollowTargetOnNodegraphAndChangeStance":
                        case "Blink":
                            break;
                        case "ChaseAndSteerAtTarget":
                            origMaxDistance = skillDriver.maxDistance;
                            origIgnoreNodeGraph = skillDriver.ignoreNodeGraph;

                            skillDriver.maxDistance = 10f;
                            skillDriver.ignoreNodeGraph = true;
                            newSkillDrivers.Add(skillDriver);
                            break;
                        default:
                            newSkillDrivers.Add(skillDriver);
                            break;
                    }
                }

                baseAI.ReplaceSkillDrivers(newSkillDrivers.ToArray());
            }
        }

        private void OnDestroy()
        {
            if(baseAI)
            {
                foreach(var orig in _origSkillDriverArray)
                {
                    if(orig.customName == "ChaseAndSteerAtTarget")
                    {
                        orig.maxDistance = origMaxDistance;
                        orig.ignoreNodeGraph = origIgnoreNodeGraph;
                    }
                }

                baseAI.ReplaceSkillDrivers(_origSkillDriverArray);
            }
        }
    }

    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = true, useOnServer = true)]
    public sealed class AncientWormModifyBody : VariantComponent
    {
        public CharacterBody characterBody { get; private set; }
        public WormBodyPositionsDriver positionsDriver { get; private set; }
        private float origMaxBreachSpeed;
        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            positionsDriver = GetComponent<WormBodyPositionsDriver>();
        }

        private void Start()
        {
            origMaxBreachSpeed = positionsDriver.maxBreachSpeed;
            positionsDriver.maxBreachSpeed = float.PositiveInfinity;
        }

        private void OnDestroy()
        {
            if(positionsDriver)
            {
                positionsDriver.maxBreachSpeed = origMaxBreachSpeed;
            }
        }
    }
}