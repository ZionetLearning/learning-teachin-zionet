// Put this file in the root of the AccessorUnitTests project (same folder as the .csproj)
using ApprovalTests.Namers;
using ApprovalTests.Reporters;

// Use a diff tool reporter (works locally; harmless in CI if no diff tool is present)
[assembly: UseReporter(typeof(DiffReporter))]

// Store snapshots under a dedicated folder in the test project
[assembly: UseApprovalSubdirectory("Approvals")]
