using DSharpPlus;
using DSharpPlus.Commands;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Entities;
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

            DiscordClient discord = new (new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });

            CommandsExtension commandsExtension = discord.UseCommands(new CommandsConfiguration()
            {
                ServiceProvider = new ServiceCollection().AddLogging().BuildServiceProvider()
            });

            commandsExtension.AddCommands(typeof(Program).Assembly);

            await commandsExtension.AddProcessorsAsync(
                new TextCommandProcessor(new()
                {
                    PrefixResolver = new DefaultPrefixResolver(command_prefix).ResolvePrefixAsync
                }),
                new SlashCommandProcessor()
            );

            // Start bot
            DiscordActivity activity = new()
            {
                ActivityType = DiscordActivityType.Watching,
                Name = "Circuit Maker 2",
                StreamUrl = "https://www.roblox.com/games/6652606416/Circuit-Maker-2",
            };

            await discord.ConnectAsync(activity, DiscordUserStatus.DoNotDisturb);

            await Task.Delay(-1);
        }
    }
}
