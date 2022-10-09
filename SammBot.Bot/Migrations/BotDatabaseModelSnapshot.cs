﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SammBot.Bot.Database;

#nullable disable

namespace SammBot.Bot.Migrations
{
    [DbContext(typeof(BotDatabase))]
    partial class BotDatabaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.9");

            modelBuilder.Entity("SammBotNET.Database.GuildConfig", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarningLimit")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarningLimitAction")
                        .HasColumnType("INTEGER");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("SammBotNET.Database.Pronoun", b =>
                {
                    b.Property<ulong>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DependentPossessive")
                        .HasColumnType("TEXT");

                    b.Property<string>("IndependentPossessive")
                        .HasColumnType("TEXT");

                    b.Property<string>("Object")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReflexivePlural")
                        .HasColumnType("TEXT");

                    b.Property<string>("ReflexiveSingular")
                        .HasColumnType("TEXT");

                    b.Property<string>("Subject")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId");

                    b.ToTable("Pronouns");
                });

            modelBuilder.Entity("SammBotNET.Database.UserTag", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("CreatedAt")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Reply")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("UserTags");
                });

            modelBuilder.Entity("SammBotNET.Database.UserWarning", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<long>("Date")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("UserWarnings");
                });
#pragma warning restore 612, 618
        }
    }
}
