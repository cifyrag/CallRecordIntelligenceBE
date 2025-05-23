﻿// <auto-generated />
using System;
using CallRecordIntelligence.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CallRecordIntelligence.EF.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CallRecordIntelligence.EF.Models.CallRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("CallerId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR")
                        .HasColumnName("caller_id");

                    b.Property<decimal>("Cost")
                        .HasColumnType("DECIMAL(10, 3)")
                        .HasColumnName("cost");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("VARCHAR")
                        .HasColumnName("currency");

                    b.Property<DateTimeOffset>("EndTime")
                        .HasColumnType("TIMESTAMP WITH TIME ZONE")
                        .HasColumnName("end_time");

                    b.Property<DateTimeOffset>("Inserted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTimeOffset>("LastUpdated")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Recipient")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("VARCHAR")
                        .HasColumnName("recipient");

                    b.Property<string>("Reference")
                        .IsRequired()
                        .HasColumnType("VARCHAR")
                        .HasColumnName("reference");

                    b.Property<DateTimeOffset>("StartTime")
                        .HasColumnType("TIMESTAMP WITH TIME ZONE")
                        .HasColumnName("call_start");

                    b.HasKey("Id");

                    b.ToTable("CallRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
