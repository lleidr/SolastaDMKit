namespace SolastaDMKit.Core.Runtime;

public static class SxCampaign
{
    public static string InternalName => Gui.GameCampaign?.campaignDefinition?.Name ?? string.Empty;

    public static string Title => Gui.GameCampaign?.campaignDefinition?.FormatTitle() ?? string.Empty;

    public static bool IsUserCampaign => Gui.GameCampaign?.campaignDefinition?.IsUserCampaign ?? false;

    public static bool IsActive => Gui.GameCampaign != null;

    public static GameCampaign Current => Gui.GameCampaign;
}
