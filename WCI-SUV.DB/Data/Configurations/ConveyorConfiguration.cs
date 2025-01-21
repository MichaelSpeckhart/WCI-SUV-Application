using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;

namespace WCI_SUV.DB.Data.Configurations
{
    public class ConveyorConfiguration : IEntityTypeConfiguration<Conveyor>
    {
        public void Configure(EntityTypeBuilder<Conveyor> builder)
        {
            builder.HasKey(p => p.AccountNumber);

            builder.Property(p => p.AccountNumber).HasColumnName("Account Number")
                .IsRequired()
                .HasColumnType("integer");

            builder.Property(p => p.EmployeeNumber).HasColumnName("Employee Number")
                .IsRequired()
                .HasColumnType("integer");

            builder.Property(p => p.GarmentNumber).HasColumnName("Garment Number")
                .IsRequired()
                .HasColumnType("integer");

            builder.Property(p => p.SlotNumber).HasColumnName("Slot Number")
                .IsRequired()
                .HasColumnType("integer");

            builder.Property(p => p.TicketNumber).HasColumnName("Ticket Number")
                .IsRequired()
                .HasColumnType("integer");
        }
    }
}
