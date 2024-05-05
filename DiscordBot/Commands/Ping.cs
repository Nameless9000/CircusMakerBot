using DSharpPlus.Commands;

public class PingCommand
{
    [Command("ping")]
    public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"Pong! Latency is {context.Client.Ping}ms.");
}