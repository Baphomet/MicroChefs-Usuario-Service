using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ClienteService.Context;

#nullable disable

namespace ClienteService.Migrations
{
    [DbContext(typeof(Db))]
    [Migration("20260610000000_AddSettingsAndCoordinates")]
    /// <inheritdoc />
    public partial class AddSettingsAndCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add Latitude and Longitude to Enderecos
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Enderecos",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Enderecos",
                type: "double",
                nullable: true);

            // Add Latitude and Longitude to Usuarios
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Usuarios",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Usuarios",
                type: "double",
                nullable: true);

            // Create Settings table
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NomeRestaurante = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    Telefone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HorarioFuncionamento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Logo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Settings");
            migrationBuilder.DropColumn(name: "Latitude", table: "Enderecos");
            migrationBuilder.DropColumn(name: "Longitude", table: "Enderecos");
            migrationBuilder.DropColumn(name: "Latitude", table: "Usuarios");
            migrationBuilder.DropColumn(name: "Longitude", table: "Usuarios");
        }
    }
}
