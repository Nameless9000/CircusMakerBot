using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using System.ComponentModel;

namespace Util;

public class HelpCommand
{
    private static string ProcessCommand(Command command)
    {
        string output = $"- **{command.FullName}**";

        if (command.Method != null)
            foreach (var parameter in command.Parameters)
                output += $" *<{parameter.Name}: {parameter.Type.Name}{(parameter.DefaultValue.HasValue ? " = " + parameter.DefaultValue.Value : "")}>*";
        else
            output += " *(Group)*";

        output += $" - {command.Description ?? "No Description"}\n";

        foreach (var subcommand in command.Subcommands)
            output += " " + ProcessCommand(subcommand);

        return output;
    }

    [Command("help"), Description("Lists all commands")]
    public static ValueTask ExecuteAsync(CommandContext context)
    {
        string response = "# Commands:\n";

        foreach (var command in context.Extension.Commands.Values)
            response += ProcessCommand(command);

        return context.RespondAsync(response);
    }
}

public class PingCommand
{
    [Command("ping"), Description("Tests the bot latency")]
    public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"Pong! Latency is {context.Client.Ping}ms.");
}