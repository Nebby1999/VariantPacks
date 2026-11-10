using MSU;
using NW.Modules;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NW.PrefabClones
{
    public sealed class ExplosiveGupPreDetonation : IClonedPrefabContentPiece
    {
        public GameObject asset => explosiveVFX;
        public static GameObject explosiveVFX;
        public static GameObject areaIndicator;
        public void Initialize()
        {
            explosiveVFX.transform.localScale *= 2;
            if(explosiveVFX.TryGetComponent<RTPCController>(out var ctrl))
            {
                ref Keyframe last = ref ctrl.rtpcValueCurve.keys[ctrl.rtpcValueCurve.keys.Length - 1];
                last.time *= 1.5f;
            }

            LightIntensityCurve curve = explosiveVFX.GetComponentInChildren<LightIntensityCurve>();
            if(curve)
            {
                curve.timeMax *= 1.5f;
            }

            PostProcessDuration ppDuration = explosiveVFX.GetComponentInChildren<PostProcessDuration>();
            if(ppDuration)
            {
                ppDuration.maxDuration *= 1.5f;
            }

            var particlesystems = explosiveVFX.GetComponentsInChildren<ParticleSystem>();
            foreach(var sys in particlesystems)
            {
                var main = sys.main;
                main.duration *= 1.5f;
            }
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var origEffectRequest = Addressables.LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_QuestVolatileBattery.VolatileBatteryPreDetonation_prefab);
            var areaIndicatorRequest = NWAssets.LoadAssetAsync<GameObject>("ExplosiveGupAreaIndicator");

            ParallelCoroutine coroutine = ParallelCoroutine.RequestFromPool();

            coroutine.Add(origEffectRequest);
            coroutine.Add(areaIndicatorRequest);

            while(!coroutine.isDone)
            {
                yield return null;
            }

            ParallelCoroutine.ReturnToPool(coroutine);

            explosiveVFX = PrefabAPI.InstantiateClone(origEffectRequest.Result, "ExplosiveGupPreDetonation", false);
            areaIndicator = areaIndicatorRequest.asset;
        }
    }
}