using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace System_EPS.Migrations
{
    /// <inheritdoc />
    public partial class MakeAffiliateIdOptionalInTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Affiliates_AffiliateId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "AffiliateId",
                table: "Tickets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Affiliates_AffiliateId",
                table: "Tickets",
                column: "AffiliateId",
                principalTable: "Affiliates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Affiliates_AffiliateId",
                table: "Tickets");

            migrationBuilder.AlterColumn<int>(
                name: "AffiliateId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Affiliates_AffiliateId",
                table: "Tickets",
                column: "AffiliateId",
                principalTable: "Affiliates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
