using Moonstorm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NW.Modules
{
    public class DamageTypeModule : DamageTypeModuleBase
    {
        public override void Initialize()
        {
            base.Initialize();
            GetDamageTypeBases();
        }

        protected override IEnumerable<DamageTypeBase> GetDamageTypeBases()
        {
            base.GetDamageTypeBases()
                .ToList()
                .ForEach(dtb => AddDamageType(dtb));
            return null;
        }
    }
}