using System;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Configuration;

namespace DiscordBot
{
    class Program
    {
        /* This is the cancellation token we'll use to end the bot if needed(used for most async stuff). */
        private CancellationTokenSource? cts { get; set; }

        /* We'll load the app config into this when we create it a little later. */
        private IConfigurationRoot? config;

        /* These are the discord library's main classes */
        private DiscordClient? discord;
        private CommandsNextExtension? commands;
        private InteractivityExtension? interactivity;

        static async Task Main(string[] args) => await new Program().InitBot(args);

        async Task InitBot(string[] args)
        {
            try
            {
                Console.WriteLine("[info] Welcome to my bot!");
                cts = new CancellationTokenSource();

                // Load the config file(we'll create this shortly)
                Console.WriteLine("[info] Loading config file..");
                config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .Build();

                // Create the DSharpPlus client
                Console.WriteLine("[info] Creating discord client..");
                discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = config.GetValue<string>("discord:token"),
                    TokenType = TokenType.Bot
                });

                // Create the interactivity module(I'll show you how to use this later on)
                interactivity = discord.UseInteractivity(new InteractivityConfiguration()
                {
                    Timeout = TimeSpan.FromSeconds(30) // Default time to wait for interactive commands like waiting for a message or a reaction
                });

                // Build dependancies and then create the commands module.
                commands = discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefixes = [config.GetValue<string>("discord:CommandPrefix")], // Load the command prefix(what comes before the command, eg "!" or "/") from our config file
                });

                // TODO: Add command loading!

                RunAsync(args).Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        async Task RunAsync(string[] args)
        {
            // Connect to discord's service
            Console.WriteLine("Connecting..");
            await discord.ConnectAsync();
            Console.WriteLine("Connected!");

            // Keep the bot running until the cancellation token requests we stop
            while (!cts.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}
