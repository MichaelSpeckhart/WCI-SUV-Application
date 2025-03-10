﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WCI_SUV.DB.Data;

#nullable disable

namespace WCI_SUV.DB.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WCI_SUV.Core.Entities.Conveyor", b =>
                {
                    b.Property<int>("AccountNumber")
                        .HasColumnType("integer")
                        .HasColumnName("Account Number");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("EmployeeNumber")
                        .HasColumnType("integer")
                        .HasColumnName("Employee Number");

                    b.Property<int>("GarmentNumber")
                        .HasColumnType("integer")
                        .HasColumnName("Garment Number");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("SlotNumber")
                        .HasColumnType("integer")
                        .HasColumnName("Slot Number");

                    b.Property<int>("TicketNumber")
                        .HasColumnType("integer")
                        .HasColumnName("Ticket Number");

                    b.Property<long>("TicketSize")
                        .HasColumnType("bigint")
                        .HasColumnName("Ticket Size");

                    b.HasKey("AccountNumber");

                    b.ToTable("Conveyor");
                });

            modelBuilder.Entity("WCI_SUV.Core.Entities.Node", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)")
                        .HasColumnName("Name");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<short>("Namespace")
                        .HasColumnType("smallint")
                        .HasColumnName("Namespace");

                    b.Property<long>("Register")
                        .HasColumnType("bigint")
                        .HasColumnName("Register");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Type");

                    b.HasKey("Name");

                    b.ToTable("Nodes");
                });

            modelBuilder.Entity("WCI_SUV.Core.Entities.Ticket", b =>
                {
                    b.Property<int>("TicketNumber")
                        .HasColumnType("int")
                        .HasColumnName("Ticket Number");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("SlotNumber")
                        .HasColumnType("int")
                        .HasColumnName("SlotNumber");

                    b.Property<long>("TicketSize")
                        .HasColumnType("bigint")
                        .HasColumnName("TicketSize");

                    b.HasKey("TicketNumber");

                    b.ToTable("Ticket");
                });
#pragma warning restore 612, 618
        }
    }
}
