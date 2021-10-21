﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SammBotNET.Extensions;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Information")]
    [Summary("Bot information and statistics.")]
    [Group("info")]
    public class InformationModule : ModuleBase<SocketCommandContext>
    {
        [Command("full", RunMode = RunMode.Async)]
        [Summary("Shows the FULL information of the bot.")]
        public async Task<RuntimeResult> InformationFullAsync()
        {
            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Information", "All public information about the bot.");

            string elapsedTime = string.Format("{0:00}d{1:00}h{2:00}m",
                GlobalConfig.Instance.RuntimeStopwatch.Elapsed.Days,
                GlobalConfig.Instance.RuntimeStopwatch.Elapsed.Hours,
                GlobalConfig.Instance.RuntimeStopwatch.Elapsed.Minutes);

            embed.AddField("Bot Version", $"`{GlobalConfig.Instance.LoadedConfig.BotVersion}`", true);
            embed.AddField(".NET Version", $"`{RuntimeInformation.FrameworkDescription}`", true);
            embed.AddField("Ping", $"`{Context.Client.Latency}ms.`", true);
            embed.AddField("Im In", $"`{Context.Client.Guilds.Count} server/s.`", true);
            embed.AddField("Uptime", $"`{elapsedTime}`", true);
            embed.AddField("Host", $"`{FriendlyOSName()}`", true);

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("servers", RunMode = RunMode.Async)]
        [Alias("guilds")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Shows a list of all the servers the bot is in.")]
        public async Task<RuntimeResult> ServersAsync()
        {
            string builtMsg = "I am invited in the following servers:\n```\n";
            string inside = string.Empty;

            int i = 1;
            foreach (SocketGuild guild in Context.Client.Guilds)
            {
                inside += $"{i}. {guild.Name} ({guild.Id}) with {guild.MemberCount} members.\n";
                i++;
            }
            inside += "```";
            builtMsg += inside;
            await ReplyAsync(builtMsg);

            return ExecutionResult.Succesful();
        }

        public string FriendlyOSName()
        {
            string osName = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Version version = Environment.OSVersion.Version;

                switch (version.Major)
                {
                    case 6:
                        osName = version.Minor switch
                        {
                            1 => "Windows 7",
                            2 => "Windows 8",
                            3 => "Windows 8.1",
                            _ => "Unknown Windows",
                        };
                        break;
                    case 10:
                        switch (version.Minor)
                        {
                            case 0:
                                if (version.Build >= 22000) osName = "Windows 11";
                                else osName = "Windows 10";

                                break;
                            default: osName = "Unknown Windows"; break;
                        }
                        break;
                    default:
                        osName = "Unknown Windows";
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists("/etc/issue.net"))
                    osName = File.ReadAllText("/etc/issue.net");
                else
                    osName = "Linux";
            }

            return osName;
        }
    }
}