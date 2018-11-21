using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recruiter.Models;

namespace Recruiter.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<JobPosition> JobPositions { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<ApplicationStagesRequirement> ApplicationStagesRequirements { get; set; }
        public DbSet<ApplicationsViewHistory> ApplicationsViewHistories { get; set; }
        public DbSet<ApplicationStageBase> ApplicationStages { get; set; }
        public DbSet<ApplicationApproval> ApplicationApprovals { get; set; }
        public DbSet<PhoneCall> PhoneCalls { get; set; }
        public DbSet<Homework> Homeworks { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<InterviewAppointment> InterviewAppointments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            //builder.Entity<JobPosition>()
            //   .HasOne(x => x.Creator)
            //   .WithMany(x => x.JobPositions)
            //   .HasForeignKey(x => x.CreatorId);

            #region ApplicationUser
            builder.Entity<ApplicationUser>()
                .HasMany(x => x.JobPositions)
                .WithOne(x => x.Creator)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
                .HasMany(x => x.Applications)
                .WithOne(x => x.User)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region ApplicationUser
            builder.Entity<JobPosition>()
                .HasMany(x => x.Applications)
                .WithOne(x => x.JobPosition)
                .OnDelete(DeleteBehavior.SetNull);
            #endregion

            #region ApplicationStagesRequirement
            builder.Entity<ApplicationStagesRequirement>()
                .HasOne(x => x.JobPosition)
                .WithOne(x => x.ApplicationStagesRequirement)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region ApplicationsViewHistory
            builder.Entity<ApplicationsViewHistory>()
                .HasOne(x => x.Application)
                .WithMany(x => x.ApplicationsViewHistories)
                //.IsRequired()   //
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationsViewHistory>()
                .HasOne(x => x.User)
                .WithMany(x => x.ApplicationsViewHistories)
                //.IsRequired() //
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            #region ApplicationStageBase
            builder.Entity<ApplicationStageBase>()
                .HasOne(x => x.Application)
                .WithMany(x => x.ApplicationStages)
                //.IsRequired() //
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region InterviewAppointment
            builder.Entity<InterviewAppointment>()
                .HasOne(x => x.Interview)
                .WithMany(x => x.InterviewAppointments)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
