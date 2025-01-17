﻿using Common;
using Common.Logging;
using Common.Messaging;
using Common.Util;
using GameInterface.Services.ItemRosters.Messages;
using HarmonyLib;
using Serilog;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;

namespace GameInterface.Services.ItemRosters.Patches
{
    [HarmonyPatch(typeof(ItemRoster))]
    internal class ItemRosterPatch
    {
        private static readonly ILogger Logger = LogManager.GetLogger<ItemRosterPatch>();

        [HarmonyPatch(nameof(ItemRoster.AddToCounts), new[] { typeof(EquipmentElement), typeof(int) })]
        [HarmonyPrefix]
        public static bool AddToCountsPrefix(ItemRoster __instance, ref int __result, EquipmentElement rosterElement, int number)
        {
            if (AllowedThread.IsThisThreadAllowed()) return true; // Run if allowed

            if (ModInformation.IsClient)
            {
                __result = -1;
                return false; // Disallow not allowed clients
            }

            return true; // Allow server calls
        }

        [HarmonyPatch(nameof(ItemRoster.AddToCounts), new[] { typeof(EquipmentElement), typeof(int) })]
        [HarmonyPostfix]
        public static void AddToCountsPostfix(ItemRoster __instance, ref int __result, EquipmentElement rosterElement, int number)
        {
            if (ModInformation.IsClient)
            {
                return;
            }

            if (__result == -1)
            {
                return; // Don't publish unsucessful calls
            }

            if (ItemRosterLookup.TryGetValue(__instance, out var partyBase) == false)
            {
                Logger.Error("Unable to find party from item roster");
                return;
            }

            MessageBroker.Instance.Publish(__instance, new ItemRosterUpdated(
                        partyBase.Id,
                        rosterElement.Item.StringId,
                        rosterElement.ItemModifier?.StringId,
                        number
                ));
        }

        public static void AddToCountsOverride(ItemRoster itemRoster, EquipmentElement rosterElement, int amount)
        {
            GameLoopRunner.RunOnMainThread(() =>
            {
                using (new AllowedThread())
                {
                    itemRoster.AddToCounts(rosterElement, amount);
                }
            });
        }
    }
}
