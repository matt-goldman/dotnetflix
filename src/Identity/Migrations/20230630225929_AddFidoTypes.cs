using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotNetFlix.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddFidoTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FidoUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FidoUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FidoUsers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FidoStoredCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId1 = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PublicKey = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserHandle = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    SignatureCounter = table.Column<long>(type: "bigint", nullable: false),
                    CredType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AaGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FidoStoredCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FidoStoredCredentials_FidoUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "FidoUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FidoPublicKeyDescriptors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CredentialId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Transports = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FidoPublicKeyDescriptors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FidoPublicKeyDescriptors_FidoStoredCredentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "FidoStoredCredentials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FidoPublicKeyDescriptors_CredentialId",
                table: "FidoPublicKeyDescriptors",
                column: "CredentialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FidoStoredCredentials_UserId1",
                table: "FidoStoredCredentials",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_FidoUsers_ApplicationUserId",
                table: "FidoUsers",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FidoPublicKeyDescriptors");

            migrationBuilder.DropTable(
                name: "FidoStoredCredentials");

            migrationBuilder.DropTable(
                name: "FidoUsers");
        }
    }
}
