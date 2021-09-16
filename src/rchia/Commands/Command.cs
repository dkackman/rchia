using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Spectre.Console;

namespace rchia.Commands
{
    public abstract class Command : IConsoleMessage
    {
        [Option("v", "verbose", Description = "Set output to verbose messages")]
        public bool Verbose { get; set; }

        public void WriteLine(string msg)
        {
            AnsiConsole.MarkupLine(msg);
        }

        public void Message(string msg, bool important = false)
        {
            if (important)
            {
                AnsiConsole.MarkupLine($"[bold]{msg}[/]");
            }
            else if (Verbose)
            {
                AnsiConsole.MarkupLine(msg);
            }

            Debug.WriteLine(msg);
        }

        public void Message(Exception e)
        {
            if (Verbose)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]{e.Message}[/]");
            }
        }

        public async Task<T> Status<T>(string msg, Func<StatusContext, Task<T>> func)
        {
            return await AnsiConsole.Status().StartAsync(msg, func);
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

        public abstract Task<int> Run();

        protected async Task DoWork(string msg, Func<StatusContext, Task> work)
        {
            await AnsiConsole.Status().StartAsync(msg, async ctx => await work(ctx));
            if (Verbose)
            {
                Helpful("Done.");
            }
        }

        protected async Task<int> Execute(Func<Task> run)
        {
            try
            {
                await run();

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
        }

        public void NameValue(string name, object? value)
        {
            AnsiConsole.MarkupLine($"[bold]{name}:[/] {value}");
        }

        public void Helpful(string msg, bool important = false)
        {
            if (Verbose || important)
            {
                AnsiConsole.MarkupLine($"[grey]{msg}[/]");
            }
        }

        public void Warning(string msg)
        {
            AnsiConsole.MarkupLine($"[yellow]{msg}[/]");
        }
    }
}
