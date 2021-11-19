﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SammBotNET.Core
{
    public partial class CommandHandler
    {
        public DiscordSocketClient DiscordClient { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public Logger BotLogger { get; set; }

        public AdminService AdminService { get; set; }
        public CommandService CommandsService { get; set; }

        private ConcurrentQueue<SocketMessage> MessageQueue = new();
        private bool ExecutingCommand = false;

        public string CommandName;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, Logger logger)
        {
            CommandsService = commands;
            DiscordClient = client;
            ServiceProvider = services;
            BotLogger = logger;

            DiscordClient.MessageReceived += HandleCommandAsync;
            CommandsService.CommandExecuted += OnCommandExecutedAsync;

            AdminService = services.GetRequiredService<AdminService>();
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            try
            {
                if (result.ErrorReason != "Execution succesful.")
                {
                    if (result.ErrorReason == "Unknown command.")
                    {
                        using (CommandDB CommandDatabase = new())
                        {
                            List<CustomCommand> customCommands = await CommandDatabase.CustomCommand.ToListAsync();
                            foreach (CustomCommand customCommand in customCommands)
                            {
                                if (customCommand.Name == CommandName)
                                {
                                    await context.Channel.SendMessageAsync(customCommand.Reply);
                                    return;
                                }
                            }
                            await context.Channel.SendMessageAsync($"Unknown command! Use the {GlobalConfig.Instance.LoadedConfig.BotPrefix}help command.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync(":warning: **__Error executing command!__**\n" + result.ErrorReason);
                    }
                }
                Thread.Sleep(GlobalConfig.Instance.LoadedConfig.QueueWaitTime);
                MessageQueue.TryDequeue(out SocketMessage dequeuedMessage);
                ExecutingCommand = false;
                await HandleCommandAsync(dequeuedMessage);
            }
            catch (Exception ex)
            {
                BotLogger.LogException(ex);
            }
        }

        public async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (AdminService.ChangingConfig) return;

            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return;

            SocketCommandContext context = new(DiscordClient, message);

            int argPos = 0;
            if (message.Content.StartsWith($"<@!{DiscordClient.CurrentUser.Id}>"))
            {
                await context.Channel.SendMessageAsync($"Hi! I'm **{GlobalConfig.Instance.LoadedConfig.BotName}**!\n" +
                    $"My prefix is `{GlobalConfig.Instance.LoadedConfig.BotPrefix}`! " +
                    $"You can use `{GlobalConfig.Instance.LoadedConfig.BotPrefix}help` to see a list of my available commands!");
            }
            else if (message.HasStringPrefix(GlobalConfig.Instance.LoadedConfig.BotPrefix, ref argPos))
            {
                if (message.Content.Length == GlobalConfig.Instance.LoadedConfig.BotPrefix.Length) return;
                if (ExecutingCommand)
                {
                    MessageQueue.Enqueue(messageParam);
                    return;
                }

                ExecutingCommand = true;
                CommandName = message.Content.Remove(0, GlobalConfig.Instance.LoadedConfig.BotPrefix.Length).Split()[0];

                BotLogger.Log(LogLevel.Message, string.Format(GlobalConfig.Instance.LoadedConfig.CommandLogFormat,
                                                message.Content, message.Channel.Name, message.Author.Username));

                await CommandsService.ExecuteAsync(context, argPos, ServiceProvider);
            }
            else
            {
                try
                {
                    if (message.Content.Length < 20 || message.Content.Length > 64) return;
                    if (message.Attachments.Count > 0 && message.Content.Length == 0) return;
                    if (GlobalConfig.Instance.UrlRegex.IsMatch(message.Content)) return;
                    if (GlobalConfig.Instance.LoadedConfig.BannedPrefixes.Any(x => message.Content.StartsWith(x))) return;

                    using (PhrasesDB PhrasesDatabase = new())
                    {
                        List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();

                        foreach (Phrase phrase in phrases)
                        {
                            if (message.Content == phrase.Content)
                            {
                                return;
                            }
                        }

                        await PhrasesDatabase.AddAsync(new Phrase
                        {
                            Content = message.Content,
                            AuthorId = message.Author.Id,
                            ServerId = context.Guild.Id,
                            CreatedAt = message.Timestamp.ToUnixTimeSeconds()
                        });
                        await PhrasesDatabase.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    BotLogger.LogException(ex);
                }
            }
        }
    }
}
