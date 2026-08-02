using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestrictColumnDeletionWithTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_board_tasks_board_columns_column_id",
                table: "board_tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_board_tasks_board_columns_column_id",
                table: "board_tasks",
                column: "column_id",
                principalTable: "board_columns",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_board_tasks_board_columns_column_id",
                table: "board_tasks");

            migrationBuilder.AddForeignKey(
                name: "FK_board_tasks_board_columns_column_id",
                table: "board_tasks",
                column: "column_id",
                principalTable: "board_columns",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
