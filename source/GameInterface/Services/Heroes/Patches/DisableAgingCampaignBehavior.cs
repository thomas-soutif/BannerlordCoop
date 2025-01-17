﻿using HarmonyLib;
using TaleWorlds.CampaignSystem.CampaignBehaviors;

namespace GameInterface.Services.Heroes.Patches;

[HarmonyPatch(typeof(AgingCampaignBehavior))]
internal class DisableAgingCampaignBehavior
{
    [HarmonyPatch(nameof(AgingCampaignBehavior.RegisterEvents))]
    static bool Prefix() => false;
}
