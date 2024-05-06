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
    public static ValueTask CubeAsync(CommandContext context, [SlashChoiceProvider<BlockProvider>] string block, byte x_size, byte y_size, byte z_size)
    {
        string formatted_block = block.ToUpper().Replace(" ", "");

        if (!BlockProvider.blocks.ContainsKey(formatted_block))
            return context.RespondAsync("Block not found.");

        if ((x_size * y_size * z_size) > 2097152)
            return context.RespondAsync("Requested cube is too large.");

        Stopwatch stopwatch = new();
        stopwatch.Start();

        object block_num = BlockProvider.blocks[formatted_block];

        StringBuilder sb = new()
        {
            Capacity = 10 * x_size * y_size * z_size
        };

        for (var x = 0; x < x_size; x++)
        {
            for (var y = 0; y < y_size; y++)
            {
                for (var z = 0; z < z_size; z++)
                {
                    sb.Append(block_num);
                    sb.Append(",,");
                    sb.Append(x);
                    sb.Append(',');
                    sb.Append(y);
                    sb.Append(',');
                    sb.Append(z);
                    sb.Append(',');
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
}