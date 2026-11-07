using MSU;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NW
{
    public sealed class WingUnitHaloCreator : MonoBehaviour
    {
        public AssetReferenceT<GameObject> haloFBX;
        public AssetReferenceT<Material> haloMaterial;

        private AsyncOperationHandle<GameObject> _haloFBXHandle;
        private AsyncOperationHandle<Material> _haloMaterialHandle;

        private GameObject _haloInstance;
        private void Start()
        {
            StartCoroutine(InstantiateHaloOnSelf());
        }

        private IEnumerator InstantiateHaloOnSelf()
        {
            _haloFBXHandle = AssetAsyncReferenceManager<GameObject>.LoadAsset(haloFBX, AsyncReferenceHandleUnloadType.OnSceneUnload);
            _haloMaterialHandle = AssetAsyncReferenceManager<Material>.LoadAsset(haloMaterial, AsyncReferenceHandleUnloadType.OnSceneUnload);

            ParallelCoroutine subroutine = ParallelCoroutine.RequestFromPool();
            subroutine.Add(_haloMaterialHandle);
            subroutine.Add(_haloFBXHandle);

            while(!subroutine.isDone)
            {
                yield return null;
            }

            _haloInstance = Instantiate(_haloFBXHandle.Result);
            _haloInstance.transform.SetParent(transform);
            _haloInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var child1 = _haloInstance.transform.GetChild(0);
            if(child1 != null && child1.TryGetComponent<SkinnedMeshRenderer>(out var skinnedMeshRenderer))
            {
                skinnedMeshRenderer.sharedMaterial = _haloMaterialHandle.Result;
            }
        }

        private void OnDestroy()
        {
            if(_haloInstance)
            {
                Destroy(_haloInstance);
            }

            AssetAsyncReferenceManager<GameObject>.UnloadAsset(haloFBX);
            AssetAsyncReferenceManager<Material>.UnloadAsset(haloMaterial);
        }
    }
}