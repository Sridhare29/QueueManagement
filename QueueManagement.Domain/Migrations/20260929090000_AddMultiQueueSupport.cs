using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QueueManagement.API.Data;

#nullable disable

namespace QueueManagement.Domain.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260929090000_AddMultiQueueSupport")]
public partial class AddMultiQueueSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Queues",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                AccessCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                AdminUserId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Queues", x => x.Id);
                table.ForeignKey(
                    name: "FK_Queues_Users_AdminUserId",
                    column: x => x.AdminUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.Sql("""
            INSERT INTO [Queues] ([Name], [AccessCode], [AdminUserId])
            SELECT N'Existing Queue', N'000001',
                (SELECT TOP (1) [Id] FROM [Users] WHERE [Role] = N'Admin' ORDER BY [Id]);
            """);

        migrationBuilder.CreateTable(
            name: "QueueMemberships",
            columns: table => new
            {
                QueueId = table.Column<int>(type: "int", nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
                JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QueueMemberships", x => new { x.QueueId, x.UserId });
                table.ForeignKey(
                    name: "FK_QueueMemberships_Queues_QueueId",
                    column: x => x.QueueId,
                    principalTable: "Queues",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_QueueMemberships_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("""
            INSERT INTO [QueueMemberships] ([QueueId], [UserId], [JoinedAt])
            SELECT 1, [UserId], SYSUTCDATETIME()
            FROM [QueueTokens]
            GROUP BY [UserId];

            INSERT INTO [QueueMemberships] ([QueueId], [UserId], [JoinedAt])
            SELECT 1, [AdminUserId], SYSUTCDATETIME()
            FROM [Queues]
            WHERE [AdminUserId] IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1 FROM [QueueMemberships] AS membership
                  WHERE membership.[QueueId] = 1 AND membership.[UserId] = [Queues].[AdminUserId]);
            """);

        migrationBuilder.AddColumn<int>(
            name: "QueueId",
            table: "QueueTokens",
            type: "int",
            nullable: true);

        migrationBuilder.Sql("UPDATE [QueueTokens] SET [QueueId] = 1;");

        migrationBuilder.AlterColumn<int>(
            name: "QueueId",
            table: "QueueTokens",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.DropIndex(
            name: "IX_QueueTokens_TokenNo",
            table: "QueueTokens");

        migrationBuilder.CreateIndex(
            name: "IX_Queues_AccessCode",
            table: "Queues",
            column: "AccessCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Queues_AdminUserId",
            table: "Queues",
            column: "AdminUserId");

        migrationBuilder.CreateIndex(
            name: "IX_QueueMemberships_UserId",
            table: "QueueMemberships",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_QueueTokens_QueueId_TokenNo",
            table: "QueueTokens",
            columns: new[] { "QueueId", "TokenNo" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_QueueTokens_Queues_QueueId",
            table: "QueueTokens",
            column: "QueueId",
            principalTable: "Queues",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_QueueTokens_Queues_QueueId", "QueueTokens");
        migrationBuilder.DropTable("QueueMemberships");
        migrationBuilder.DropTable("Queues");
        migrationBuilder.DropIndex("IX_QueueTokens_QueueId_TokenNo", "QueueTokens");
        migrationBuilder.DropColumn("QueueId", "QueueTokens");
        migrationBuilder.CreateIndex(
            name: "IX_QueueTokens_TokenNo",
            table: "QueueTokens",
            column: "TokenNo",
            unique: true);
    }
}