using MSU;
using RoR2;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VAPI.Legacy;

namespace EntityStates.Gup.Goop
{
    public class GoopDeath : BaseSplitDeath
    {
        private static CharacterSpawnCard gupCard;

        private static GupDeath _base;
        [AsyncAssetLoad]
        private static IEnumerator LoadAssets()
        {
            var cardRequest = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/DLC1/Gup/cscGupBody.asset");

            while (!cardRequest.IsDone)
                yield return null;

            gupCard = cardRequest.Result;
        }

        public override void OnEnter()
        {
            _base ??= new GupDeath();
            characterSpawnCard = gupCard;
            spawnCount = _base.spawnCount;
            deathDelay = _base.deathDelay;
            moneyMultiplier = _base.moneyMultiplier;
            base.OnEnter();
        }
    }
}