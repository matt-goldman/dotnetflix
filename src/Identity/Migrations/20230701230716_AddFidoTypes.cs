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
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FidoUsers", x => x.UserId);
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
                    CredentialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FidoUserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PublicKey = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    SignCount = table.Column<long>(type: "bigint", nullable: false),
                    Transports = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BE = table.Column<bool>(type: "bit", nullable: false),
                    BS = table.Column<bool>(type: "bit", nullable: false),
                    AttestationObject = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    AttestationClientDataJSON = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DevicePublicKeys = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserId = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    UserHandle = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CredType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AaGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FidoStoredCredentials", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_FidoStoredCredentials_FidoUsers_FidoUserId",
                        column: x => x.FidoUserId,
                        principalTable: "FidoUsers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FidoPublicKeyDescriptors",
                columns: table => new
                {
                    DescriptorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CredentialId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Id = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Transports = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FidoPublicKeyDescriptors", x => x.DescriptorId);
                    table.ForeignKey(
                        name: "FK_FidoPublicKeyDescriptors_FidoStoredCredentials_CredentialId",
                        column: x => x.CredentialId,
                        principalTable: "FidoStoredCredentials",
                        principalColumn: "CredentialId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FidoPublicKeyDescriptors_CredentialId",
                table: "FidoPublicKeyDescriptors",
                column: "CredentialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FidoStoredCredentials_FidoUserId",
                table: "FidoStoredCredentials",
                column: "FidoUserId");

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
