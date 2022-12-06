using Moonstorm.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TO30
{
    public class TO30Lang : LanguageLoader<TO30Lang>
    {
        public override string AssemblyDir => TO30Assets.Instance.AssemblyDir;

        public override string LanguagesFolderName => "TO30Lang";

        internal void Init()
        {
            LoadLanguages();
        }
    }
}