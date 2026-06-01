using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { status = "KrishiAI Sync API running" }));

app.MapPost("/api/detection-history/create", async (SyncDetectionRequest req) =>
{
    var conn = BuildConnectionString(builder.Configuration);
    if (string.IsNullOrWhiteSpace(conn))
    {
        return Results.Problem("Database not configured. Set ConnectionStrings__KrishiSql or KRISHI_SQL_* environment variables.");
    }

    var remoteId = Guid.NewGuid();
    await using var sql = new SqlConnection(conn);
    await sql.OpenAsync();
    await EnsureDeviceColumnsAsync(sql);

    const string insertSql = @"
INSERT INTO dbo.DiseaseHistory (
    ImagePath, DiseaseName, Confidence, Severity, DetectedDate,
    Description, AffectedCropPart, RemoteId, IsSynced, LastSyncTime,
    IsDeleted, SyncRetryCount, SyncError, CloudImageUrl, ImageUploaded,
    CreatedDateUtc, LastModifiedDateUtc, Version, DeviceId, DeviceName
)
VALUES (
    @ImagePath, @DiseaseName, @Confidence, @Severity, @DetectedDate,
    @Description, @AffectedCropPart, @RemoteId, 1, SYSUTCDATETIME(),
    0, 0, NULL, @CloudImageUrl, CASE WHEN @CloudImageUrl IS NULL THEN 0 ELSE 1 END,
    SYSUTCDATETIME(), @LastModifiedDateUtc, @Version, @DeviceId, @DeviceName
);";

    await using var cmd = new SqlCommand(insertSql, sql);
    cmd.Parameters.AddWithValue("@ImagePath", string.IsNullOrWhiteSpace(req.ImagePath) ? string.Empty : req.ImagePath);
    cmd.Parameters.AddWithValue("@DiseaseName", req.DiseaseName);
    cmd.Parameters.AddWithValue("@Confidence", req.Confidence);
    cmd.Parameters.AddWithValue("@Severity", req.Severity);
    cmd.Parameters.AddWithValue("@DetectedDate", req.DetectedDate.ToUniversalTime());
    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(req.Description) ? string.Empty : req.Description);
    cmd.Parameters.AddWithValue("@AffectedCropPart", string.IsNullOrWhiteSpace(req.AffectedCropPart) ? string.Empty : req.AffectedCropPart);
    cmd.Parameters.AddWithValue("@RemoteId", remoteId);
    cmd.Parameters.AddWithValue("@CloudImageUrl", (object?)req.CloudImageUrl ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@LastModifiedDateUtc", req.LastModifiedDateUtc.ToUniversalTime());
    cmd.Parameters.AddWithValue("@Version", req.Version <= 0 ? 1 : req.Version);
    cmd.Parameters.AddWithValue("@DeviceId", (object?)req.DeviceId ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@DeviceName", (object?)req.DeviceName ?? DBNull.Value);
    await cmd.ExecuteNonQueryAsync();

    return Results.Ok(new { id = remoteId.ToString() });
});

app.MapPost("/api/detection-history/update", async (SyncDetectionRequest req) =>
{
    var conn = BuildConnectionString(builder.Configuration);
    if (string.IsNullOrWhiteSpace(conn))
    {
        return Results.Problem("Database not configured. Set ConnectionStrings__KrishiSql or KRISHI_SQL_* environment variables.");
    }

    if (string.IsNullOrWhiteSpace(req.RemoteId) || !Guid.TryParse(req.RemoteId, out var remoteGuid))
    {
        return Results.BadRequest(new { error = "remoteId is required for update." });
    }

    await using var sql = new SqlConnection(conn);
    await sql.OpenAsync();
    await EnsureDeviceColumnsAsync(sql);

    const string updateSql = @"
UPDATE dbo.DiseaseHistory
SET DiseaseName = @DiseaseName,
    Confidence = @Confidence,
    Severity = @Severity,
    Description = @Description,
    AffectedCropPart = @AffectedCropPart,
    CloudImageUrl = @CloudImageUrl,
    LastModifiedDateUtc = @LastModifiedDateUtc,
    LastSyncTime = SYSUTCDATETIME(),
    IsSynced = 1,
    SyncRetryCount = 0,
    SyncError = NULL,
    Version = @Version,
    DeviceId = @DeviceId,
    DeviceName = @DeviceName
WHERE RemoteId = @RemoteId;";

    await using var cmd = new SqlCommand(updateSql, sql);
    cmd.Parameters.AddWithValue("@RemoteId", remoteGuid);
    cmd.Parameters.AddWithValue("@DiseaseName", req.DiseaseName);
    cmd.Parameters.AddWithValue("@Confidence", req.Confidence);
    cmd.Parameters.AddWithValue("@Severity", req.Severity);
    cmd.Parameters.AddWithValue("@Description", (object?)req.Description ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@AffectedCropPart", (object?)req.AffectedCropPart ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@CloudImageUrl", (object?)req.CloudImageUrl ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@LastModifiedDateUtc", req.LastModifiedDateUtc.ToUniversalTime());
    cmd.Parameters.AddWithValue("@Version", req.Version <= 0 ? 1 : req.Version);
    cmd.Parameters.AddWithValue("@DeviceId", (object?)req.DeviceId ?? DBNull.Value);
    cmd.Parameters.AddWithValue("@DeviceName", (object?)req.DeviceName ?? DBNull.Value);

    var rows = await cmd.ExecuteNonQueryAsync();
    if (rows == 0)
    {
        return Results.NotFound(new { error = "Remote record not found." });
    }

    return Results.Ok(new { id = remoteGuid.ToString() });
});

app.MapDelete("/api/detection-history/{remoteId}", async (string remoteId) =>
{
    var conn = BuildConnectionString(builder.Configuration);
    if (string.IsNullOrWhiteSpace(conn))
    {
        return Results.Problem("Database not configured. Set ConnectionStrings__KrishiSql or KRISHI_SQL_* environment variables.");
    }

    if (!Guid.TryParse(remoteId, out var remoteGuid))
    {
        return Results.BadRequest(new { error = "Invalid remoteId." });
    }

    await using var sql = new SqlConnection(conn);
    await sql.OpenAsync();

    const string deleteSql = @"
UPDATE dbo.DiseaseHistory
SET IsDeleted = 1,
    LastModifiedDateUtc = SYSUTCDATETIME(),
    LastSyncTime = SYSUTCDATETIME(),
    IsSynced = 1
WHERE RemoteId = @RemoteId;";

    await using var cmd = new SqlCommand(deleteSql, sql);
    cmd.Parameters.AddWithValue("@RemoteId", remoteGuid);
    var rows = await cmd.ExecuteNonQueryAsync();

    return rows > 0 ? Results.NoContent() : Results.NotFound(new { error = "Remote record not found." });
});

app.MapGet("/api/detection-history/list", async (DateTime? since) =>
{
    var conn = BuildConnectionString(builder.Configuration);
    if (string.IsNullOrWhiteSpace(conn))
    {
        return Results.Problem("Database not configured. Set ConnectionStrings__KrishiSql or KRISHI_SQL_* environment variables.");
    }

    await using var sql = new SqlConnection(conn);
    await sql.OpenAsync();
    await EnsureDeviceColumnsAsync(sql);

    var list = new List<DetectionHistoryDto>();
    var query = since.HasValue
        ? @"SELECT * FROM dbo.DiseaseHistory WHERE IsDeleted = 0 AND LastModifiedDateUtc >= @Since ORDER BY LastModifiedDateUtc ASC"
        : @"SELECT * FROM dbo.DiseaseHistory WHERE IsDeleted = 0 ORDER BY LastModifiedDateUtc ASC";

    await using var cmd = new SqlCommand(query, sql);
    if (since.HasValue)
    {
        cmd.Parameters.AddWithValue("@Since", since.Value.ToUniversalTime());
    }

    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        list.Add(new DetectionHistoryDto
        {
            Id = reader["Id"] == DBNull.Value ? 0 : (int)reader["Id"],
            DiseaseName = reader["DiseaseName"] == DBNull.Value ? string.Empty : (string)reader["DiseaseName"],
            Confidence = reader["Confidence"] == DBNull.Value ? 0 : Convert.ToDouble(reader["Confidence"]),
            Severity = reader["Severity"] == DBNull.Value ? string.Empty : (string)reader["Severity"],
            DetectedDate = reader["DetectedDate"] == DBNull.Value ? DateTime.UtcNow : (DateTime)reader["DetectedDate"],
            Description = reader["Description"] == DBNull.Value ? string.Empty : (string)reader["Description"],
            AffectedCropPart = reader["AffectedCropPart"] == DBNull.Value ? string.Empty : (string)reader["AffectedCropPart"],
            RemoteId = reader["RemoteId"] == DBNull.Value ? null : reader["RemoteId"].ToString(),
            IsSynced = reader["IsSynced"] != DBNull.Value && (bool)reader["IsSynced"],
            LastSyncTime = reader["LastSyncTime"] == DBNull.Value ? null : (DateTime?)reader["LastSyncTime"],
            IsDeleted = reader["IsDeleted"] != DBNull.Value && (bool)reader["IsDeleted"],
            SyncRetryCount = reader["SyncRetryCount"] == DBNull.Value ? 0 : (int)reader["SyncRetryCount"],
            SyncError = reader["SyncError"] == DBNull.Value ? null : reader["SyncError"].ToString(),
            CloudImageUrl = reader["CloudImageUrl"] == DBNull.Value ? null : reader["CloudImageUrl"].ToString(),
            ImageUploaded = reader["ImageUploaded"] != DBNull.Value && (bool)reader["ImageUploaded"],
            CreatedDateUtc = reader["CreatedDateUtc"] == DBNull.Value ? DateTime.UtcNow : (DateTime)reader["CreatedDateUtc"],
            LastModifiedDateUtc = reader["LastModifiedDateUtc"] == DBNull.Value ? DateTime.UtcNow : (DateTime)reader["LastModifiedDateUtc"],
            Version = reader["Version"] == DBNull.Value ? 1 : (int)reader["Version"],
            DeviceId = reader["DeviceId"] == DBNull.Value ? null : reader["DeviceId"].ToString(),
            DeviceName = reader["DeviceName"] == DBNull.Value ? null : reader["DeviceName"].ToString()
        });
    }

    return Results.Ok(list);
});

app.MapPost("/api/images/upload", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new { error = "Form data expected." });
    }

    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    if (file is null)
    {
        return Results.BadRequest(new { error = "Missing file." });
    }

    // Placeholder URL contract for client sync flow.
    var url = $"https://local-upload.invalid/{Guid.NewGuid():N}/{file.FileName}";
    return Results.Ok(new { url });
});

app.Run();

static string BuildConnectionString(IConfiguration configuration)
{
    // Preferred source: environment/user-secrets via ConnectionStrings__KrishiSql.
    var configured = configuration.GetConnectionString("KrishiSql");
    if (!string.IsNullOrWhiteSpace(configured))
    {
        return configured;
    }

    // Backward-compatible fallback for split env vars.
    var server = Environment.GetEnvironmentVariable("KRISHI_SQL_SERVER");
    var database = Environment.GetEnvironmentVariable("KRISHI_SQL_DATABASE");
    var user = Environment.GetEnvironmentVariable("KRISHI_SQL_USER");
    var password = Environment.GetEnvironmentVariable("KRISHI_SQL_PASSWORD");

    if (string.IsNullOrWhiteSpace(server) ||
        string.IsNullOrWhiteSpace(database) ||
        string.IsNullOrWhiteSpace(user) ||
        string.IsNullOrWhiteSpace(password))
    {
        return string.Empty;
    }

    return $"Server={server};Database={database};User Id={user};Password={password};Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;";
}

static async Task EnsureDeviceColumnsAsync(SqlConnection sql)
{
    const string ensureSql = @"
IF COL_LENGTH('dbo.DiseaseHistory', 'DeviceId') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD DeviceId NVARCHAR(128) NULL;

IF COL_LENGTH('dbo.DiseaseHistory', 'DeviceName') IS NULL
    ALTER TABLE dbo.DiseaseHistory ADD DeviceName NVARCHAR(256) NULL;";

    await using var cmd = new SqlCommand(ensureSql, sql);
    await cmd.ExecuteNonQueryAsync();
}

sealed class SyncDetectionRequest
{
    public int LocalId { get; set; }
    public string? RemoteId { get; set; }
    public string DiseaseName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
    public string? Description { get; set; }
    public string? AffectedCropPart { get; set; }
    public string? CloudImageUrl { get; set; }
    public DateTime LastModifiedDateUtc { get; set; }
    public int Version { get; set; }
    public string? ImagePath { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
}

sealed class DetectionHistoryDto
{
    public int Id { get; set; }
    public string DiseaseName { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime DetectedDate { get; set; }
    public string? Description { get; set; }
    public string? AffectedCropPart { get; set; }
    public string? RemoteId { get; set; }
    public bool IsSynced { get; set; }
    public DateTime? LastSyncTime { get; set; }
    public bool IsDeleted { get; set; }
    public int SyncRetryCount { get; set; }
    public string? SyncError { get; set; }
    public string? CloudImageUrl { get; set; }
    public bool ImageUploaded { get; set; }
    public DateTime CreatedDateUtc { get; set; }
    public DateTime LastModifiedDateUtc { get; set; }
    public int Version { get; set; }
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
}
