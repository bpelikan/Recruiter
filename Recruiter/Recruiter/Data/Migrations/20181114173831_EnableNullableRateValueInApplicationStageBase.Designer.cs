﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Recruiter.Data;

namespace Recruiter.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20181114173831_EnableNullableRateValueInApplicationStageBase")]
    partial class EnableNullableRateValueInApplicationStageBase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.3-rtm-32065")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Recruiter.Models.Application", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("CvFileName");

                    b.Property<string>("JobPositionId");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("JobPositionId");

                    b.HasIndex("UserId");

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationStageBase", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Accepted");

                    b.Property<string>("AcceptedById");

                    b.Property<string>("ApplicationId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<int>("Level");

                    b.Property<string>("Note");

                    b.Property<int?>("Rate");

                    b.Property<string>("ResponsibleUserId");

                    b.Property<int>("State");

                    b.HasKey("Id");

                    b.HasIndex("AcceptedById");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("ResponsibleUserId");

                    b.ToTable("ApplicationStages");

                    b.HasDiscriminator<string>("Discriminator").HasValue("ApplicationStageBase");
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationStagesRequirement", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DefaultResponsibleForApplicatioApprovalId");

                    b.Property<string>("DefaultResponsibleForHomeworkId");

                    b.Property<string>("DefaultResponsibleForInterviewId");

                    b.Property<string>("DefaultResponsibleForPhoneCallId");

                    b.Property<bool>("IsApplicationApprovalRequired");

                    b.Property<bool>("IsHomeworkRequired");

                    b.Property<bool>("IsInterviewRequired");

                    b.Property<bool>("IsPhoneCallRequired");

                    b.Property<string>("JobPositionId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("DefaultResponsibleForApplicatioApprovalId");

                    b.HasIndex("DefaultResponsibleForHomeworkId");

                    b.HasIndex("DefaultResponsibleForInterviewId");

                    b.HasIndex("DefaultResponsibleForPhoneCallId");

                    b.HasIndex("JobPositionId")
                        .IsUnique();

                    b.ToTable("ApplicationStagesRequirements");
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationsViewHistory", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationId");

                    b.Property<string>("UserId");

                    b.Property<DateTime>("ViewTime");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("UserId");

                    b.ToTable("ApplicationsViewHistories");
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Recruiter.Models.JobPosition", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CreatorId");

                    b.Property<string>("Description");

                    b.Property<DateTime?>("EndDate");

                    b.Property<string>("Name");

                    b.Property<DateTime>("StartDate");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("JobPositions");
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationApproval", b =>
                {
                    b.HasBaseType("Recruiter.Models.ApplicationStageBase");


                    b.ToTable("ApplicationApproval");

                    b.HasDiscriminator().HasValue("ApplicationApproval");
                });

            modelBuilder.Entity("Recruiter.Models.Homework", b =>
                {
                    b.HasBaseType("Recruiter.Models.ApplicationStageBase");

                    b.Property<string>("Description");

                    b.Property<int>("Duration");

                    b.Property<DateTime?>("EndTime");

                    b.Property<int>("HomeworkState");

                    b.Property<DateTime?>("SendingTime");

                    b.Property<DateTime?>("StartTime");

                    b.Property<string>("Url");

                    b.ToTable("Homework");

                    b.HasDiscriminator().HasValue("Homework");
                });

            modelBuilder.Entity("Recruiter.Models.Interview", b =>
                {
                    b.HasBaseType("Recruiter.Models.ApplicationStageBase");


                    b.ToTable("Interview");

                    b.HasDiscriminator().HasValue("Interview");
                });

            modelBuilder.Entity("Recruiter.Models.PhoneCall", b =>
                {
                    b.HasBaseType("Recruiter.Models.ApplicationStageBase");


                    b.ToTable("PhoneCall");

                    b.HasDiscriminator().HasValue("PhoneCall");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Recruiter.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Recruiter.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recruiter.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Recruiter.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recruiter.Models.Application", b =>
                {
                    b.HasOne("Recruiter.Models.JobPosition", "JobPosition")
                        .WithMany("Applications")
                        .HasForeignKey("JobPositionId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Recruiter.Models.ApplicationUser", "User")
                        .WithMany("Applications")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationStageBase", b =>
                {
                    b.HasOne("Recruiter.Models.ApplicationUser", "AcceptedBy")
                        .WithMany()
                        .HasForeignKey("AcceptedById");

                    b.HasOne("Recruiter.Models.Application", "Application")
                        .WithMany("ApplicationStages")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recruiter.Models.ApplicationUser", "ResponsibleUser")
                        .WithMany()
                        .HasForeignKey("ResponsibleUserId");
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationStagesRequirement", b =>
                {
                    b.HasOne("Recruiter.Models.ApplicationUser", "DefaultResponsibleForApplicatioApproval")
                        .WithMany()
                        .HasForeignKey("DefaultResponsibleForApplicatioApprovalId");

                    b.HasOne("Recruiter.Models.ApplicationUser", "DefaultResponsibleForHomework")
                        .WithMany()
                        .HasForeignKey("DefaultResponsibleForHomeworkId");

                    b.HasOne("Recruiter.Models.ApplicationUser", "DefaultResponsibleForInterview")
                        .WithMany()
                        .HasForeignKey("DefaultResponsibleForInterviewId");

                    b.HasOne("Recruiter.Models.ApplicationUser", "DefaultResponsibleForPhoneCall")
                        .WithMany()
                        .HasForeignKey("DefaultResponsibleForPhoneCallId");

                    b.HasOne("Recruiter.Models.JobPosition", "JobPosition")
                        .WithOne("ApplicationStagesRequirement")
                        .HasForeignKey("Recruiter.Models.ApplicationStagesRequirement", "JobPositionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recruiter.Models.ApplicationsViewHistory", b =>
                {
                    b.HasOne("Recruiter.Models.Application", "Application")
                        .WithMany("ApplicationsViewHistories")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recruiter.Models.ApplicationUser", "User")
                        .WithMany("ApplicationsViewHistories")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Recruiter.Models.JobPosition", b =>
                {
                    b.HasOne("Recruiter.Models.ApplicationUser", "Creator")
                        .WithMany("JobPositions")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.SetNull);
                });
#pragma warning restore 612, 618
        }
    }
}
