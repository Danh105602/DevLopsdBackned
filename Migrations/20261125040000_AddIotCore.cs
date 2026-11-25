using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddIotCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    Metric = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LowerThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UpperThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ConsecutiveReadings = table.Column<int>(type: "int", nullable: false),
                    CooldownMinutes = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertRules_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    ApiKeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorDevices_Fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "Fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadingAggregates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SensorDeviceId = table.Column<int>(type: "int", nullable: false),
                    BucketStartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadingCount = table.Column<int>(type: "int", nullable: false),
                    TemperatureCount = table.Column<int>(type: "int", nullable: false),
                    TemperatureAverage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TemperatureMinimum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TemperatureMaximum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AirHumidityCount = table.Column<int>(type: "int", nullable: false),
                    AirHumidityAverage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AirHumidityMinimum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AirHumidityMaximum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoilMoistureCount = table.Column<int>(type: "int", nullable: false),
                    SoilMoistureAverage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoilMoistureMinimum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoilMoistureMaximum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PHCount = table.Column<int>(type: "int", nullable: false),
                    PHAverage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PHMinimum = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PHMaximum = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadingAggregates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadingAggregates_SensorDevices_SensorDeviceId",
                        column: x => x.SensorDeviceId,
                        principalTable: "SensorDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SensorDeviceId = table.Column<int>(type: "int", nullable: false),
                    MessageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MeasuredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TemperatureC = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AirHumidityPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SoilMoisturePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PH = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadings_SensorDevices_SensorDeviceId",
                        column: x => x.SensorDeviceId,
                        principalTable: "SensorDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertRuleId = table.Column<int>(type: "int", nullable: false),
                    SensorDeviceId = table.Column<int>(type: "int", nullable: false),
                    SensorReadingId = table.Column<long>(type: "bigint", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastNotifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SeenAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_AlertRules_AlertRuleId",
                        column: x => x.AlertRuleId,
                        principalTable: "AlertRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_SensorDevices_SensorDeviceId",
                        column: x => x.SensorDeviceId,
                        principalTable: "SensorDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_SensorReadings_SensorReadingId",
                        column: x => x.SensorReadingId,
                        principalTable: "SensorReadings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertRules_FieldId_Metric",
                table: "AlertRules",
                columns: new[] { "FieldId", "Metric" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertRuleId_SensorDeviceId",
                table: "Alerts",
                columns: new[] { "AlertRuleId", "SensorDeviceId" },
                unique: true,
                filter: "[Status] <> 'Resolved'");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SensorDeviceId_TriggeredAtUtc",
                table: "Alerts",
                columns: new[] { "SensorDeviceId", "TriggeredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_SensorReadingId",
                table: "Alerts",
                column: "SensorReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Status",
                table: "Alerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SensorDevices_ApiKeyHash",
                table: "SensorDevices",
                column: "ApiKeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorDevices_DeviceCode",
                table: "SensorDevices",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorDevices_FieldId",
                table: "SensorDevices",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadingAggregates_SensorDeviceId_BucketStartUtc",
                table: "SensorReadingAggregates",
                columns: new[] { "SensorDeviceId", "BucketStartUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_SensorDeviceId_MeasuredAtUtc",
                table: "SensorReadings",
                columns: new[] { "SensorDeviceId", "MeasuredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_SensorDeviceId_MessageId",
                table: "SensorReadings",
                columns: new[] { "SensorDeviceId", "MessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "SensorReadingAggregates");

            migrationBuilder.DropTable(
                name: "AlertRules");

            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "SensorDevices");
        }
    }
}
