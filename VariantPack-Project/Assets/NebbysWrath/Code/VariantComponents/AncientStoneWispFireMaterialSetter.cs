using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using VAPI.Legacy.Components;

namespace NW.Components
{
    public class AncientStoneWispMaterialSetter : VariantComponent
    {
        public static Material fireMaterial;

        private ParticleSystemRenderer _renderer1;
        private ParticleSystemRenderer _renderer2;
        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var request = NWAssets.LoadAssetAsync<Material>("matStoneWispFire");

            while (!request.isDone)
                yield return null;

            fireMaterial = request.asset;
        }

        private void Start()
        {
            if(characterModel && characterModel.TryGetComponent<AncientWispFireController>(out var ctrl))
            {
                _renderer1 = ctrl.normalParticles.GetComponent<ParticleSystemRenderer>();
                _renderer1.material = fireMaterial;
                _renderer2 = ctrl.rageParticles.GetComponent<ParticleSystemRenderer>();
                _renderer2.material = fireMaterial;
            }
        }

        private void OnDestroy()
        {
            if (_renderer1 && _renderer1.material)
                Destroy(_renderer1.material);

            if (_renderer2 && _renderer2.material)
                Destroy(_renderer2.material);
        }
    }
}