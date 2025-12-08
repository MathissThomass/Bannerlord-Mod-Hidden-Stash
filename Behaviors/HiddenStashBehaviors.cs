using System.Collections.Generic;
using HiddenStash.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;

namespace HiddenStash.Behaviors;

public class HiddenStashBehaviors : CampaignBehaviorBase
{
    private Dictionary<string, ItemRoster>? StashInventoryBeforeLost { get; set; }

    public override void RegisterEvents()
    {
        CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChangedEvent);
    }

    public override void SyncData(IDataStore dataStore)
    {
        var stashInventoryBeforeLost = StashInventoryBeforeLost;
        dataStore.SyncData("HiddenStash_StashInventoryBeforeLost", ref stashInventoryBeforeLost);
        StashInventoryBeforeLost = stashInventoryBeforeLost;
    }

    private void OnSettlementOwnerChangedEvent(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner,
        Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
    {
        if (oldOwner == Hero.MainHero && settlement.Stash.Count != 0)
        {
            if (StashInventoryBeforeLost == null)
            {
                StashInventoryBeforeLost = new Dictionary<string, ItemRoster>();
            }

            StashInventoryBeforeLost[settlement.StringId] = InventoryUtils.CloneRoster(settlement.Stash);
        }

        else if (newOwner == Hero.MainHero)
        {
            if (StashInventoryBeforeLost == null || !StashInventoryBeforeLost.TryGetValue(settlement.StringId, out var stash)) return;
            foreach (var item in stash)
            {
                settlement.Stash.Add(item);
            }
        }
    }
}