namespace SolastaDMKit.Core.Runtime;

public readonly struct SxCheckResult
{
    public readonly bool Success;
    public readonly int RawRoll;
    public readonly int Total;
    public readonly int DC;
    public readonly bool IsCriticalSuccess;
    public readonly bool IsCriticalFailure;

    public SxCheckResult(bool success, int rawRoll, int total, int dc, bool criticalSuccess, bool criticalFailure)
    {
        Success = success;
        RawRoll = rawRoll;
        Total = total;
        DC = dc;
        IsCriticalSuccess = criticalSuccess;
        IsCriticalFailure = criticalFailure;
    }

    public static SxCheckResult Invalid(int dc = 0) => new(false, 0, 0, dc, false, false);

    public override string ToString()
    {
        var outcome = IsCriticalSuccess ? "crit success"
            : IsCriticalFailure ? "crit failure"
            : Success ? "success"
            : "failure";
        return $"{outcome}: raw={RawRoll} total={Total} dc={DC}";
    }
}
