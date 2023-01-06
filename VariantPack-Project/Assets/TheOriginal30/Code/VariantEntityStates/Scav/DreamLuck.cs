using EntityStates;
using EntityStates.ScavMonster;
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
            this.duration = FindItem.baseDuration / this.attackSpeedStat;
            base.PlayCrossfade("Body", "SitRummage", "Sit.playbackRate", this.duration, 0.1f);
            Util.PlaySound(FindItem.sound, base.gameObject);

            if (base.isAuthority)
            {
                WeightedSelection<List<PickupIndex>> weightedSelection = new WeightedSelection<List<PickupIndex>>(8);
                weightedSelection.AddChoice(Run.instance.availableTier3DropList.Where(new Func<PickupIndex, bool>(this.PickupIsNonBlacklistedItem)).ToList<PickupIndex>(), FindItem.tier3Chance);

                List<PickupIndex> list = weightedSelection.Evaluate(UnityEngine.Random.value);
                this.dropPickup = list[UnityEngine.Random.Range(0, list.Count)];

                PickupDef pickupDef = PickupCatalog.GetPickupDef(this.dropPickup);
                if (pickupDef != null)
                {
                    ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
                    if (itemDef != null)
                    {
                        this.itemsToGrant = 0;

                        switch (itemDef.tier)
                        {
                            case ItemTier.Tier1:
                                this.itemsToGrant = FindItem.tier1Count;
                                break;
                            case ItemTier.Tier2:
                                this.itemsToGrant = FindItem.tier2Count;
                                break;
                            case ItemTier.Tier3:
                                this.itemsToGrant = FindItem.tier3Count;
                                break;
                            default:
                                this.itemsToGrant = 1;
                                break;
                        }
                    }
                }
            }

            Transform transform = base.FindModelChild("PickupDisplay");
            this.pickupDisplay = transform.GetComponent<PickupDisplay>();
            this.pickupDisplay.SetPickupIndex(this.dropPickup, false);
        }

        public override void OnExit()
        {
            this.pickupDisplay.SetPickupIndex(PickupIndex.none, false);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextState(new GrantItem
                {
                    dropPickup = this.dropPickup,
                    itemsToGrant = this.itemsToGrant
                });
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.dropPickup);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.dropPickup = reader.ReadPickupIndex();
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