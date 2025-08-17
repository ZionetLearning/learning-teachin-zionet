using System.Text.RegularExpressions;
using ApprovalTests;
using ApprovalTests.Namers;

namespace AccessorUnitTests;

public static class ApprovalSetup
{
    private static readonly Regex GuidRegex =
        new(@"[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}");
    private static readonly Regex IsoTimeRegex =
        new(@"\d{4}\-\d{2}\-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z");

    public static void VerifyJsonClean(string json, string? additionalInfo = null)
    {
        var cleaned = GuidRegex.Replace(json, "GUID");
        cleaned = IsoTimeRegex.Replace(cleaned, "2020-01-01T00:00:00Z");

        if (!string.IsNullOrWhiteSpace(additionalInfo))
            NamerFactory.AdditionalInformation = additionalInfo;

        Approvals.VerifyJson(cleaned);

        if (!string.IsNullOrWhiteSpace(additionalInfo))
            NamerFactory.AdditionalInformation = null;
    }
}
