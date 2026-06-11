using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TractorTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnrichPositionRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AltitudeMeters",
                table: "PositionRecords",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormattedAddress",
                table: "PositionRecords",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Satellites",
                table: "PositionRecords",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AltitudeMeters",
                table: "PositionRecords");

            migrationBuilder.DropColumn(
                name: "FormattedAddress",
                table: "PositionRecords");

            migrationBuilder.DropColumn(
                name: "Satellites",
                table: "PositionRecords");
        }
    }
}
