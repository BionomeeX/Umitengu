using Umitengu.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

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
        private readonly CommandService _commands = new();


        public static Credentials Credentials;

        private Program()
        {
            Client.Log += (msg) =>
            {
                Console.WriteLine(msg);
                return Task.CompletedTask;
            };
            _commands.Log += async (msg) =>
            {
                Console.WriteLine(msg);
#if DEBUG
                if (msg.Exception is CommandException ce)
                {
                    if (ce.InnerException is ArgumentException ae)
                    {
                        await ce.Context.Channel.SendMessageAsync(ae.Message);
                    }
                    else
                    {
                        await ce.Context.Channel.SendMessageAsync(embed: new EmbedBuilder
                        {
                            Color = Color.Red,
                            // We don't log raw FileNotFoundException message because they may contains local path
                            Title = msg.Exception.InnerException.GetType().ToString(),
                            Description = ce.InnerException is FileNotFoundException ? "Could not find file" : msg.Exception.InnerException.Message
                        }.Build());
                    }
                }
#endif
            };
        }

        private async Task MainAsync()
        {
            Client.MessageReceived += HandleCommandAsync;
            Client.Ready += Ready;

            await _commands.AddModuleAsync<MachineLearning>(null);
            await _commands.AddModuleAsync<Communication>(null);

            Credentials = JsonSerializer.Deserialize<Credentials>(File.ReadAllText("Keys/Credentials.json"), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            await Client.LoginAsync(TokenType.Bot, Credentials.BotToken);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Ready()
        {
            await Client.SetActivityAsync(new Game("u.help", ActivityType.CustomStatus));
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage msg || arg.Author.IsBot || arg.Channel is not ITextChannel)
            {
                return;
            }
            int pos = 0;
            if (msg.HasMentionPrefix(Client.CurrentUser, ref pos) || msg.HasStringPrefix("u.", ref pos))
            {
                SocketCommandContext context = new(Client, msg);
                var result = await _commands.ExecuteAsync(context, pos, null);
                if (!result.IsSuccess)
                {
                    if (result.Error == CommandError.UnmetPrecondition || result.Error == CommandError.BadArgCount
                        || result.Error == CommandError.ParseFailed)
                    {
                        await msg.Channel.SendMessageAsync(result.ErrorReason);
                    }
                }
            }
        }
    }
}
