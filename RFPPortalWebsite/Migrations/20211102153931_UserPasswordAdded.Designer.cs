﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RFPPortalWebsite.Contexts;

namespace RFPPortalWebsite.Migrations
{
    [DbContext(typeof(rfpdb_context))]
    [Migration("20211102153931_UserPasswordAdded")]
    partial class UserPasswordAdded
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("RFPPortalWebsite.Models.DbModels.ApplicationLog", b =>
                {
                    b.Property<int>("ApplicationLogID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Application")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime");

                    b.Property<string>("Explanation")
                        .HasColumnType("text");

                    b.Property<int>("IdField")
                        .HasColumnType("int");

                    b.Property<string>("IdFieldName")
                        .HasColumnType("text");

                    b.Property<string>("Server")
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.HasKey("ApplicationLogID");

                    b.ToTable("ApplicationLogs");
                });

            modelBuilder.Entity("RFPPortalWebsite.Models.DbModels.ErrorLog", b =>
                {
                    b.Property<int>("ErrorLogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Application")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime");

                    b.Property<int>("IdField")
                        .HasColumnType("int");

                    b.Property<string>("IdFieldName")
                        .HasColumnType("text");

                    b.Property<string>("Message")
                        .HasColumnType("text");

                    b.Property<string>("Server")
                        .HasColumnType("text");

                    b.Property<string>("Target")
                        .HasColumnType("text");

                    b.Property<string>("Trace")
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.HasKey("ErrorLogId");

                    b.ToTable("ErrorLogs");
                });

            modelBuilder.Entity("RFPPortalWebsite.Models.DbModels.Rfp", b =>
                {
                    b.Property<int>("RfpID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<double>("Amount")
                        .HasColumnType("double");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Currency")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .HasColumnType("text");

                    b.Property<string>("Timeframe")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int?>("WinnerRfpBidID")
                        .HasColumnType("int");

                    b.HasKey("RfpID");

                    b.ToTable("Rfps");
                });

            modelBuilder.Entity("RFPPortalWebsite.Models.DbModels.RfpBid", b =>
                {
                    b.Property<int>("RfpBidID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<double>("Amount")
                        .HasColumnType("double");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<int>("RfpID")
                        .HasColumnType("int");

                    b.Property<string>("Time")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("RfpBidID");

                    b.ToTable("RfpBids");
                });

            modelBuilder.Entity("RFPPortalWebsite.Models.DbModels.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<string>("NameSurname")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.Property<string>("UserType")
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("RFPPortalWebsite.Models.DbModels.UserLog", b =>
                {
                    b.Property<int>("UserLogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Application")
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime");

                    b.Property<string>("Explanation")
                        .HasColumnType("text");

                    b.Property<string>("IdField")
                        .HasColumnType("text");

                    b.Property<int>("IdValue")
                        .HasColumnType("int");

                    b.Property<string>("Ip")
                        .HasColumnType("text");

                    b.Property<string>("Port")
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("UserLogId");

                    b.ToTable("UserLogs");
                });
#pragma warning restore 612, 618
        }
    }
}
