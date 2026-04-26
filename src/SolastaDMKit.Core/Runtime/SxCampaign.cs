using System.Text;

namespace SolastaDMKit.Core.Runtime;

public static class SxCampaign
{
    private const string PlaytestSentinel = "UserCampaignPlayTest";

    private static UserCampaign _editorCampaign;

    /// <summary>
    /// The internal name of the currently active campaign — the key used to scope
    /// scripts. Resolution order:
    ///   1. UserCampaign.InternalName, if populated (mod.io-published campaigns)
    ///   2. Slugified UserCampaign.Title (locally-authored campaigns where the JSON
    ///      has internalName="" — playtest, draft user campaigns)
    ///   3. Engine campaign-definition Name (built-in campaigns)
    /// </summary>
    public static string InternalName
    {
        get
        {
            var src = SourceUserCampaign;
            if (src != null)
            {
                if (!string.IsNullOrEmpty(src.InternalName))
                {
                    return src.InternalName;
                }

                if (!string.IsNullOrEmpty(src.Title))
                {
                    return Slugify(src.Title);
                }
            }

            return Gui.GameCampaign?.campaignDefinition?.Name ?? string.Empty;
        }
    }

    /// <summary>
    /// What the engine itself reports (may be "UserCampaignPlayTest" during editor playtest).
    /// </summary>
    public static string EngineInternalName =>
        Gui.GameCampaign?.campaignDefinition?.Name ?? string.Empty;

    public static bool IsPlaytest => EngineInternalName == PlaytestSentinel;

    public static string Title => Gui.GameCampaign?.campaignDefinition?.FormatTitle() ?? string.Empty;

    public static bool IsUserCampaign => Gui.GameCampaign?.campaignDefinition?.IsUserCampaign ?? false;

    public static bool IsActive => Gui.GameCampaign != null;

    public static GameCampaign Current => Gui.GameCampaign;

    public static UserCampaign SourceUserCampaign
    {
        get
        {
            var fromLocation = Gui.GameLocation?.UserCampaign;
            if (HasUsableData(fromLocation))
            {
                return fromLocation;
            }

            var fromSession = Gui.Session?.UserCampaign;
            if (HasUsableData(fromSession))
            {
                return fromSession;
            }

            var fromService = ServiceRepository.GetService<ISessionService>()?.Session?.UserCampaign;
            if (HasUsableData(fromService))
            {
                return fromService;
            }

            return _editorCampaign;
        }
    }

    public static UserCampaign EditorCapturedCampaign => _editorCampaign;

    public static void CaptureEditorCampaign(UserCampaign campaign)
    {
        if (campaign != null)
        {
            _editorCampaign = campaign;
        }
    }

    public static string ResolutionDiagnostic()
    {
        var loc = Gui.GameLocation?.UserCampaign;
        var sess = Gui.Session?.UserCampaign;
        var svc = ServiceRepository.GetService<ISessionService>()?.Session?.UserCampaign;
        var editor = _editorCampaign;
        var engine = EngineInternalName;
        var resolved = InternalName;

        return $"resolved='{resolved}' engine='{engine}' | LOC[{Describe(loc)}] | SESS[{Describe(sess)}] | SVC[{Describe(svc)}] | EDIT[{Describe(editor)}]";
    }

    /// <summary>
    /// Lowercases and replaces non-alphanumeric runs with single underscores. Used to
    /// derive a stable filesystem-safe key from a campaign Title. "Nonsense Quest" →
    /// "nonsense_quest"; "Foo: Bar!" → "foo_bar".
    /// </summary>
    public static string Slugify(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(s.Length);
        var lastWasUnderscore = false;
        foreach (var c in s)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
                lastWasUnderscore = false;
            }
            else if (!lastWasUnderscore && sb.Length > 0)
            {
                sb.Append('_');
                lastWasUnderscore = true;
            }
        }

        if (sb.Length > 0 && sb[sb.Length - 1] == '_')
        {
            sb.Length -= 1;
        }

        return sb.ToString();
    }

    private static bool HasUsableData(UserCampaign uc)
    {
        return uc != null && (!string.IsNullOrEmpty(uc.InternalName) || !string.IsNullOrEmpty(uc.Title));
    }

    private static string Describe(UserCampaign uc)
    {
        if (uc == null)
        {
            return "<null>";
        }

        return $"InternalName='{uc.InternalName ?? "<null>"}' Title='{uc.Title ?? "<null>"}'";
    }
}
