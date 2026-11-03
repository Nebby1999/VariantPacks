using RoR2;
using System;
using UnityEngine;
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
            On.RoR2.HealthComponent.TakeDamageProcess += Pinky_DamageModifier;
        }

        //IOnIncomingDamageServerReciever runs after we set the boolean to ignore armor, so this needs to happen before that.
        private static void Pinky_DamageModifier(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if(self && damageInfo != null && self.TryGetComponent<CharacterBodyVariantController>(out var variantController) && variantController.IsVariantApplied(NWContent.CharacterVariants.Pinky))
            {
                if (!damageInfo.attacker) return;

                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                Vector3 attackerPosition = attackerBody ? attackerBody.corePosition : damageInfo.attacker.transform.position;
                Vector3 between = damageInfo.position - attackerPosition;
                if (BackstabManager.IsBackstab(between, self.body))
                {
                    damageInfo.damageType.damageType |= DamageType.BypassArmor;
                }
            }
            orig(self, damageInfo);
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