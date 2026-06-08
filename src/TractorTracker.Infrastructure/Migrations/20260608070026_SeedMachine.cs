using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TractorTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Machines",
                columns: ["Id", "Name", "TrackerDeviceId", "LastSyncAt"],
                values: [new Guid("5541d758-e49f-4e4d-b34b-5ae1a841197b"), "Mon tracteur", "861327082960092", null]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Machines",
                keyColumn: "Id",
                keyValue: new Guid("5541d758-e49f-4e4d-b34b-5ae1a841197b"));
        }
    }
}
