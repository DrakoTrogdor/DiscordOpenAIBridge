using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DiscordOpenAIBridge.Modules;

namespace DiscordOpenAIBridge.Services
{
    public class CommandHandler : IDisposable
    {
        // setup fields to be set later in the constructor
        private readonly IConfiguration _config;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        public CommandHandler(IServiceProvider services)
        {
            // juice up the fields with these services
            // since we passed the services in, we can use GetRequiredService to pass them into the fields set earlier
            _config = services.GetRequiredService<IConfiguration>();
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _logger = services.GetRequiredService<ILogger<CommandHandler>>();
            _services = services;

            // take action when we execute a command
            _commands.CommandExecuted += CommandExecutedAsync;

            // take action when we receive a message (so we can process it, and see if it is a valid command)
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            // register modules that are public and inherit ModuleBase<T>.
            // await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // register modules that are public and inherit ModuleBase<T>.
            //await _commands.AddModuleAsync<CommandsOpenAI>(_services.CreateScope().ServiceProvider);

            // register modules that are public and inherit ModuleBase<T>.
            var result = await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Write added commands to the console
            foreach (var module in result)
            {
                foreach (var command in module.Commands)
                {
                    _logger.LogInformation($"Command '{command.Name}' added from module '{module.Name}'");
                }
            }
        }

        public void Dispose()
        {
            // Dispose of resources here, such as unhooking events
            _commands.CommandExecuted -= CommandExecutedAsync;
            _client.MessageReceived -= MessageReceivedAsync;
        }

        // this class is where the magic starts, and takes actions upon receiving messages
        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Log the raw message
            _logger.LogInformation($"Received message from {rawMessage.Author}: {rawMessage.Content}");

            // ensures we don't process system/other bot messages
            if (!(rawMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            // sets the argument position away from the prefix we set
            var argPos = 0;

            // get prefix from the configuration file
            char prefix = Char.Parse(_config["DiscordPrefix"]);

            // determine if the message has a valid prefix, and adjust argPos based on prefix
            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasCharPrefix(prefix, ref argPos) || message.Channel is IDMChannel)
            {
                var context = new SocketCommandContext(_client, message);

                // execute command if one is found that matches
                await _commands.ExecuteAsync(context, argPos, _services);
            }
            else
            {
                return;
            }

        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // if a command isn't found, log that info to console and exit this method
            if (!command.IsSpecified)
            {
                _logger.LogError($"Command failed to execute for [{context.User.Username}] <-> [{result.ErrorReason}]!");
                return;
            }


            // log success to the console and exit this method
            if (result.IsSuccess)
            {
                string guildName = context.Guild?.Name ?? "DM channel";
                _logger.LogInformation($"Command [{command.Value.Name}] executed for [{context.User.Username}] in [{guildName}]");
                return;
            }

            // failure scenario, let's let the user know
            await context.Channel.SendMessageAsync($"Sorry, {context.User.Username}... something went wrong -> [{result}]!");
        }
    }
}