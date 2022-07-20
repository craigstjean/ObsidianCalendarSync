using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CalendarSync.Migrations
{
    public partial class Initial_Public : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventBodies",
                columns: table => new
                {
                    EventBodyId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventBodies", x => x.EventBodyId);
                });

            migrationBuilder.CreateTable(
                name: "EventFilters",
                columns: table => new
                {
                    EventFilterId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Search = table.Column<string>(type: "TEXT", nullable: false),
                    IsIgnore = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPersonal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventFilters", x => x.EventFilterId);
                });

            migrationBuilder.CreateTable(
                name: "EventLocations",
                columns: table => new
                {
                    EventLocationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    LocationUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLocations", x => x.EventLocationId);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ICalUid = table.Column<string>(type: "TEXT", nullable: false),
                    Uid = table.Column<string>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAllDay = table.Column<bool>(type: "INTEGER", nullable: false),
                    EventBodyId = table.Column<int>(type: "INTEGER", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    OnlineMeetingUrl = table.Column<string>(type: "TEXT", nullable: true),
                    EventLocationId = table.Column<int>(type: "INTEGER", nullable: true),
                    ShowAs = table.Column<string>(type: "TEXT", nullable: false),
                    IsCancelled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_EventBodies_EventBodyId",
                        column: x => x.EventBodyId,
                        principalTable: "EventBodies",
                        principalColumn: "EventBodyId");
                    table.ForeignKey(
                        name: "FK_Events_EventLocations_EventLocationId",
                        column: x => x.EventLocationId,
                        principalTable: "EventLocations",
                        principalColumn: "EventLocationId");
                });

            migrationBuilder.CreateTable(
                name: "AdditionalDatas",
                columns: table => new
                {
                    AdditionalDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    EventBodyId = table.Column<int>(type: "INTEGER", nullable: true),
                    EventId = table.Column<int>(type: "INTEGER", nullable: true),
                    EventLocationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdditionalDatas", x => x.AdditionalDataId);
                    table.ForeignKey(
                        name: "FK_AdditionalDatas_EventBodies_EventBodyId",
                        column: x => x.EventBodyId,
                        principalTable: "EventBodies",
                        principalColumn: "EventBodyId");
                    table.ForeignKey(
                        name: "FK_AdditionalDatas_EventLocations_EventLocationId",
                        column: x => x.EventLocationId,
                        principalTable: "EventLocations",
                        principalColumn: "EventLocationId");
                    table.ForeignKey(
                        name: "FK_AdditionalDatas_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId");
                });

            migrationBuilder.InsertData(
                table: "EventFilters",
                columns: new[] { "EventFilterId", "IsIgnore", "IsPersonal", "Search" },
                values: new object[] { 1, false, true, "Dentist" });

            migrationBuilder.InsertData(
                table: "EventFilters",
                columns: new[] { "EventFilterId", "IsIgnore", "IsPersonal", "Search" },
                values: new object[] { 2, false, true, "Orthodontist" });

            migrationBuilder.InsertData(
                table: "EventFilters",
                columns: new[] { "EventFilterId", "IsIgnore", "IsPersonal", "Search" },
                values: new object[] { 3, false, true, "Therapist" });

            migrationBuilder.InsertData(
                table: "EventFilters",
                columns: new[] { "EventFilterId", "IsIgnore", "IsPersonal", "Search" },
                values: new object[] { 4, false, true, "Therapy" });

            migrationBuilder.InsertData(
                table: "EventFilters",
                columns: new[] { "EventFilterId", "IsIgnore", "IsPersonal", "Search" },
                values: new object[] { 5, false, true, "Doctor" });

            migrationBuilder.InsertData(
                table: "EventFilters",
                columns: new[] { "EventFilterId", "IsIgnore", "IsPersonal", "Search" },
                values: new object[] { 6, false, true, "Dermatologist" });

            migrationBuilder.InsertData(
                table: "EventFilters",
                columns: new[] { "EventFilterId", "IsIgnore", "IsPersonal", "Search" },
                values: new object[] { 7, true, true, "Change furnace filters" });

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalDatas_EventBodyId",
                table: "AdditionalDatas",
                column: "EventBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalDatas_EventId",
                table: "AdditionalDatas",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalDatas_EventLocationId",
                table: "AdditionalDatas",
                column: "EventLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventBodyId",
                table: "Events",
                column: "EventBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventLocationId",
                table: "Events",
                column: "EventLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ICalUid_Uid",
                table: "Events",
                columns: new[] { "ICalUid", "Uid" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdditionalDatas");

            migrationBuilder.DropTable(
                name: "EventFilters");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EventBodies");

            migrationBuilder.DropTable(
                name: "EventLocations");
        }
    }
}
