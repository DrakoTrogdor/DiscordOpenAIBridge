using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using DiscordOpenAIBridge.Services;
namespace DiscordOpenAIBridge
{
    public class Program
    {
        // setup our fields we assign later
        private IConfiguration? _config;
        private DiscordSocketClient? _client;
        private CommandHandler _commandHandler;
        private ServiceProvider _services;
        private FileSystemWatcher _configWatcher;
        private static string _logLevel = string.Empty;
        private bool _restartRequested = false;
        private readonly SemaphoreSlim _restartSemaphore = new SemaphoreSlim(1, 1);

        static void Main(string[] args = null)
        {
            if (args?.Length != 0)
            {
                _logLevel = args[0];
            }
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/DiscordPirateTranslator.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public Program()
        {
            LoadConfiguration();

            // Create a FileSystemWatcher to watch for changes in the config file
            _configWatcher = new FileSystemWatcher(AppContext.BaseDirectory, "config.json");
            _configWatcher.Changed += OnConfigChanged;
            _configWatcher.EnableRaisingEvents = true;
        }

        private void LoadConfiguration()
        {
            // create the configuration
            var _builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");

            // build the configuration and assign to _config
            _config = _builder.Build();

            // Get the location of the config.json file and print it
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            Console.WriteLine($"Configuration file loaded from: {configPath}");
        }

        private async void OnConfigChanged(object sender, FileSystemEventArgs e)
        {

            // Wait for the semaphore to be available to prevent concurrent restart requests
            await _restartSemaphore.WaitAsync();

            if (_restartRequested)
            {
                _restartSemaphore.Release();
                return;
            }
            try
            {
                _restartRequested = true;

                // Add a delay before reloading the configuration
                await Task.Delay(1000);

                Console.WriteLine("Configuration has been changed.");

                // Stop the bot
                await _client.StopAsync();
                await _client.LogoutAsync();

                // Reload configuration when the config file is updated
                LoadConfiguration();

                // Recreate the services with the new configuration
                _services = ConfigureServices();

                // Update the _client reference
                _client = _services.GetRequiredService<DiscordSocketClient>();

                // Restart the bot
                await _client.LoginAsync(TokenType.Bot, _config["DiscordToken"]);
                await _client.StartAsync();

                // Restart the CommandHandler
                _commandHandler.Dispose();
                _commandHandler = new CommandHandler(_services);
                await _commandHandler.InitializeAsync();
            }
            finally
            {
                _restartRequested = false;
                _restartSemaphore.Release();
            }
        }

        public async Task MainAsync()
        {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            _services = ConfigureServices();
            // Get the client and assign to client 
            // you get the services via GetRequiredService<T>
            var client = _services.GetRequiredService<DiscordSocketClient>();
            _client = client;

            // setup logging and the ready event
            _services.GetRequiredService<LoggingService>();

            // This is where we get the Discord Token value from the configuration file
            await _client.LoginAsync(TokenType.Bot, _config["DiscordToken"]);
            await _client.StartAsync();

            // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            await _commandHandler.InitializeAsync();
            //await services.GetRequiredService<CommandHandler>().InitializeAsync();

            // Block the program until it is closed.
            await Task.Delay(-1);
        }
        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}] :)");
            return Task.CompletedTask;
        }

        // this method handles the ServiceCollection creation/configuration, and builds out the service provider we can call on later
        private ServiceProvider ConfigureServices()
        {
            // This returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using DiscordPirateTranslator.Services;
            // the config we build is also added, which comes in handy for setting the command prefix!
            var services = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(provider => new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000,
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.DirectMessages
                }))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LoggingService>()
                //.AddDbContext<CsharpiEntities>()
                .AddLogging(configure => configure.AddSerilog());

            if (!string.IsNullOrEmpty(_logLevel))
            {
                switch (_logLevel.ToLower())
                {
                    case "info":
                        {
                            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
                            break;
                        }
                    case "error":
                        {
                            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Error);
                            break;
                        }
                    case "debug":
                        {
                            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);
                            break;
                        }
                    default:
                        {
                            services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Error);
                            break;
                        }
                }
            }
            else
            {
                services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
            }

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
