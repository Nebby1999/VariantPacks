using R2API;
using RoR2;
using System;
using VAPI;

namespace NW
{
    [Serializable]
    public sealed class StatModifierWithRoot : BasicStatModifier
    {
        public int rootBonus = 0;
        public override object Clone()
        {
            return new StatModifierWithRoot
            {
                healthMultiplier = healthMultiplier,
                armorBonus = armorBonus,
                armorMultiplier = armorMultiplier,
                attackSpeedMultiplier = attackSpeedMultiplier,
                damageMultiplier = damageMultiplier,
                moveSpeedMultiplier = moveSpeedMultiplier,
                regenBonus = regenBonus,
                regenMultiplier = regenMultiplier,
                shieldBonus = shieldBonus,
                shieldMultiplier = shieldMultiplier,
            };
        }

        public override void ApplyStatModifiers(RecalculateStatsAPI.StatHookEventArgs args, CharacterBody targetBody)
        {
            base.ApplyStatModifiers(args, targetBody);
            args.moveSpeedRootCount += rootBonus;
        }
    }
}