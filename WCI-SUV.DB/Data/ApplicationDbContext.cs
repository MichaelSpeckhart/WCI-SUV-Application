using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Entities;
using WCI_SUV.Core.Interface;
using WCI_SUV.Core.Interface.Database;

namespace WCI_SUV.DB.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, bool skipAudit) : base(options) {
            _skipAudit = skipAudit;
        }

        public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUser? currentUser = null,
        IConfiguration? configuration = null)
        : base(options)
        {
            _currentUser = currentUser;
            _configuration = configuration;
        }

        #region Private Members

        private bool _skipAudit;
        private readonly IConfiguration? _configuration;
        private readonly ICurrentUser?   _currentUser;

        #endregion

        #region DbSets

        public DbSet<Conveyor>  Conveyors   { get; set; }
        public DbSet<Node>      Nodes       { get; set; }
        public DbSet<Ticket>    Tickets     { get; set; }
        // Add more as needed

        #endregion

        /// <summary>
        /// Apply Entity configurations from the Assembly code
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically apply Entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // modelBuilder.ApplyConfiguration<Conveyor>(new ConveyorConfiguration());
            modelBuilder.Entity<Ticket>(entity =>
            {
                // Explicitly configure the primary key
                entity.HasKey(e => e.TicketNumber);

                // Make sure it's not auto-generated
                entity.Property(e => e.TicketNumber).ValueGeneratedNever();

                // Configure the other properties with explicit column names
                entity.Property(e => e.TicketSize).HasColumnName("TicketSize");
                entity.Property(e => e.SlotNumber).HasColumnName("SlotNumber");
            });

            modelBuilder.Entity<Conveyor>(entity =>
            {
                entity.HasKey(e => e.AccountNumber);

                entity.Property(e => e.AccountNumber).ValueGeneratedNever();

                entity.Property(e => e.TicketNumber).HasColumnName("Ticket Number");
                entity.Property(e => e.SlotNumber).HasColumnName("Slot Number");
                entity.Property(e => e.GarmentNumber).HasColumnName("Garment Number");
                entity.Property(e => e.EmployeeNumber).HasColumnName("Employee Number");
                entity.Property(e => e.TicketSize).HasColumnName("Ticket Size");
            });

            modelBuilder.Entity<Node>(entity =>
            {
                entity.HasKey(e => e.Name);

                entity.Property(e => e.Name).ValueGeneratedNever();

                entity.Property(e => e.Namespace).HasColumnName("Namespace");
                entity.Property(e => e.Register).HasColumnName("Register");
                entity.Property(e => e.Type).HasColumnName("Type");
            });
        }

        // Need connection string
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("Data Source=MICHAEL-XPS-13\\SQLEXPRESS;Initial Catalog=compusort_suv;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.UtcNow;
                        entry.Entity.CreatedBy = _currentUser?.Id ?? "System"; // Ensure it's never null
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = DateTime.UtcNow;
                        entry.Entity.ModifiedBy = _currentUser?.Id ?? "System";
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }



    }
}
