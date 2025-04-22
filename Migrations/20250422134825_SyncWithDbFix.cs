using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxDeclarationWeb.Migrations
{
    /// <inheritdoc />
    public partial class SyncWithDbFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_InspectorId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "InspectorId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "InspectorId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_InspectorId",
                table: "AspNetUsers",
                column: "InspectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers",
                column: "InspectorId",
                principalTable: "Инспекторы",
                principalColumn: "инспектор",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
