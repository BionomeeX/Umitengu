using Umitengu.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Umitengu
{
    class Program
    {
        public static async Task Main()
            => await new Program().MainAsync();


        public static readonly DiscordSocketClient Client = new(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
        });


        public static Credentials Credentials;

        private Program()
        {
            Client.Log += (msg) =>
            {
                Console.WriteLine(msg);
                return Task.CompletedTask;
            };
        }

        private async Task MainAsync()
        {
            Client.Ready += Ready;
            Client.SlashCommandExecuted += SlashCommandExecuted;

            Credentials = JsonSerializer.Deserialize<Credentials>(File.ReadAllText("Keys/Credentials.json"), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            await Client.LoginAsync(TokenType.Bot, Credentials.BotToken);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task SlashCommandExecuted(SocketSlashCommand arg)
        {
            if (arg.CommandName == "generate")
            {
                await arg.DeferAsync();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await MachineLearning.GenerateAsync(arg);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await arg.ModifyOriginalResponseAsync(x => x.Embed = new EmbedBuilder
                        {
                            Color = Color.Red,
                            Title = ex.GetType().ToString(),
                            // We don't log raw FileNotFoundException message because they may contains local path
                            Description = ex is FileNotFoundException ? "Could not find file" : ex.Message
                        }.Build());
                    }
                });
            }
        }

        private async Task Ready()
        {
            await Client.CreateGlobalApplicationCommandAsync(MachineLearning.GetCommand());
            await Client.BulkOverwriteGlobalApplicationCommandsAsync(new[] { MachineLearning.GetCommand() });
            await Client.SetActivityAsync(new Game("Ready", ActivityType.Watching));
        }
    }
}
