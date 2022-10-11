﻿// <auto-generated />
using System;
using BscTokenSniper.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BscTokenSniper.Migrations
{
    [DbContext(typeof(PersistenceContext))]
    partial class PersistenceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenEvent", b =>
                {
                    b.Property<int>("TokenEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TokenEventId"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<long>("BuyQuantity")
                        .HasColumnType("bigint");

                    b.Property<long>("BuyValue")
                        .HasColumnType("bigint");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("EventType")
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TokenPairId")
                        .HasColumnType("integer");

                    b.Property<string>("Wallet")
                        .HasColumnType("text");

                    b.Property<string>("WalletAddress")
                        .HasColumnType("text");

                    b.HasKey("TokenEventId");

                    b.HasIndex("Address");

                    b.HasIndex("TokenPairId");

                    b.ToTable("TokenEvents");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPair", b =>
                {
                    b.Property<int>("TokenPairId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TokenPairId"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<bool>("Owned")
                        .HasColumnType("boolean");

                    b.Property<string>("State")
                        .HasColumnType("text");

                    b.Property<string>("Symbol")
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("ToTrade")
                        .HasColumnType("boolean");

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

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenEvent", b =>
                {
                    b.HasOne("BscTokenSniper.Handlers.TokenPair", "TokenPair")
                        .WithMany("TokenPairEvents")
                        .HasForeignKey("TokenPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TokenPair");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPairValue", b =>
                {
                    b.HasOne("BscTokenSniper.Handlers.TokenPair", "TokenPair")
                        .WithMany()
                        .HasForeignKey("TokenPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TokenPair");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPair", b =>
                {
                    b.Navigation("LiquidityEvents");

                    b.Navigation("TokenPairEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
