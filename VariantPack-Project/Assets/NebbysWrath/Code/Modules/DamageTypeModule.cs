using MSU;
using R2API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NW.Modules
{
    internal interface IDamageTypeContent : IContentPiece
    {
        DamageAPI.ModdedDamageType assignedModdedDamageType { get; set; }
    }
    internal static class DamageTypeModule
    {
        private static List<IDamageTypeContent> _instancedClasses = new List<IDamageTypeContent>();
        public static IEnumerator Initialize(IContentPieceProvider provider)
        {
            var contents = provider.GetContents().OfType<IDamageTypeContent>();
            List<IDamageTypeContent> initialized = new List<IDamageTypeContent>();
            ParallelMultiStartCoroutine routine = new ParallelMultiStartCoroutine();

            foreach(var content in contents)
            {
                if (!content.IsAvailable(provider.contentPack))
                    continue;

                initialized.Add(content);
                routine.Add(content.LoadContentAsync);
            }

            routine.Start();
            while (!routine.isDone)
                yield return null;

            foreach(var content in initialized)
            {
                content.assignedModdedDamageType = DamageAPI.ReserveDamageType();
                content.Initialize();
                _instancedClasses.Add(content);
            }
        }
    }
}