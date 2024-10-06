using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NW.Items
{
    public class ApplyTarOnHit : IItemContentPiece
    {
        public NullableRef<List<GameObject>> itemDisplayPrefabs => default;

        public ItemDef asset { get; private set; }

        public void Initialize()
        {
            GlobalEventManager.onServerDamageDealt += ApplyTar;
        }

        private void ApplyTar(DamageReport obj)
        {
            if(obj.attackerBody && obj.attackerBody.TryGetItemCount(asset, out var itemCount))
            {
                if (!obj.victimBody)
                    return;

                obj.victimBody.AddTimedBuff(RoR2Content.Buffs.ClayGoo, 5);
            }
        }

        public bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public IEnumerator LoadContentAsync()
        {
            var rqst = NWAssets.LoadAssetAsync<ItemDef>("ApplyTarOnHit");

            while (!rqst.isDone)
                yield return null;

            asset = rqst.asset;
        }
    }
}