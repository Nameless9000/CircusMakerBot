using DSharpPlus.Commands;
using System.ComponentModel;

[Command("math"), Description("Group of math related commands")]
public class MathCommands
{
    [Command("add"), Description("Add two numbers")]
    public static ValueTask AddAsync(CommandContext context, int a, int b) => context.RespondAsync($"{a} + {b} = {a + b}");

    [Command("sub"), Description("Subtract two numbers")]
    public static ValueTask SubtractAsync(CommandContext context, int a, int b) => context.RespondAsync($"{a} - {b} = {a - b}");
}