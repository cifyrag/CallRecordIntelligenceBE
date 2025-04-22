using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallRecordIntelligence.EF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CallRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    caller_id = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
                    recipient = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
                    call_start = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "TIMESTAMP WITH TIME ZONE", nullable: false),
                    cost = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    reference = table.Column<string>(type: "VARCHAR", nullable: false),
                    currency = table.Column<string>(type: "VARCHAR", maxLength: 3, nullable: false),
                    Inserted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallRecords");
        }
    }
}
