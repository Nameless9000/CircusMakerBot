using DSharpPlus;
using DSharpPlus.Commands;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Commands.Processors.SlashCommands;

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
                DebugGuildId = 0,
                RegisterDefaultCommandProcessors = true
            });

            // Iterate through each Discord shard
            foreach (CommandsExtension commandsExtension in commandsExtensions.Values)
            {
                // Add all commands by scanning the current assembly
                commandsExtension.AddCommands(typeof(Program).Assembly);

                TextCommandProcessor textCommandProcessor = new(new()
                {
                    PrefixResolver = new DefaultPrefixResolver(command_prefix).ResolvePrefixAsync
                });

                SlashCommandProcessor slashCommandProcessor = new();

                // Add text commands with a custom prefix (?ping)
                await commandsExtension.AddProcessorsAsync(textCommandProcessor);
                await commandsExtension.AddProcessorsAsync(slashCommandProcessor);
            }

            await discord.StartAsync();
            await Task.Delay(-1);
        }
    }
}
