using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SwH.Models.EF
{
    public partial class SwHDbContext : DbContext
    {
        public SwHDbContext()
        {
        }

        public SwHDbContext(DbContextOptions<SwHDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<ProjectTeam> ProjectTeam { get; set; }
        public virtual DbSet<Tasks> Tasks { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("server=mysql.labranet.jamk.fi;database=L5192_2;uid=L5192;password=0idSIogAW862QXoNrDQy9lg1vxCQWAAy;persistsecurityinfo=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasIndex(e => e.Manager)
                    .HasName("FK_Project_User");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Expires).HasColumnType("date");

                entity.Property(e => e.Manager).HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.HasOne(d => d.ManagerNavigation)
                    .WithMany(p => p.Project)
                    .HasForeignKey(d => d.Manager)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_User");
            });

            modelBuilder.Entity<ProjectTeam>(entity =>
            {
                entity.HasIndex(e => e.ProjectId)
                    .HasName("FK_ProjectTeam_Project");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_ProjectTeam_User");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.ProjectId).HasColumnType("int(11)");

                entity.Property(e => e.UserId).HasColumnType("int(11)");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectTeam)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProjectTeam_Project");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ProjectTeam)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProjectTeam_User");
            });

            modelBuilder.Entity<Tasks>(entity =>
            {
                entity.HasIndex(e => e.ProjectId)
                    .HasName("FK_Tasks_Project");

                entity.HasIndex(e => e.UserId)
                    .HasName("FK_Tasks_User");

                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.Descr).HasColumnType("varchar(255)");

                entity.Property(e => e.Expires).HasColumnType("date");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("varchar(25)");

                entity.Property(e => e.ProjectId).HasColumnType("int(11)");

                entity.Property(e => e.Status).HasColumnType("varchar(20)");

                entity.Property(e => e.UserId).HasColumnType("int(11)");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tasks_Project");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tasks_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("int(11)");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnType("varchar(25)");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnType("varchar(20)");
            });
        }
    }
}
