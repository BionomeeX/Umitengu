using Discord.Commands;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using Discord;
using Discord.WebSocket;

namespace Umitengu.Modules
{
    public class MachineLearning : ModuleBase
    {
        public static async Task<SlashCommandProperties> GetCommand()
        {
            return new SlashCommandBuilder()
            {
                Name = "generate",
                Description = "Generate an image using machine learning",
                Options = new()
                {
                    new SlashCommandOptionBuilder()
                    {
                        Name = "prompt",
                        Description = "Starting prompt",
                        Type = ApplicationCommandOptionType.String,
                        IsRequired = false
                    }
                }
            }.Build();
        }

        private static bool _isBusy;

        private static HttpClient Http = new();
        private string ParseAndSaveUrl(string url, string fileName)
        {
            var extension = Path.GetExtension(url).Split('?')[0];
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            {
                throw new ArgumentException("Invalid file type " + extension);
            }

            // Download the image
            var bytes = Http.GetByteArrayAsync(url).GetAwaiter().GetResult();
            if (bytes.Length > 8_000_000)
            {
                throw new ArgumentException("Your image must be less than 8MB");
            }
            var path = Program.Credentials.Path + (Program.Credentials.Path.EndsWith("/") ? "" : "/") + fileName + extension;
            File.WriteAllBytes(path, bytes);
            return path;
        }

        public async Task Generate(SocketSlashCommand ctx)
        {
            if (_isBusy) {
                await ReplyAsync("Already in use, please try later ...");
                return;
            }

            _isBusy = true;
            await Program.Client.SetActivityAsync(new Game("An image being generated...", ActivityType.Watching));

            try
            {
                var prompt = (string)ctx.Data.Options.First(x => x.Name == "prompt").Value ?? "";
                var width = (int?)ctx.Data.Options.First(x => x.Name == "width").Value ?? 128;
                var height = (int?)ctx.Data.Options.First(x => x.Name == "height").Value ?? 128;
                var nbGen = (int?)ctx.Data.Options.First(x => x.Name == "nbgen").Value ?? 500;
                var startImg = (string)ctx.Data.Options.First(x => x.Name == "startimg").Value ?? "";
                var helpImg = (string)ctx.Data.Options.First(x => x.Name == "helpimg").Value ?? "";

                var iiFile = "";
                if (startImg != "")
                {
                    iiFile = ParseAndSaveUrl(startImg, "ii");
                }
                var ipFile = "";
                if (helpImg != "")
                {
                    ipFile = ParseAndSaveUrl(helpImg, "ip");
                }

                var msg = await ReplyAsync("generating : " + prompt);

                ProcessStartInfo si = new()
                {
                    Arguments = $"generate.py " + (prompt == "" ? "" : $"-p \"{prompt}\" ") + (iiFile == "" ? "" : $"-ii {iiFile} ") + (ipFile == "" ? "" : $"-ip {ipFile} ") + $"-s {width} {height} -i {nbGen}",
                    WorkingDirectory = Program.Credentials.Path,
                    FileName = "python"
                };

                Process.Start(si).WaitForExit();
                await msg.DeleteAsync();
                await Context.Channel.SendFileAsync(Program.Credentials.Path + "/output.png");
                File.Delete(Program.Credentials.Path + "/output.png");
                if (iiFile != "")
                {
                    File.Delete(iiFile);
                }
                if (ipFile != "")
                {
                    File.Delete(ipFile);
                }
            }
            catch (Exception)
            {
                _isBusy = false;
                await Program.Client.SetActivityAsync(new Game("Ready", ActivityType.Watching));
                throw;
            }

            _isBusy = false;
            await Program.Client.SetActivityAsync(new Game("Ready", ActivityType.Watching));
        }
    }
}
