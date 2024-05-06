using DSharpPlus.Commands;
using System.ComponentModel;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using System.Text;
using System.Diagnostics;

namespace Generator;

public class BlockProvider : IChoiceProvider
{
    public static readonly IReadOnlyDictionary<string, object> blocks = new Dictionary<string, object>
    {
        ["NOR"] = 0,
        ["AND"] = 1,
        ["OR"] = 2,
        ["XOR"] = 3,
        ["BUTTON"] = 4,
        ["FLIPFLOP"] = 5,
        ["LED"] = 6,
        ["SOUND"] = 7,
        ["CONDUCTOR"] = 8,
        ["CUSTOM"] = 9,
        ["NAND"] = 10,
        ["XNOR"] = 11,
        ["RANDOM"] = 12,
        ["TEXT"] = 13,
        ["TILE"] = 14,
        ["NODE"] = 15,
        ["DELAY"] = 16,
        ["ANTENNA"] = 17,
        ["CONDUCTORV2"] = 18
    };

    public ValueTask<IReadOnlyDictionary<string, object>> ProvideAsync(CommandParameter parameter) => ValueTask.FromResult(blocks);
}

[Command("generate"), Description("Group of generator related commands")]
public class GeneratorCommands
{
    [Command("cube"), Description("Generates a cube")]
    public static ValueTask CubeAsync(CommandContext context, [SlashChoiceProvider<BlockProvider>] int block, int x_size, int y_size, int z_size, params byte[] properties)
    {
        if (block < 0 || block >= BlockProvider.blocks.Count)
            return context.RespondAsync("Invalid block.");

        if (x_size < 1 || y_size < 1 || z_size < 1)
            return context.RespondAsync("**A 3D object cannot have side that's less than 1...**");

        if ((x_size * y_size * z_size) > 2097152)
            return context.RespondAsync("Requested cube is too large.");

        Stopwatch stopwatch = new();
        stopwatch.Start();

        StringBuilder sb = new()
        {
            Capacity = 10 * x_size * y_size * z_size
        };

        string property_string = string.Join("+", properties);

        for (var x = 0; x < x_size; x++)
        {
            for (var y = 0; y < y_size; y++)
            {
                for (var z = 0; z < z_size; z++)
                {
                    sb.Append(block);
                    sb.Append(",,");
                    sb.Append(x);
                    sb.Append(',');
                    sb.Append(y);
                    sb.Append(',');
                    sb.Append(z);
                    sb.Append(',');
                    sb.Append(property_string);
                    sb.Append(';');
                }
            }
        }

        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1);
            sb.Append('?');
        }

        var message_builder = new DiscordMessageBuilder();
        message_builder.AddFile($"Cube{block}-{x_size}-{y_size}-{z_size}.txt", new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())));

        stopwatch.Stop();

        message_builder.Content = $"Took {stopwatch.ElapsedMilliseconds}ms";

        return context.RespondAsync(message_builder);
    }

    [Command("hollow-cube"), Description("Generates a hollow cube")]
    public static ValueTask HCubeAsync(CommandContext context, [SlashChoiceProvider<BlockProvider>] int block, int x_size, int y_size, int z_size, params byte[] properties)
    {
        if (block < 0 || block >= BlockProvider.blocks.Count)
            return context.RespondAsync("Invalid block.");

        if (x_size < 1 || y_size < 1 || z_size < 1)
            return context.RespondAsync("**A 3D object cannot have side that's less than 1...**");

        var total_blocks = (4 + x_size) + (4 * y_size) + (4 * z_size) - 8;

        if (total_blocks > 6136)
            return context.RespondAsync("Requested cube is too large.");

        Stopwatch stopwatch = new();
        stopwatch.Start();

        StringBuilder sb = new()
        {
            Capacity = 10 * total_blocks
        };

        string property_string = string.Join("+", properties);

        for (var x = 0; x < x_size; x++)
        {
            for (var y = 0; y < y_size; y++)
            {
                for (var z = 0; z < z_size; z++)
                {
                    if (x == 0 || x == x_size - 1 ||
                        y == 0 || y == y_size - 1 ||
                        z == 0 || z == z_size - 1) {
                        sb.Append(block);
                        sb.Append(",,");
                        sb.Append(x);
                        sb.Append(',');
                        sb.Append(y);
                        sb.Append(',');
                        sb.Append(z);
                        sb.Append(',');
                        sb.Append(property_string);
                        sb.Append(';');
                    }
                }
            }
        }

        if (sb.Length > 0)
        {
            sb.Remove(sb.Length - 1, 1);
            sb.Append('?');
        }

        var message_builder = new DiscordMessageBuilder();
        message_builder.AddFile($"Cube{block}-{x_size}-{y_size}-{z_size}.txt", new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString())));

        stopwatch.Stop();

        message_builder.Content = $"Took {stopwatch.ElapsedMilliseconds}ms";

        return context.RespondAsync(message_builder);
    }
}