using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CalendarSync.Migrations
{
    public partial class AddSyncState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncStates",
                columns: table => new
                {
                    SyncStateId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Principal = table.Column<string>(type: "TEXT", nullable: false),
                    DeltaUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncStates", x => x.SyncStateId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncStates");
        }
    }
}
