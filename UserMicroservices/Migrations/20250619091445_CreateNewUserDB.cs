using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserMicroservices.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewUserDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    EmailId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserPassword = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.EmailId);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "EmailId", "Address", "DateOfBirth", "Gender", "RoleName", "UserPassword" },
                values: new object[,]
                {
                    { "Franken@gmail.com", "Texas, USA", new DateTime(1978, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "M", "Admin", "Franken@785" },
                    { "PaulGrey@gmail.com", "Denver, USA", new DateTime(1993, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "M", "User", "Paul@123" },
                    { "SamRocks@gmail.com", "Denver, USA", new DateTime(1986, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "M", "User", "Sam@564" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
