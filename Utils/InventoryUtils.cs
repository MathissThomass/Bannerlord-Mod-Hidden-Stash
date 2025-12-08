using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace HiddenStash.Utils;

public abstract class InventoryUtils
{
    /// <summary>
    /// Clone a roster to make a snapshot of it
    /// </summary>
    /// <param name="source">The roster to copy</param>
    /// <returns>Cloned roster</returns>
    public static ItemRoster CloneRoster(ItemRoster source)
    {
        var clone = new ItemRoster();

        foreach (var element in source)
        {
            if (element.IsEmpty)
                continue;

            var eq = element.EquipmentElement;
            var amount = element.Amount;

            clone.Add(new ItemRosterElement(eq, amount));
        }

        return clone;
    }
}