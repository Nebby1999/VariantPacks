using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NW
{
    public class NWLang : LanguageLoader<NWLang>
    {
        public override string AssemblyDir => NWAssets.Instance.AssemblyDir;

        public override string LanguagesFolderName => "NWLang";

        internal void Init()
        {
            LoadLanguages();
        }
    }
}