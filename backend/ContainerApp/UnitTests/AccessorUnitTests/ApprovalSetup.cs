using System.Text.RegularExpressions;
using ApprovalTests;

namespace AccessorUnitTests;

public static class ApprovalSetup
{
    private static readonly Regex GuidRegex =
        new(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
    private static readonly Regex IsoTimeRegex =
        new(@"\d{4}\-\d{2}\-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z");

    /// <summary>
    /// Run Approvals.VerifyJson after scrubbing GUIDs & ISO timestamps.
    /// </summary>
    public static void VerifyJsonClean(string json, string? additionalInfo = null)
    {
        var cleaned = GuidRegex.Replace(json, "GUID");
        cleaned = IsoTimeRegex.Replace(cleaned, "2020-01-01T00:00:00Z");

        if (!string.IsNullOrWhiteSpace(additionalInfo))
            ApprovalTests.Namers.NamerFactory.AdditionalInformation = additionalInfo;

        Approvals.VerifyJson(cleaned);

        if (!string.IsNullOrWhiteSpace(additionalInfo))
            ApprovalTests.Namers.NamerFactory.AdditionalInformation = null;
    }
}
