using DSharpPlus.Commands;
using System.ComponentModel;

namespace Math;

[Command("math"), Description("Group of math related commands")]
public class MathCommands
{
    [Command("add"), Description("Add two numbers")]
    public static ValueTask AddAsync(CommandContext context, double a, double b) => context.RespondAsync($"{a} + {b} = {a + b}");

    [Command("sub"), Description("Subtract two numbers")]
    public static ValueTask SubtractAsync(CommandContext context, double a, double b) => context.RespondAsync($"{a} - {b} = {a - b}");

    [Command("eval"), Description("Evaluates an expression")]
    public static ValueTask EvalAsync(CommandContext context, params string[] input)
    { 
        try
        {
            var processed_input = string.Join(" ", input)
                .Trim()
                .Replace(" ", "");

            var tokens = Calculator.tokenize(processed_input);
            if (tokens.Length == 0)
                return context.RespondAsync("Error: No tokens found");

            var ast = Calculator.parse_expr(tokens);

            var result = Calculator.eval(ast);

            return context.RespondAsync("Result: " + result.ToString());
        } catch (Exception ex)
        {
            return context.RespondAsync("Error: " + ex.Message);
        }
    }
}