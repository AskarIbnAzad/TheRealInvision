using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheRealInvision.Migrations
{
    /// <inheritdoc />
    public partial class AddingPositionField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "ProjectImages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "ProjectImages");
        }
    }
}
