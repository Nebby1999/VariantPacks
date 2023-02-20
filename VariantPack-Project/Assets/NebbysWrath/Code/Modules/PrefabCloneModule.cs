using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moonstorm;
using NW.PrefabClones;

namespace NW.Modules
{
    internal class PrefabCloneModule : ModuleBase<PrefabCloneBase>
    {
        public static PrefabCloneModule Instance { get; private set; }
        public override void Initialize()
        {
            Instance = this;
            GetPrefabBases()
                .ToList()
                .ForEach(pcb => InitializeContent(pcb));
        }
        private IEnumerable<PrefabCloneBase> GetPrefabBases()
        {
            return GetContentClasses<PrefabCloneBase>();
        }
        protected override void InitializeContent(PrefabCloneBase contentClass)
        {
            contentClass.Initialize();
        }
    }
}