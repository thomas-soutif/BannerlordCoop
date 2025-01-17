﻿using Common;
using Common.Messaging;
using Common.Util;
using GameInterface.Policies;
using GameInterface.Services.Template.Messages;
using TaleWorlds.CampaignSystem;

namespace GameInterface.Services.Template.Patches;

/// <summary>
/// TODO fill me out and uncomment HarmonyPatch attributes
/// </summary>
//[HarmonyPatch(typeof(Campaign))]
class TemplatePatch
{
    // See https://harmony.pardeike.net/articles/intro.html on how to use harmony patches
    //[HarmonyPatch("TimeControlMode")]
    //[HarmonyPatch(MethodType.Setter)]
    private static bool Prefix(ref Campaign __instance)
    {
        // Returning true in a prefix calls the original function (after all other patches)
        if (AllowedThread.IsThisThreadAllowed()) return true;

        // returns true if when the client state is not in Campaign or Mission to allow original calls
        if (PolicyProvider.AllowOriginalCalls) return true;

        // Publishing a message to all internal software is done using the message broker
        // This type of message should be IEvent since it is a reaction to something
        // Normally sent to a handler in Coop.Core
        MessageBroker.Instance.Publish(__instance, new TemplateEventMessage());

        // Returning false in a prefix will skip the original
        return false;
    }

    public static void OverrideTemplateFn()
    {
        GameLoopRunner.RunOnMainThread(() =>
        {
            // Allowed thread will call the original function rather than skip or do patch functionality
            // See if (AllowedThread.IsThisThreadAllowed()) return true; in the method above
            using (new AllowedThread())
            {
                // Do something with the patched instance here
            }
        }, blocking: true);


        // This is equivalant to the using statement above
        // Only one version is needed
        GameLoopRunner.RunOnMainThread(() =>
        {
            AllowedThread.AllowThisThread();
            // Do something with the patched instance here
            AllowedThread.RevokeThisThread();
        }, blocking: true);
    }
}
