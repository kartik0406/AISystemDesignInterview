using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SdiApiGateway.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evaluation_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    overall_score = table.Column<double>(type: "double precision", nullable: false),
                    strengths = table.Column<string>(type: "text", nullable: true),
                    weaknesses = table.Column<string>(type: "text", nullable: true),
                    suggestions = table.Column<string>(type: "text", nullable: true),
                    rubric_breakdown = table.Column<string>(type: "text", nullable: true),
                    architecture_diagram = table.Column<string>(type: "text", nullable: true),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluation_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "interview_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "text", nullable: false),
                    company_mode = table.Column<string>(type: "text", nullable: false),
                    current_difficulty = table.Column<string>(type: "text", nullable: false),
                    current_round = table.Column<int>(type: "integer", nullable: false),
                    max_rounds = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "interview_rounds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_number = table.Column<int>(type: "integer", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    user_answer = table.Column<string>(type: "text", nullable: true),
                    evaluation = table.Column<string>(type: "text", nullable: true),
                    score = table.Column<double>(type: "double precision", nullable: true),
                    difficulty = table.Column<string>(type: "text", nullable: true),
                    topic_area = table.Column<string>(type: "text", nullable: true),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_rounds", x => x.id);
                    table.ForeignKey(
                        name: "FK_interview_rounds_interview_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "interview_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_interview_rounds_session_id",
                table: "interview_rounds",
                column: "session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evaluation_results");

            migrationBuilder.DropTable(
                name: "interview_rounds");

            migrationBuilder.DropTable(
                name: "interview_sessions");
        }
    }
}
