using Moonstorm;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NW.DamageTypes
{
    public class PulverizeOnHit : DamageTypeBase
    {
        public override DamageAPI.ModdedDamageType ModdedDamageType { get; protected set; }

        public static DamageAPI.ModdedDamageType pulverizeOnHit;
        public override void Delegates()
        {
            pulverizeOnHit = ModdedDamageType;
            GlobalEventManager.onServerDamageDealt += Pulverize;
        }

        private void Pulverize(DamageReport obj)
        {
            var victimBody = obj.victimBody;
            var damageInfo = obj.damageInfo;
            if(damageInfo.HasModdedDamageType(pulverizeOnHit))
            {
                victimBody.AddTimedBuff(RoR2Content.Buffs.Pulverized, 16 * damageInfo.procCoefficient);
            }
        }
    }
}