using NW.Modules;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NW.DamageTypes
{
    public class PulverizeOnHit : IDamageTypeContent
    {
        public DamageAPI.ModdedDamageType assignedModdedDamageType { get; set; }

        public static DamageAPI.ModdedDamageType pulverizeOnHit;

        private void Pulverize(DamageReport obj)
        {
            var victimBody = obj.victimBody;
            var damageInfo = obj.damageInfo;
            if(damageInfo.HasModdedDamageType(pulverizeOnHit))
            {
                victimBody.AddTimedBuff(RoR2Content.Buffs.Pulverized, 16 * damageInfo.procCoefficient);
            }
        }

        public IEnumerator LoadContentAsync()
        {
            yield break;
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public void Initialize()
        {
            pulverizeOnHit = assignedModdedDamageType;
            GlobalEventManager.onServerDamageDealt += Pulverize;
        }
    }
}