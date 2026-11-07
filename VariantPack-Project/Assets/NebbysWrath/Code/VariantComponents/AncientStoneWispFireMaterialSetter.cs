using MSU;
using RoR2;
using System.Collections;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterModel, useOnClient = true)]
    public class AncientStoneWispMaterialSetter : VariantComponent
    {
        public static Material fireMaterial;
        public CharacterModel characterModel { get; private set; }

        private ParticleSystemRenderer _renderer1;
        private ParticleSystemRenderer _renderer2;
        private Material originalMaterial1;
        private Material originalMaterial2;

        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            using var request = NWAssets.LoadAssetAsync<Material>("matStoneWispFire");

            while (!request.isComplete)
                yield return null;

            fireMaterial = request.asset;
        }

        private void Awake()
        {
            characterModel = GetComponent<CharacterModel>();
        }

        private void Start()
        {
            if(characterModel && characterModel.TryGetComponent<AncientWispFireController>(out var ctrl))
            {
                _renderer1 = ctrl.normalParticles.GetComponent<ParticleSystemRenderer>();
                originalMaterial1 = _renderer1.sharedMaterial;
                _renderer1.sharedMaterial = fireMaterial;
                _renderer2 = ctrl.rageParticles.GetComponent<ParticleSystemRenderer>();
                originalMaterial2 = _renderer2.sharedMaterial;
                _renderer2.sharedMaterial = fireMaterial;
            }
        }

        private void OnDestroy()
        {
            if (_renderer1 && _renderer1.material)
            {
                Destroy(_renderer1.material);
                _renderer1.sharedMaterial = originalMaterial1;
            }

            if (_renderer2 && _renderer2.material)
            {
                Destroy(_renderer2.material);
                _renderer2.sharedMaterial = originalMaterial2;
            }
        }
    }
}