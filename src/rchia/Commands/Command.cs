using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Spectre.Console;

namespace rchia.Commands
{
    public abstract class Command
    {
        [Option("v", "verbose", Description = "Set output to verbose messages")]
        public bool Verbose { get; init; }

        public void MarkupLine(string msg)
        {
            AnsiConsole.MarkupLine(msg);
        }

        public void WriteLine(string msg)
        {
            AnsiConsole.WriteLine(msg);
        }

        public void Message(string msg, bool important = false)
        {
            if (important)
            {
                MarkupLine($"[yellow]{msg}[/]");
            }
            else if (Verbose)
            {
                MarkupLine(msg);
            }

            Debug.WriteLine(msg);
        }

        public void NameValue(string name, object? value)
        {
            MarkupLine($"[wheat1]{name}:[/] {value}");
        }

        public void Helpful(string msg, bool important = false)
        {
            if (Verbose || important)
            {
                MarkupLine($"[{(important ? "lime" : "grey")}]{msg}[/]");
            }
        }

        public void Warning(string msg)
        {
            MarkupLine($"[yellow]{msg}[/]");
        }

        public void Message(Exception e)
        {
            if (Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
            }
            else
            {
                MarkupLine($"[red]{e.Message}[/]");
            }
        }

        public bool Confirm(string warning, bool force)
        {
            if (!force)
            {
                if (!AnsiConsole.Confirm(warning, false))
                {
                    Message("Cancelled");
                    return false;
                }

                Message("Confirmed");
            }

            return true;
        }

        protected async Task<int> DoWorkAsync(string msg, Func<StatusContext, Task> work)
        {
            Debug.Assert(!string.IsNullOrEmpty(msg));

            try
            {
                await AnsiConsole.Status().StartAsync(msg, async ctx => await work(ctx));

                Helpful("Done.");

                return 0;
            }
            catch (TaskCanceledException)
            {
                MarkupLine($"[red]The operation timed out[/]");
                Helpful("Check that the chia service is running and available. You can extend the timeout period by using the '-to' option.");

                return -1;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }

        protected int DoWork(Action work)
        {
            try
            {
                work();

                Helpful("Done.");

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }
    }
}
