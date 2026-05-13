using Microsoft.Data.SqlClient;

namespace SaigonAudioTour.Api.Data;

public static class DatabaseHelper
{
    public static bool CanConnectSqlServer(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return false;
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString) { ConnectTimeout = 2 };
            using var conn = new SqlConnection(builder.ConnectionString);
            conn.Open();
            return true;
        }
        catch { return false; }
    }

    public static string ResolveSqlitePath(string contentRootPath, string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            return Path.Combine(contentRootPath, "App_Data", "saigonaudiotour.db");

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(contentRootPath, configuredPath);
    }
}
