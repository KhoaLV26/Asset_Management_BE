﻿// <auto-generated />
using System;
using AssetManagement.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AssetManagement.Infrastructure.Migrations
{
    [DbContext(typeof(DBContext))]
    partial class DBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AssetManagement.Domain.Entities.Asset", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AssetCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("AssetName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<Guid?>("AssignmentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateOnly>("InstallDate")
                        .HasColumnType("date");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("LastUpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("LocationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Specification")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AssetCode")
                        .IsUnique();

                    b.HasIndex("CategoryId");

                    b.HasIndex("LocationId");

                    b.ToTable("Assets", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Assignment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AssetId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AssignedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("AssignedDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("AssignedTo")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("LastUpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Note")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ReturnRequestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AssetId");

                    b.HasIndex("AssignedBy");

                    b.HasIndex("AssignedTo");

                    b.ToTable("Assignments", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.BlackListToken", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("BlackListTokens", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("LastUpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Categories", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Location", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("nvarchar(2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("LastUpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Locations", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.RefreshToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("ExpiredAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("TokenHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TokenSalt")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("RefreshTokens", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.ReturnRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("AcceptanceBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("AssignmentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("LastUpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateOnly?>("ReturnDate")
                        .HasColumnType("date");

                    b.Property<int>("ReturnStatus")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AcceptanceBy");

                    b.HasIndex("AssignmentId")
                        .IsUnique();

                    b.ToTable("ReturnRequests", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("LastUpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Roles", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Token", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("HashToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Tokens", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CreatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateOnly>("DateJoined")
                        .HasColumnType("date");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<string>("HashPassword")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("IsFirstLogin")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("LastUpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("LastUpdatedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SaltPassword")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("StaffCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.HasIndex("RoleId");

                    b.HasIndex("StaffCode")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Asset", b =>
                {
                    b.HasOne("AssetManagement.Domain.Entities.Category", "Category")
                        .WithMany("Assets")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AssetManagement.Domain.Entities.Location", "Location")
                        .WithMany("Assets")
                        .HasForeignKey("LocationId");

                    b.Navigation("Category");

                    b.Navigation("Location");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Assignment", b =>
                {
                    b.HasOne("AssetManagement.Domain.Entities.Asset", "Asset")
                        .WithMany("Assignments")
                        .HasForeignKey("AssetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AssetManagement.Domain.Entities.User", "UserBy")
                        .WithMany("AssignmentsBy")
                        .HasForeignKey("AssignedBy")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AssetManagement.Domain.Entities.User", "UserTo")
                        .WithMany("AssignmentsTo")
                        .HasForeignKey("AssignedTo")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Asset");

                    b.Navigation("UserBy");

                    b.Navigation("UserTo");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.RefreshToken", b =>
                {
                    b.HasOne("AssetManagement.Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.ReturnRequest", b =>
                {
                    b.HasOne("AssetManagement.Domain.Entities.User", "UserAccept")
                        .WithMany("ReturnRequestsAccepted")
                        .HasForeignKey("AcceptanceBy")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("AssetManagement.Domain.Entities.Assignment", "Assignment")
                        .WithOne("ReturnRequest")
                        .HasForeignKey("AssetManagement.Domain.Entities.ReturnRequest", "AssignmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Assignment");

                    b.Navigation("UserAccept");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Token", b =>
                {
                    b.HasOne("AssetManagement.Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.User", b =>
                {
                    b.HasOne("AssetManagement.Domain.Entities.Location", "Location")
                        .WithMany("Users")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AssetManagement.Domain.Entities.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Location");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Asset", b =>
                {
                    b.Navigation("Assignments");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Assignment", b =>
                {
                    b.Navigation("ReturnRequest");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Category", b =>
                {
                    b.Navigation("Assets");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Location", b =>
                {
                    b.Navigation("Assets");

                    b.Navigation("Users");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("AssetManagement.Domain.Entities.User", b =>
                {
                    b.Navigation("AssignmentsBy");

                    b.Navigation("AssignmentsTo");

                    b.Navigation("ReturnRequestsAccepted");
                });
#pragma warning restore 612, 618
        }
    }
}
