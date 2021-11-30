using Discord.Commands;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;

namespace Umitengu.Modules
{
    public class MachineLearning : ModuleBase
    {

        public static bool _isBusy;

        [Command("Generate", RunMode = RunMode.Async)]
        public async Task Generate([Remainder] string args = "")
        {
            if(_isBusy) {
                await ReplyAsync("Already in use, please try later ...");
                return;
            }

            _isBusy = true;

            var msg = await ReplyAsync("generating : " + args);

            ProcessStartInfo si = new()
            {
                Arguments = $"generate.py {args}",
                WorkingDirectory = Program.Credentials.Path,
                FileName = "python"
            };

            Console.WriteLine(args);

            Process.Start(si).WaitForExit();
            await msg.DeleteAsync();
            await Context.Channel.SendFileAsync(Program.Credentials.Path + "/output.png");
            File.Delete(Program.Credentials.Path + "/output.png");

            _isBusy = false;
        }
    }
}
