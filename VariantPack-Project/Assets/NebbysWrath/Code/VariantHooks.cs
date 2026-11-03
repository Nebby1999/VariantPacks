using RoR2;
using System;
using VAPI;

namespace NW
{
    //Sometimes being lazy is best...
    internal static class VariantHooks
    {
        [SystemInitializer]
        private static void Init()
        {
            On.EntityStates.ExtractorUnit.ExtractLunge.EndAttack += Klepto_StealAgainIfApplicable;
            On.EntityStates.ExtractorUnit.ExtractLunge.OnStealBegin += Klepto_DontChangeState;
        }

        private static void Klepto_DontChangeState(On.EntityStates.ExtractorUnit.ExtractLunge.orig_OnStealBegin orig, EntityStates.ExtractorUnit.ExtractLunge self)
        {
            //don't change the extracting ESM state if we're the klepto variant
            if(self != null && self.TryGetComponent<CharacterBodyVariantController>(out var variantController) && variantController.IsVariantApplied(NWContent.CharacterVariants.KleptomaniacExtractor))
            {
                return;
            }
            orig(self);
        }

        //If we're ending the attack, and we're the kleptomaniac variant
        //We want to step steal again, as long as we have already stolen something.
        private static void Klepto_StealAgainIfApplicable(On.EntityStates.ExtractorUnit.ExtractLunge.orig_EndAttack orig, EntityStates.ExtractorUnit.ExtractLunge self)
        {
            if(self != null && self.TryGetComponent<CharacterBodyVariantController>(out var bodyVariantController) && bodyVariantController.IsVariantApplied(NWContent.CharacterVariants.KleptomaniacExtractor) && self._attackEnded == false)
            {
                self.FindItemStealer();
                if(self._itemStealController && self._itemStealController.FirstStolenItemIndex != ItemIndex.None)
                {
                    self._itemStealController.StepSteal();
                }
            }
            orig(self);
        }
    }
}