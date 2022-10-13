﻿// <auto-generated />
using System;
using BscTokenSniper.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BscTokenSniper.Migrations
{
    [DbContext(typeof(PersistenceContext))]
    [Migration("20221011051609_AddSomeIndexes1")]
    partial class AddSomeIndexes1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BscTokenSniper.Handlers.LiquidityEvent", b =>
                {
                    b.Property<int>("LiquidityEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LiquidityEventId"));

                    b.Property<string>("Amount")
                        .HasColumnType("text");

                    b.Property<string>("PairAddress")
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Token0")
                        .HasColumnType("text");

                    b.Property<string>("Token1")
                        .HasColumnType("text");

                    b.Property<int>("TokenPairId")
                        .HasColumnType("integer");

                    b.HasKey("LiquidityEventId");

                    b.HasIndex("TokenPairId");

                    b.ToTable("LiquidityEvents");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.Token", b =>
                {
                    b.Property<int>("TokenId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TokenId"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<bool>("Blocked")
                        .HasColumnType("boolean");

                    b.Property<string>("Symbol")
                        .HasColumnType("text");

                    b.HasKey("TokenId");

                    b.HasIndex("Address");

                    b.ToTable("Tokens");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPair", b =>
                {
                    b.Property<int>("TokenPairId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TokenPairId"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<bool>("IsRejected")
                        .HasColumnType("boolean");

                    b.Property<string>("Symbol")
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Token0")
                        .HasColumnType("text");

                    b.Property<string>("Token1")
                        .HasColumnType("text");

                    b.HasKey("TokenPairId");

                    b.HasIndex("Address");

                    b.ToTable("TokenPairs");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPairValue", b =>
                {
                    b.Property<int>("TokenPairValueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TokenPairValueId"));

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TokenPairId")
                        .HasColumnType("integer");

                    b.Property<int>("Value")
                        .HasColumnType("integer");

                    b.HasKey("TokenPairValueId");

                    b.HasIndex("TokenPairId");

                    b.ToTable("TokenPairValues");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.LiquidityEvent", b =>
                {
                    b.HasOne("BscTokenSniper.Handlers.TokenPair", "TokenPair")
                        .WithMany("LiquidityEvents")
                        .HasForeignKey("TokenPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TokenPair");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPairValue", b =>
                {
                    b.HasOne("BscTokenSniper.Handlers.TokenPair", "TokenPair")
                        .WithMany("TokenPairValue")
                        .HasForeignKey("TokenPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TokenPair");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPair", b =>
                {
                    b.Navigation("LiquidityEvents");

                    b.Navigation("TokenPairValue");
                });
#pragma warning restore 612, 618
        }
    }
}