using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ScavMonster.Dream
{
    public class DreamLuck : BaseState
    {
        private float duration;
        private PickupIndex dropPickup;
        private int itemsToGrant;
        private PickupDisplay pickupDisplay;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = FindItem.baseDuration / attackSpeedStat;
            PlayCrossfade("Body", "SitRummage", "Sit.playbackRate", duration, 0.1f);
            Util.PlaySound(FindItem.sound, gameObject);

            if (isAuthority)
            {
                WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList.Where(new Func<PickupIndex, bool>(PickupIsNonBlacklistedItem)).ToList<PickupIndex>(), FindItem.tier3Chance);

                List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                dropPickup = list[UnityEngine.Random.Range(0, list.Count)];

                PickupDef pickupDef = PickupCatalog.GetPickupDef(dropPickup);
                if (pickupDef != null)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                    if (itemDef != null)
                    {
                        itemsToGrant = 0;

                        switch (itemDef.tier)
                        {
                            case ItemTier.Tier1:
                                itemsToGrant = FindItem.tier1Count;
                                break;
                            case ItemTier.Tier2:
                                itemsToGrant = FindItem.tier2Count;
                                break;
                            case ItemTier.Tier3:
                                itemsToGrant = FindItem.tier3Count;
                                break;
                            default:
                                itemsToGrant = 1;
                                break;
                        }
                    }
                }
            }

            Transform transform = FindModelChild("PickupDisplay");
            pickupDisplay = transform.GetComponent<PickupDisplay>();
            pickupDisplay.SetPickup(new UniquePickup(dropPickup), false);
        }

        public override void OnExit()
        {
            pickupDisplay.SetPickup(new UniquePickup(PickupIndex.none), false);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextState(new GrantItem
                {
                    dropPickup = dropPickup,
                    itemsToGrant = itemsToGrant
                });
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(dropPickup);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            dropPickup = reader.ReadPickupIndex();
        }

        private bool PickupIsNonBlacklistedItem(PickupIndex pickupIndex)
        {
            PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);

            if (pickupDef == null) return false;

            ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
            return itemDef != null && itemDef.DoesNotContainTag(ItemTag.AIBlacklist);
        }
    }
}