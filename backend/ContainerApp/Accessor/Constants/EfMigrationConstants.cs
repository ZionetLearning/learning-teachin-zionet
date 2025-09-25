namespace Accessor.Constants;

public static class EfMigrationConstants
{
    public const string InitialMigrationId = "20250916103944_InitialClean";
    public const string EfCoreVersion = "9.0.8";
    public const string AnchorTableName = "Users";

    public const string CheckIfAnchorTableExistsSql = $@"
        SELECT EXISTS (
            SELECT FROM information_schema.tables
            WHERE table_schema = 'public'
            AND table_name = '{AnchorTableName}'
        );";

    public const string InsertInitialMigrationSql = @"
        INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
        VALUES (@migrationId, @version);
    ";
}
