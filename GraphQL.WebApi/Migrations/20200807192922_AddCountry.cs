using Microsoft.EntityFrameworkCore.Migrations;

namespace GraphQL.WebApi.Migrations
{
    public partial class AddCountry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "country_id",
                schema: "business",
                table: "city",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_city_country_id",
                schema: "business",
                table: "city",
                column: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_city_country_country_id",
                schema: "business",
                table: "city",
                column: "country_id",
                principalSchema: "business",
                principalTable: "country",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_city_country_country_id",
                schema: "business",
                table: "city");

            migrationBuilder.DropIndex(
                name: "IX_city_country_id",
                schema: "business",
                table: "city");

            migrationBuilder.DropColumn(
                name: "country_id",
                schema: "business",
                table: "city");
        }
    }
}
