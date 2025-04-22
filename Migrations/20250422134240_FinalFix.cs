using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxDeclarationWeb.Migrations
{
    /// <inheritdoc />
    public partial class FinalFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers",
                column: "InspectorId",
                principalTable: "Инспекторы",
                principalColumn: "инспектор",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Инспекторы_InspectorId",
                table: "AspNetUsers",
                column: "InspectorId",
                principalTable: "Инспекторы",
                principalColumn: "инспектор");
        }
    }
}
