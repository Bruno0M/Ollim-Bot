using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using Ollim.Bot.Services;

namespace Ollim.Bot.Commands.Modules
{
    public class TestGeminiCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("gemininho", "testano essa tal de AI")]
        public async Task Gemininho(string prompt, Attachment attachment = null)
        {
            await DeferAsync();

            var gemi = new GeminiService();
            var generateContent = "";

            if (attachment != null)
            {
                var filePath = attachment.Filename;
                var response = await gemi.UploadFile(attachment.Url, filePath, "application/pdf");
                generateContent = await gemi.GenerateContent(prompt, response, filePath);
            }
            //var prompt = "Summarize what's in this pdf";

            generateContent = await gemi.GenerateContent(prompt);

            await ModifyOriginalResponseAsync(props => props.Content = generateContent);
            //await ModifyOriginalResponseAsync(props => props.Content = "Ok");

        }
    }
}
