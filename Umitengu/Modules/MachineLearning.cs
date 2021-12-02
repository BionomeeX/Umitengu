﻿using Discord.Commands;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using Discord;

namespace Umitengu.Modules
{
    public class MachineLearning : ModuleBase
    {

        public static bool _isBusy;

        private string[] GetParameterArgument(string[] args, string flag, int argCount, string[] def)
        {
            var index = Array.IndexOf(args, flag);
            if (index == -1)
            {
                return def;
            }
            if (index + argCount >= args.Length)
            {
                return null;
            }
            var res = args[(index + 1)..(index + argCount + 1)];
            if (res.Any(x => x.StartsWith('-')))
            {
                return null;
            }
            return res;
        }

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

        [Command("Generate", RunMode = RunMode.Async)]
        public async Task Generate(params string[] args)
        {
            if (_isBusy) {
                await ReplyAsync("Already in use, please try later ...");
                return;
            }

            _isBusy = true;
            await Program.Client.SetActivityAsync(new Game("an image being generated...", ActivityType.Watching));

            try
            {
                var pFlag = GetParameterArgument(args, "-p", 1, new[] { "" });
                var sFlag = GetParameterArgument(args, "-s", 2, new[] { "128", "128" });
                var iFlag = GetParameterArgument(args, "-i", 1, new[] { "500" });

                var iiFlag = GetParameterArgument(args, "-ii", 1, new[] { "" });
                var ipFlag = GetParameterArgument(args, "-ip", 1, new[] { "" });

                if (pFlag == null || sFlag == null || iFlag == null || iiFlag == null || ipFlag == null)
                {
                    throw new ArgumentException("Invalid input format", nameof(args));
                }

                string prompt = pFlag[0];
                if (prompt.Contains('"') || iiFlag[0].Contains('"') || ipFlag[0].Contains('"'))
                {
                    throw new InvalidOperationException("Your command can't contains double quotes");
                }

                var iiFile = "";
                if (iiFlag[0] != "")
                {
                    iiFile = ParseAndSaveUrl(iiFlag[0], "ii");
                }
                var ipFile = "";
                if (ipFlag[0] != "")
                {
                    ipFile = ParseAndSaveUrl(ipFlag[0], "ip");
                }

                (int X, int Y) dimension = (int.Parse(sFlag[0]), int.Parse(sFlag[1]));
                int nbIteration = int.Parse(iFlag[0]);

                var promptIndex = Array.IndexOf(args, "-p");

                var msg = await ReplyAsync("generating : " + prompt);

                ProcessStartInfo si = new()
                {
                    Arguments = $"generate.py " + (pFlag[0] == "" ? "" : $"-p \"{prompt}\" ") + (iiFile == "" ? "" : $"-ii {iiFile} ") + (ipFile == "" ? "" : $"-ip {ipFile} ") + $"-s {dimension.X} {dimension.Y} -i {nbIteration}",
                    WorkingDirectory = Program.Credentials.Path,
                    FileName = "python"
                };

                Console.WriteLine(args);

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
                await Program.Client.SetActivityAsync(new Game("u.help", ActivityType.Watching));
                throw;
            }

            _isBusy = false;
            await Program.Client.SetActivityAsync(new Game("u.help", ActivityType.Watching));
        }
    }
}
