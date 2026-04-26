using HarmonyLib;
using SolastaDMKit.Core.Runtime;

namespace SolastaDMKit.Core.Patches;

[HarmonyPatch(typeof(UserCampaignEditorScreen), nameof(UserCampaignEditorScreen.Show))]
internal static class UserCampaignEditorScreenShowPatch
{
    internal static void Prefix(UserCampaign campaign)
    {
        SxCampaign.CaptureEditorCampaign(campaign);
    }
}
