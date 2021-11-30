using Discord.Commands;
using System.Threading.Tasks;

namespace Umitengu.Modules
{
    public class MachineLearning : ModuleBase
    {
        [Command("Generate", RunMode = RunMode.Async)]
        public async Task Generate()
        {
            await ReplyAsync("Hey");
        }
    }
}
