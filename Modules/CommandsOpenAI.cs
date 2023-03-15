using Discord.Commands;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace DiscordOpenAIBridge.Modules
{
    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class CommandsOpenAI : ModuleBase
    {

        public IConfiguration Config { get; set; }

        public CommandsOpenAI(IConfiguration config)
        {
            Config = config;
        }

        private string GetDirective(string commandName)
        {
            var directives = Config.GetSection("Directives").Get<List<Directive>>();
            var directive = directives.FirstOrDefault(d => d.Command.Equals(commandName, StringComparison.OrdinalIgnoreCase));

            return directive?.Text ?? string.Empty;
        }

        public class Directive
        {
            public string Command { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
        }

        private async Task<string> TranslateTextAsync(string inputText, string directive)
        {
            using var httpClient = new HttpClient();
            string apiEndpoint = "https://api.openai.com/v1/chat/completions";

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config["ChatGptApiKey"]);

            var messages = new[]
            {
                new { role = "system", content = directive },
                new { role = "user", content = inputText }
            };

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = messages,
                max_tokens = 250,
                n = 1,
                stop = default(string),
                temperature = 0.8,
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await httpClient.PostAsync(apiEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenAIResponse>(jsonResponse);
                if (result?.choices != null && result.choices.Count > 0)
                {
                    var choice = result.choices[0];
                    if (choice?.message != null)
                    {
                        var messageContent = choice.message.content;
                        if (messageContent != null)
                        {
                            return messageContent.Trim();
                        }
                    }
                }
            }
            else
            {
                throw new HttpRequestException($"API call failed with status code {response.StatusCode}");
            }

            return string.Empty;
        }

        [Command("tlap")]
        [Summary("Translates text to Pirate Lingo.")]
        public async Task Command_tlap([Remainder] string inputText)
        {
            string directive = GetDirective("tlap");

            string translatedText = await TranslateTextAsync(inputText, directive);
            await ReplyAsync(translatedText);
        }

        [Command("zapp")]
        [Summary("Translates text to speak like Zapp Brannigan.")]
        public async Task Command_zapp([Remainder] string inputText)
        {
            string directive = GetDirective("zapp");

            string translatedText = await TranslateTextAsync(inputText, directive);
            await ReplyAsync(translatedText);
        }

    }
    public class OpenAIResponse
    {
        public List<Choice> choices { get; set; } = new List<Choice>();
    }

    public class Choice
    {
        public Message message { get; set; } = new Message();
        public string finish_reason { get; set; } = string.Empty;
        public int index { get; set; } = 0;
    }

    public class Message
    {
        public string role { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
    }
}