using EntityStates.MoffeinAncientWispSkills;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VAPI;

namespace NW
{
    public static class AncientWispVariantHandler
    {
        public static void AddAncientToGreaterVariantRelation(CharacterVariantDef ancientWispVariant, CharacterVariantDef greaterWispVariant)
        {
            _ancientWispToGreaterWispVariantRelation.Add(ancientWispVariant, greaterWispVariant);
        }

        public static void AddAncientToArchaicVariantRelation(CharacterVariantDef ancientWispVariant, CharacterVariantDef archaicWispVariant)
        {
            _ancientWispToArchaicWispVariantRelation.Add(ancientWispVariant, archaicWispVariant);
        }

        private static Dictionary<CharacterVariantDef, CharacterVariantDef> _ancientWispToGreaterWispVariantRelation = new Dictionary<CharacterVariantDef, CharacterVariantDef>();
        private static Dictionary<CharacterVariantDef, CharacterVariantDef> _ancientWispToArchaicWispVariantRelation = new Dictionary<CharacterVariantDef, CharacterVariantDef>();

        private static ILHook Enrage_SummonEnemy_HandleVariantsHook;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Init()
        {
            try
            {
                Enrage_SummonEnemy_HandleVariantsHook = new ILHook(typeof(EntityStates.MoffeinAncientWispSkills.Enrage).GetMethod("SummonEnemy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), EnsureVariantPropagation, new ILHookConfig { ManualApply = true });
                Enrage_SummonEnemy_HandleVariantsHook.Apply();

                AddAncientToArchaicVariantRelation(NWContent.CharacterVariants.SomewhatAncientWisp, NWContent.CharacterVariants.SomewhatArchaicWisp);
                AddAncientToArchaicVariantRelation(NWContent.CharacterVariants.AncientStoneWisp, NWContent.CharacterVariants.ArchaicStoneWisp);

                AddAncientToGreaterVariantRelation(NWContent.CharacterVariants.SomewhatAncientWisp, NWContent.CharacterVariants.SomewhatGreatWisp);
                AddAncientToGreaterVariantRelation(NWContent.CharacterVariants.AncientStoneWisp, NWContent.CharacterVariants.GreaterStoneWisp);
            }
            catch (Exception ex)
            {
                NWLog.Fatal($"Error during AncientWispVariantHandler initialization process. AncientWisp propagation will not work: {ex}");
                Enrage_SummonEnemy_HandleVariantsHook.Undo();
            }
        }

        private static void EnsureVariantPropagation(ILContext il)
        {
            DirectorSpawnRequest EnsureVariantPropagationToMinions(DirectorSpawnRequest toReturn, Enrage enrageInstance)
            {
                CharacterBody body = enrageInstance.characterBody;
                if(!body.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController))
                {
                    return toReturn;
                }

                List<CharacterVariantDef> variantsToUse = null;
                Dictionary<CharacterVariantDef, CharacterVariantDef> dictionaryToUse = Enrage.archWispCard ? _ancientWispToArchaicWispVariantRelation : _ancientWispToGreaterWispVariantRelation;

                foreach(var (ancientWispDef, minionDef) in dictionaryToUse)
                {
                    if(bodyVariantController.IsVariantApplied(ancientWispDef))
                    {
                        variantsToUse ??= new List<CharacterVariantDef>();
                        variantsToUse.Add(minionDef);
                    }
                }

                VariantDirectorSpawnRequest variantDirectorSpawnRequest = new VariantDirectorSpawnRequest(toReturn)
                {
                    supressArrivalMessage = true,
                    deathRewardsCoefficient = 1,
                    noGoldExpModifications = true,
                    noItemReward = true,
                    preassignedVariantDefs = variantsToUse?.ToArray()
                };

                return toReturn;
            }

            ILCursor cursor = new ILCursor(il);

            bool matchBeforeDirectorCoreTrySpawnObject = cursor.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject)));

            if(!matchBeforeDirectorCoreTrySpawnObject)
            {
                NWLog.Fatal($"Failed to hook EntityStates.MoffeinAncientWispSkills.Enrage.SummonEnemy(). CharacterVariantDefs relations won't be propagated to summoned monsters!");
                return;
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<DirectorSpawnRequest, Enrage, DirectorSpawnRequest>>(EnsureVariantPropagationToMinions);
        }
    }
}