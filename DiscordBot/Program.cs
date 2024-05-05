using DSharpPlus;
using DSharpPlus.Commands;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;

namespace DiscordBot
{
    class Program
    {
        [RequiresUnreferencedCode("Calls Microsoft.Extensions.Configuration.ConfigurationBinder.GetValue<T>(String)")]
        static async Task Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config/config.json", optional: false, reloadOnChange: true)
                .Build();

            string? command_prefix = config.GetValue<string>("discord:CommandPrefix");
            if (command_prefix == null)
                throw new ArgumentNullException(nameof(command_prefix));

            string? token = config.GetValue<string>("discord:token");
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Error: No discord token found. Please provide a token via the DISCORD_TOKEN environment variable.");
                Environment.Exit(1);
            }

            DiscordShardedClient discord = new (new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(discord);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Use the commands extension
            IReadOnlyDictionary<int, CommandsExtension> commandsExtensions = await discord.UseCommandsAsync(new CommandsConfiguration()
            {
                ServiceProvider = serviceProvider,
                DebugGuildId = 1235311014342295595,
                RegisterDefaultCommandProcessors = true
            });

            // Iterate through each Discord shard
            foreach (CommandsExtension commandsExtension in commandsExtensions.Values)
            {
                // Add all commands by scanning the current assembly
                commandsExtension.AddCommands(typeof(Program).Assembly);
                TextCommandProcessor textCommandProcessor = new(new()
                {
                    // By default, the prefix will be "!"
                    // However the bot will *always* respond to a direct mention
                    // as long as the `DefaultPrefixResolver` is used
                    PrefixResolver = new DefaultPrefixResolver(command_prefix).ResolvePrefixAsync
                });

                // Add text commands with a custom prefix (?ping)
                await commandsExtension.AddProcessorsAsync(textCommandProcessor);
            }

            await discord.StartAsync();
            await Task.Delay(-1);
        }
    }
}
