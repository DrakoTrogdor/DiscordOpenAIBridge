using Discord.Commands;
using System.Text;

namespace DiscordOpenAIBridge.Modules
{
    // for commands to be available, and have the Context passed to them, we must inherit ModuleBase
    public class CommandsTemplate : ModuleBase
    {
        [Command("aboutTLAP")]
        public async Task CommandAbout()
        {
            // initialize empty string builder for reply
            var sb = new StringBuilder();

            // get user info from the Context
            var user = Context.User;
            
            // build out the reply
            sb.AppendLine($"[{user.Username}], this bot is for translating text that you type into Pirate Lingo.");
            sb.AppendLine("Use 'tlap <message>' to use.");

            // send simple string reply
            await ReplyAsync(sb.ToString());
        }        
    }
}