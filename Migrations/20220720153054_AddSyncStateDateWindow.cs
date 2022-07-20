using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CalendarSync.Migrations
{
    public partial class AddSyncStateDateWindow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndWindow",
                table: "SyncStates",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartWindow",
                table: "SyncStates",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndWindow",
                table: "SyncStates");

            migrationBuilder.DropColumn(
                name: "StartWindow",
                table: "SyncStates");
        }
    }
}
