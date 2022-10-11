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
    [Migration("20221010043417_InitialCreate")]
    partial class InitialCreate
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

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int>("TokenId")
                        .HasColumnType("integer");

                    b.Property<int?>("TokenPairId")
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

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<int?>("TokenPairId")
                        .HasColumnType("integer");

                    b.HasKey("TokenId");

                    b.HasIndex("TokenPairId");

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

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("TokenPairId");

                    b.ToTable("TokenPairs");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPairValue", b =>
                {
                    b.Property<int>("TokenPairValueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TokenPairValueId"));

                    b.Property<int>("TokenPairId")
                        .HasColumnType("integer");

                    b.Property<int>("Value")
                        .HasColumnType("integer");

                    b.HasKey("TokenPairValueId");

                    b.HasIndex("TokenPairId")
                        .IsUnique();

                    b.ToTable("TokenPairValues");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.LiquidityEvent", b =>
                {
                    b.HasOne("BscTokenSniper.Handlers.TokenPair", "TokenPair")
                        .WithMany("LiquidityEvents")
                        .HasForeignKey("TokenPairId");

                    b.Navigation("TokenPair");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.Token", b =>
                {
                    b.HasOne("BscTokenSniper.Handlers.TokenPair", null)
                        .WithMany("Token")
                        .HasForeignKey("TokenPairId");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPairValue", b =>
                {
                    b.HasOne("BscTokenSniper.Handlers.TokenPair", "TokenPair")
                        .WithOne("Value")
                        .HasForeignKey("BscTokenSniper.Handlers.TokenPairValue", "TokenPairId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TokenPair");
                });

            modelBuilder.Entity("BscTokenSniper.Handlers.TokenPair", b =>
                {
                    b.Navigation("LiquidityEvents");

                    b.Navigation("Token");

                    b.Navigation("Value");
                });
#pragma warning restore 612, 618
        }
    }
}
