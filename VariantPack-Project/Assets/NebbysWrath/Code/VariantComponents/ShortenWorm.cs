using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VAPI.Components;

namespace NW.Components
{
    public class ShortenWorm : VariantComponent
    {
        private void Start()
        {
            if(characterBody && characterBody.TryGetComponent<WormBodyPositions2>(out var wurmPositions))
            {
                ShortenWormLength(wurmPositions);
            }
        }

        private void ShortenWormLength(WormBodyPositions2 wormPositions)
        {
            for(int i = 0; i < wormPositions.segmentLengths.Length; i++)
            {
                wormPositions.segmentLengths[i] /= 2f;
            }
        }
    }
}
