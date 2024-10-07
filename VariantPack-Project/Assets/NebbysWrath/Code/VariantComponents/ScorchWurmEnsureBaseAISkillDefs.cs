using MSU;
using RoR2.CharacterAI;
using RoR2.Skills;
using System.Collections;
using VAPI.Components;

namespace NW.Components
{
    public class ScorchWurmEnsureBaseAISkillDefs : VariantComponent
    {
        private static SkillDef _electricBreach;
        private static SkillDef _spewOrb;

        [AsyncAssetLoad]
        private static IEnumerator Load()
        {
            var breachRequest = NWAssets.LoadAssetAsync<SkillDef>("ElectricBreach");
            var spewRequest = NWAssets.LoadAssetAsync<SkillDef>("SpewOrb");

            var routine = new ParallelCoroutine();
            routine.Add(breachRequest);
            routine.Add(spewRequest);

            while (!routine.IsDone())
                yield return null;

            _electricBreach = breachRequest.asset;
            _spewOrb = spewRequest.asset;
        }

        private void Awake()
        {
            var ai = GetComponents<AISkillDriver>();

            foreach(var driver in ai)
            {
                switch(driver.customName)
                {
                    case "Breach":
                        driver.requiredSkill = _electricBreach;
                        break;
                    case "LavaBomb":
                        driver.requiredSkill = _spewOrb;
                        break;
                }
            }
        }
    }
}