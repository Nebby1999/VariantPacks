using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI;

namespace NW.Components
{
    [AddComponentMenu("")]
    [TargetComponentObject(TargetComponentObjectAttribute.TargetObject.CharacterBody, useOnClient = true, useOnServer = true)]
    public class ShortenWorm : VariantComponent
    {
        public CharacterBody characterBody { get; private set; }
        public WormBodyPositions2 positions2 { get; private set; }

        private void Awake()
        {
            characterBody = GetComponent<CharacterBody>();
            positions2 = GetComponent<WormBodyPositions2>();
        }

        private void Start()
        {
            if(characterBody && positions2)
            {
                ShortenWormLength();
            }
        }

        private void ShortenWormLength()
        {
            for(int i = 0; i < positions2.segmentLengths.Length; i++)
            {
                positions2.segmentLengths[i] /= 1.5f;
            }
        }

        private void OnDestroy()
        {
            if (positions2)
            {
                for(int i = 0; i < positions2.segmentLengths.Length; i++)
                {
                    positions2.segmentLengths[i] *= 1.5f;
                }
            }
        }
    }
}
