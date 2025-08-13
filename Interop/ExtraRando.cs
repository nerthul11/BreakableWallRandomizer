using BreakableWallRandomizer.IC;
using BreakableWallRandomizer.Modules;
using ExtraRando.Data;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using ItemChanger;
using ItemChanger.Internal;
using RandomizerCore.Logic;
using RandomizerMod.Extensions;
using System;
using System.Collections.Generic;

namespace BreakableWallRandomizer.Interop;

internal static class ExtraRando_Interop
{
    public static void Hook()
    {
        VictoryModule.RequestConditions += VictoryModule_RequestConditions;
    }

    private static void VictoryModule_RequestConditions(List<IVictoryCondition> conditionList) => conditionList.Add(new WallVictory());
}

public class WallVictory : IVictoryCondition
{
    #region Interface

    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(211, Math.Max(setAmount, 0));

    public string GetMenuName() => "Breakable Walls";

    public string PrepareLogic(LogicManagerBuilder logicBuilder) => $"Total_Broken_Walls>{RequiredAmount - 1}";

    public void StartListening() => BreakableWallModule.Instance.OnWallObtained += CheckForWinCon;

    public void StopListening() => BreakableWallModule.Instance.OnWallObtained -= CheckForWinCon;

    public string GetHintText()
    {
        Dictionary<string, int> leftItems = [];
        foreach (AbstractItem item in Ref.Settings.GetItems())
        {
            if (item.IsObtained())
                continue;

            if (item is BreakableWallItem)
            {
                string area = item.RandoLocation()?.LocationDef?.MapArea ?? "an unknown place.";
                if (!leftItems.ContainsKey(area))
                    leftItems.Add(area, 0);
                leftItems[area]++;
            }
        }
        if (leftItems.Count == 0)
            return null;
        string text = $"You're missing at least {RequiredAmount - CurrentAmount} walls. <br>Randomized walls can be found in these areas:";
        foreach (string item in leftItems.Keys)
            text += $"<br>{leftItems[item]} in {item}";
        return text;
    }

    #endregion

    private void CheckForWinCon(int walls)
    {
        CurrentAmount = walls;
        ItemChangerMod.Modules.Get<VictoryModule>().CheckForFinish();
    }
}