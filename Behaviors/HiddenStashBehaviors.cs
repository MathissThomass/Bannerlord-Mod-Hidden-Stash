using Newtonsoft.Json;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;

namespace HiddenStash.Behaviors;

public class HiddenStashBehaviors : CampaignBehaviorBase
{
    private Dictionary<string, Dictionary<string, int>> _stashSnapshots = new();
    private string _stashSnapshotsJson = "{}";

    public override void RegisterEvents()
    {
        CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChangedEvent);
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData("HiddenStash_StashSnapshotsJson", ref _stashSnapshotsJson);
        
        if (dataStore.IsLoading)
        {
            try
            {
                _stashSnapshots = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(_stashSnapshotsJson)
                                 ?? new Dictionary<string, Dictionary<string, int>>();
            }
            catch
            {
                _stashSnapshots = new Dictionary<string, Dictionary<string, int>>();
                _stashSnapshotsJson = "{}";
            }
        }

        if (dataStore.IsSaving)
        {
            _stashSnapshotsJson = JsonConvert.SerializeObject(_stashSnapshots);
        }
    }

    private void OnSettlementOwnerChangedEvent(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner,
        Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
    {
        if (oldOwner == Hero.MainHero && settlement.Stash != null && settlement.Stash.Count > 0)
        {
            var snapshot = new Dictionary<string, int>();

            foreach (var element in settlement.Stash)
            {
                if (element.IsEmpty) continue;
                var item = element.EquipmentElement.Item;
                if (item == null) continue;

                snapshot.TryGetValue(item.StringId, out var current);
                snapshot[item.StringId] = current + element.Amount;
            }

            _stashSnapshots[settlement.StringId] = snapshot;
        }
        else if (newOwner == Hero.MainHero && settlement.Stash != null)
        {
            if (!_stashSnapshots.TryGetValue(settlement.StringId, out var snapshot))
                return;

            foreach (var kvp in snapshot)
            {
                var itemId = kvp.Key;
                var count = kvp.Value;

                var item = Items.All.Find(i => i.StringId == itemId);
                if (item == null) continue;

                settlement.Stash.AddToCounts(item, count);
            }
            _stashSnapshots.Remove(settlement.StringId);
        }
    }
}
