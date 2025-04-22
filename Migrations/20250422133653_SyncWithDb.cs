using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxDeclarationWeb.Migrations
{
    /// <inheritdoc />
    public partial class SyncWithDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IIN",
                table: "AspNetUsers",
                newName: "Role");

            migrationBuilder.AddColumn<string>(
                name: "InspectorId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InspectorId",
                table: "AspNetUsers",
                column: "InspectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers",
                column: "InspectorId",
                principalTable: "Инспекторы",
                principalColumn: "инспектор");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_InspectorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InspectorId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "AspNetUsers",
                newName: "IIN");
        }
    }
}
