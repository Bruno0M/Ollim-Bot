using Discord;
using Discord.Interactions;
using Ollim.Bot.Services;

namespace Ollim.Bot.Commands.Modules
{
    public class TestGeminiCommandHandler : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly GeminiService _geminiService;

        public TestGeminiCommandHandler(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [SlashCommand("gemininho", "testano essa tal de AI")]
        public async Task Gemininho(string prompt, Attachment attachment = null)
        {
            await DeferAsync();

            var generateContent = "";

            if (attachment != null)
            {
                var filePath = attachment.Filename;
                var response = await _geminiService.UploadFile(attachment.Url, filePath, "application/pdf");
                generateContent = await _geminiService.GenerateContent(prompt, response, filePath);
            }
            //var prompt = "Summarize what's in this pdf";

            generateContent = await _geminiService.GenerateContent(prompt);

            await ModifyOriginalResponseAsync(props => props.Content = generateContent);
            //await ModifyOriginalResponseAsync(props => props.Content = "Ok");

        }
    }
}
