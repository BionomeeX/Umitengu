using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace Umitengu.Modules
{
    public class Communication : ModuleBase
    {
        [Command("Help")]
        private async Task HelpAsync()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = new Color(6, 127, 189),
                Title = "Generate",
                Description =
                    "Generate an image\n" +
                    "**-p \"[text]\":** Text to generate the image from\n" +
                    "**-s [width] [length]:** Size of the image to generate\n" +
                    "**-i [iterations]:** Number of iterations to do the generation with\n" +
                    "**-ii [url]:** The generation will start from the given image\n" +
                    "**-ip [url]:** Same as -p but with an image\n"
            }.Build());
        }
    }
}
