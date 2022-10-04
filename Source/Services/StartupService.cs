﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Figgle;
using Matcha;
using Pastel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace SammBotNET.Services
{
    public class StartupService
    {
        private IServiceProvider ServiceProvider;
        private DiscordShardedClient ShardedClient { get; set; }
        private CommandService CommandsService { get; set; }
        private Logger BotLogger { get; set; }

        private Timer _StatusTimer;
        private Timer _AvatarTimer;

        private AutoDequeueList<string> RecentAvatars;

        private bool _EventsSetUp = false;
        private int _ShardsReady = 0;
        
        public StartupService(IServiceProvider ServiceProvider, DiscordShardedClient ShardedClient, CommandService CommandsService, Logger Logger)
        {
            this.ServiceProvider = ServiceProvider;
            this.ShardedClient = ShardedClient;
            this.CommandsService = CommandsService;
            BotLogger = Logger;

            RecentAvatars = new AutoDequeueList<string>(Settings.Instance.LoadedConfig.AvatarRecentQueueSize);
        }

        public async Task StartAsync()
        {
            BotLogger.Log("Logging in as a bot...", LogSeverity.Information);
            await ShardedClient.LoginAsync(TokenType.Bot, Settings.Instance.LoadedConfig.BotToken);
            await ShardedClient.StartAsync();
            BotLogger.Log("Succesfully connected to web socket.", LogSeverity.Success);

            ShardedClient.ShardConnected += OnShardConnected;
            ShardedClient.ShardReady += OnShardReady;
            ShardedClient.ShardDisconnected += OnShardDisconnect;

            await CommandsService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
            Settings.Instance.StartupStopwatch.Stop();

            Console.Title = $"{Settings.BOT_NAME} {Settings.Instance.LoadedConfig.BotVersion}";

            string discordNetVersion = Assembly.GetAssembly(typeof(SessionStartLimit)).GetName().Version.ToString(3);
            string matchaVersion = Assembly.GetAssembly(typeof(MatchaLogger)).GetName().Version.ToString(3);
            
            Console.Clear();

            Console.Write(FiggleFonts.Slant.Render(Settings.BOT_NAME).Pastel(Color.SkyBlue));
            Console.Write("===========".Pastel(Color.CadetBlue));
            Console.Write($"Source code {Settings.Instance.LoadedConfig.BotVersion}, Discord.NET {discordNetVersion}".Pastel(Color.LightCyan));
            Console.WriteLine("===========".Pastel(Color.CadetBlue));
            Console.WriteLine();

            Settings.Instance.RuntimeStopwatch.Start();

            BotLogger.Log($"Using MatchaLogger {matchaVersion}.", LogSeverity.Information);

            BotLogger.Log($"{Settings.BOT_NAME} took" +
                $" {Settings.Instance.StartupStopwatch.ElapsedMilliseconds}ms to boot.", LogSeverity.Information);
            
#if DEBUG
            BotLogger.Log($"{Settings.BOT_NAME} has been built on Debug configuration. Extra logging will be available.", LogSeverity.Warning);
#endif
        }

        private Task OnShardConnected(DiscordSocketClient ShardClient)
        {
            BotLogger.Log($"Shard #{ShardClient.ShardId} has connected to the gateway.", LogSeverity.Debug);

            return Task.CompletedTask;
        }

        private Task OnShardReady(DiscordSocketClient ShardClient)
        {
            _ShardsReady++;
            
            BotLogger.Log($"Shard #{ShardClient.ShardId} is ready to run.", LogSeverity.Debug);
            
            if (!_EventsSetUp && _ShardsReady == ShardedClient.Shards.Count)
            {
                if (Settings.Instance.LoadedConfig.StatusList.Count > 0 && Settings.Instance.LoadedConfig.RotatingStatus)
                    _StatusTimer = new Timer(RotateStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

                if (Settings.Instance.LoadedConfig.RotatingAvatar)
                {
                    TimeSpan avatarDelay = TimeSpan.FromHours(Settings.Instance.LoadedConfig.AvatarRotationTime);

                    _AvatarTimer = new Timer(RotateAvatar, null, avatarDelay, avatarDelay);
                }

                BotLogger.Log($"{Settings.BOT_NAME} is ready to run.", LogSeverity.Success);

                _EventsSetUp = true;
            }

            return Task.CompletedTask;
        }

        private Task OnShardDisconnect(Exception IncludedException, DiscordSocketClient ShardClient)
        {
            BotLogger.Log($"Shard #{ShardClient.ShardId} has disconnected from the gateway! Reason: " + IncludedException.Message, LogSeverity.Warning);

            return Task.CompletedTask;
        }

        private async void RotateStatus(object State)
        {
            BotStatus chosenStatus = Settings.Instance.LoadedConfig.StatusList.PickRandom();

            string gameUrl = chosenStatus.Type == 1 ? Settings.Instance.LoadedConfig.TwitchUrl : null;

            await ShardedClient.SetGameAsync(chosenStatus.Content, gameUrl, (ActivityType)chosenStatus.Type);
        }

        private async void RotateAvatar(object State)
        {
            List<string> avatarList = Directory.EnumerateFiles(Path.Combine(Settings.Instance.BotDataDirectory, "Avatars")).ToList();
            if (avatarList.Count < 2) return;

            List<string> filteredList = avatarList.Except(RecentAvatars).ToList();

            string chosenAvatar = filteredList.PickRandom();
            BotLogger.Log($"Setting bot avatar to \"{Path.GetFileName(chosenAvatar)}\".", LogSeverity.Debug);

            using (FileStream avatarStream = new FileStream(chosenAvatar, FileMode.Open))
            {
                Image loadedAvatar = new Image(avatarStream);

                await ShardedClient.CurrentUser.ModifyAsync(x => x.Avatar = loadedAvatar);
            }

            RecentAvatars.Push(chosenAvatar);
        }
    }
}
