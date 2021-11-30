using Discord.Commands;
using System.Threading.Tasks;

namespace Umitengu.Modules
{
    public class ML : ModuleBase
    {
        [Command("Generate", RunMode = RunMode.Async)]
        public async Task GenerateAsync()
        {
            await ReplyAsync("Hey");
        }
    }
}
