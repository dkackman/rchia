using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Spectre.Console;

namespace rchia.Commands;

public abstract class Command
{
    [Option("v", "verbose", Description = "Set output to verbose messages")]
    public bool Verbose { get; set; }

    [Option("", "json", Description = "Set this flag to output json", IsHidden = true)]
    public bool Json { get; init; }

    protected async Task<int> DoWorkAsync(string msg, Func<ICommandOutput, Task> work)
    {
        Debug.Assert(!string.IsNullOrEmpty(msg));
        var output = CreateCommandOutput();

        try
        {
            if (!Json)
            {
                await AnsiConsole.Status().StartAsync(msg, async ctx => await work(output.SetContext(ctx)));
                output.Helpful("Done.");
            }
            else
            {
                await work(output);
            }

            return 0;
        }
        catch (TaskCanceledException)
        {
            output.MarkupLine($"[red]The operation timed out[/]");
            output.Helpful("Check that the chia service is running and available. You can extend the timeout period by using the '-to' option.");

            return -1;
        }
        catch (Exception e)
        {
            output.Message(e);

            return -1;
        }
    }

    protected int DoWork(string msg, Action<ICommandOutput> work)
    {
        var output = CreateCommandOutput();

        try
        {
            if (!Json)
            {
                AnsiConsole.Status().Start(msg, ctx => work(output.SetContext(ctx)));
                output.Helpful("Done.");
            }
            else
            {
                work(output);
            }

            return 0;
        }
        catch (Exception e)
        {
            output.Message(e);

            return -1;
        }
    }

    private ICommandOutput CreateCommandOutput()
    {
        if (Json)
        {
            return new JsonOutput()
            {
                Verbose = Verbose
            };
        }

        return new ConsoleOutput()
        {
            Verbose = Verbose
        };
    }
}
