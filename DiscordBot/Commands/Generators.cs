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
    [Command("cube"), Description("Generates a cube (first property is `hollow: 0 or 1`)")]
    public static ValueTask CubeAsync(CommandContext context, [SlashChoiceProvider<BlockProvider>] int block, int x_size, int y_size, int z_size, params byte[] properties)
    {
        if (block < 0 || block >= BlockProvider.blocks.Count)
            return context.RespondAsync("Invalid block.");

        if (x_size < 1 || y_size < 1 || z_size < 1)
            return context.RespondAsync("**A 3D object cannot have side that's less than 1...**");
        if (properties[0] > 1 || properties[0] < 0)
            return context.RespondAsync("The first property should be a 1 or a 0 specifing if it should be hollow.");

        bool hollow = properties[0] == 1;

        Stopwatch stopwatch = new();
        stopwatch.Start();

        StringBuilder sb = new();

        string property_string = properties.Length > 1 ? string.Join("+", properties[1..]) : "";

        for (var x = 0; x < x_size; x++)
        {
            for (var y = 0; y < y_size; y++)
            {
                for (var z = 0; z < z_size; z++)
                {
                    if (hollow && !(
                        x == 0 || x == x_size - 1 ||
                        y == 0 || y == y_size - 1 ||
                        z == 0 || z == z_size - 1
                        ))
                        continue;

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

        string built_string = sb.ToString();

        string message_content = "";
        if (built_string.Length > 500000)
            message_content += "### File is too big for circuit maker 2!\n";

        if (built_string.Length > 5 * 1024 * 1024)
            return context.RespondAsync("Created file cannot be above 5MB.");

        var message_builder = new DiscordMessageBuilder();
        message_builder.AddFile($"Cube{block}-{x_size}-{y_size}-{z_size}.txt", new MemoryStream(Encoding.UTF8.GetBytes(built_string)));

        message_builder.Content = $"{message_content}Took {stopwatch.ElapsedMilliseconds}ms";

        return context.RespondAsync(message_builder);
    }
}