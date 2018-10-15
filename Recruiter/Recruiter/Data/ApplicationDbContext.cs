﻿using System;
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

            builder.Entity<ApplicationUser>()
                .HasMany(x => x.JobPositions)
                .WithOne(x => x.Creator)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
                .HasMany(x => x.Applications)
                .WithOne(x => x.User)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<JobPosition>()
                .HasMany(x => x.Applications)
                .WithOne(x => x.JobPosition)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationStagesRequirement>()
                .HasOne(x => x.JobPosition)
                .WithOne(x => x.ApplicationStagesRequirement)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
