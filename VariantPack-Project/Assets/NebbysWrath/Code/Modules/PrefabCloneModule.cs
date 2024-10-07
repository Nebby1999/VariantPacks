using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSU;
using NW.PrefabClones;
using UnityEngine;

namespace NW.Modules
{
    public class ClonedPrefabBehaviour : MonoBehaviour
    {
        public int uniqueIndex;
    }

    internal interface IClonedPrefabContentPiece : IGameObjectContentPiece<ClonedPrefabBehaviour>
    {

    }

    internal static class PrefabCloneModule
    {
        private static List<IClonedPrefabContentPiece> _instancedClasses = new List<IClonedPrefabContentPiece>();
        public static IEnumerator Initialize(IContentPieceProvider<GameObject> contentPieceProvider)
        {
            var contents = contentPieceProvider.GetContents().OfType<IClonedPrefabContentPiece>();

            List<IClonedPrefabContentPiece> initialized = new List<IClonedPrefabContentPiece>();
            ParallelCoroutine routine = new ParallelCoroutine();

            foreach(var content in contents)
            {
                if (!content.IsAvailable(contentPieceProvider.contentPack))
                    continue;

                initialized.Add(content);
                routine.Add(content.LoadContentAsync());
            }

            while (!routine.isDone)
                yield return null;

            foreach(var content in initialized)
            {
                content.Initialize();
                if(content is IContentPackModifier modifier)
                {
                    modifier.ModifyContentPack(contentPieceProvider.contentPack);
                }
                _instancedClasses.Add(content);
            }
        }
    }
}