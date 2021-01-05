using Microsoft.EntityFrameworkCore;
using Pds.Contracts.Data.Repository.DataModels;

#nullable disable

namespace Pds.Contracts.Data.Repository.Context
{
    /// <summary>
    /// PDS database context - Owner is monolith, this is created by reverse engineering existing database.
    /// Migrations should not be performed.
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    public partial class PdsContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdsContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public PdsContext(DbContextOptions<PdsContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the contracts.
        /// </summary>
        /// <value>
        /// The contracts.
        /// </value>
        public virtual DbSet<Contract> Contracts { get; set; }

        /// <summary>
        /// Gets or sets the contract contents.
        /// </summary>
        /// <value>
        /// The contract contents.
        /// </value>
        public virtual DbSet<ContractContent> ContractContents { get; set; }

        /// <summary>
        /// Gets or sets the contract datas.
        /// </summary>
        /// <value>
        /// The contract datas.
        /// </value>
        public virtual DbSet<ContractData> ContractDatas { get; set; }

        /// <summary>
        /// Gets or sets the contract funding stream period codes.
        /// </summary>
        /// <value>
        /// The contract funding stream period codes.
        /// </value>
        public virtual DbSet<ContractFundingStreamPeriodCode> ContractFundingStreamPeriodCodes { get; set; }

        /// <summary>
        /// Gets or sets the contract funding stream period code displays.
        /// </summary>
        /// <value>
        /// The contract funding stream period code displays.
        /// </value>
        public virtual DbSet<ContractFundingStreamPeriodCodeDisplay> ContractFundingStreamPeriodCodeDisplays { get; set; }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.ToTable("Contracts", "Contracts");

                entity.HasIndex(e => e.FirstCensusDateId, "IX_FirstCensusDate_Id");

                entity.HasIndex(e => e.SecondCensusDateId, "IX_SecondCensusDate_Id");

                entity.Property(e => e.ContractNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedBy).HasDefaultValueSql("('Feed')");

                entity.Property(e => e.EmailReminder).HasDefaultValueSql("((1))");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.FirstCensusDateId).HasColumnName("FirstCensusDate_Id");

                entity.Property(e => e.LastEmailReminderSent).HasColumnType("datetime");

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.NotificationReadAt).HasColumnType("datetime");

                entity.Property(e => e.ParentContractNumber).HasMaxLength(20);

                entity.Property(e => e.SecondCensusDateId).HasColumnName("SecondCensusDate_Id");

                entity.Property(e => e.SignedOn).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Title).IsRequired();

                entity.Property(e => e.Ukprn).HasColumnName("UKPRN");

                entity.Property(e => e.Value).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Year).IsRequired();
            });

            modelBuilder.Entity<ContractContent>(entity =>
            {
                entity.ToTable("ContractContents", "Contracts");

                entity.HasIndex(e => e.Id, "IX_Id");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Content).IsRequired();

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasDefaultValueSql("('')");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.ContractContent)
                    .HasForeignKey<ContractContent>(d => d.Id)
                    .HasConstraintName("FK_Contracts.ContractContents_Contracts.Contracts_Id");
            });

            modelBuilder.Entity<ContractData>(entity =>
            {
                entity.ToTable("ContractDatas", "Contracts");

                entity.HasIndex(e => e.Id, "IX_Id");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.ContractData)
                    .HasForeignKey<ContractData>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Contracts.ContractDatas_Contracts.Contracts_Id");
            });

            modelBuilder.Entity<ContractFundingStreamPeriodCode>(entity =>
            {
                entity.ToTable("ContractFundingStreamPeriodCodes", "Contracts");

                entity.HasIndex(e => e.ContractId, "IX_Contract_Id");

                entity.Property(e => e.ContractId).HasColumnName("Contract_Id");

                entity.HasOne(d => d.Contract)
                    .WithMany(p => p.ContractFundingStreamPeriodCodes)
                    .HasForeignKey(d => d.ContractId)
                    .HasConstraintName("FK_Contracts.ContractFundingStreamPeriodCodes_Contracts.Contracts_Contract_Id");
            });

            modelBuilder.Entity<ContractFundingStreamPeriodCodeDisplay>(entity =>
            {
                entity.ToTable("ContractFundingStreamPeriodCodeDisplays", "Contracts");

                entity.Property(e => e.Code).IsRequired();

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.FriendlyName).IsRequired();

                entity.Property(e => e.LastUpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Name).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}