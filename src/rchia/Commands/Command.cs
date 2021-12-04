using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using Spectre.Console;

namespace rchia.Commands
{
    public abstract class Command : ICommandOuptut
    {
        private ICommandOuptut _commandOuptut = new ConsoleOutput();

        protected void SetJsonOutput()
        {
            _commandOuptut = new JsonOutput()
            {
                Verbose = Verbose
            };
        }

        [Option("v", "verbose", Description = "Set output to verbose messages")]
        public bool Verbose { get { return _commandOuptut.Verbose; } set { _commandOuptut.Verbose = value; } }

        public bool IsInteractive => _commandOuptut.IsInteractive;

        public IStatus CreateStatus(StatusContext? ctx)
        {
            return _commandOuptut.CreateStatus(ctx);
        }

        public void WriteOutput(IEnumerable<string> output)
        {
            _commandOuptut.WriteOutput(output);
        }

        public void MarkupLine(string msg)
        {
            _commandOuptut.MarkupLine(msg);
        }

        public void WriteLine(string msg)
        {
            _commandOuptut.WriteLine(msg);
        }

        public void Message(string msg, bool important = false)
        {
            _commandOuptut.Message(msg, important);
        }

        public void NameValue(string name, object? value)
        {
            _commandOuptut.NameValue(name, value);
        }

        public void Helpful(string msg, bool important = false)
        {
            _commandOuptut.Helpful(msg, important);
        }

        public void Warning(string msg)
        {
            _commandOuptut.Warning(msg);
        }

        public void Message(Exception e)
        {
            _commandOuptut.Message(e);
        }

        public bool Confirm(string warning, bool force)
        {
            return _commandOuptut.Confirm(warning, force);
        }

        protected async Task<int> DoWorkAsync(string msg, Func<IStatus, Task> work)
        {
            Debug.Assert(!string.IsNullOrEmpty(msg));

            try
            {
                if (IsInteractive)
                {
                    await AnsiConsole.Status().StartAsync(msg, async ctx => await work(_commandOuptut.CreateStatus(ctx)));
                    Helpful("Done.");

                }
                else
                {
                    await work(_commandOuptut.CreateStatus(null));
                }

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

        protected int DoWork(string msg, Action<IStatus> work)
        {
            try
            {
                if (IsInteractive)
                {
                    AnsiConsole.Status().Start(msg, ctx => work(_commandOuptut.CreateStatus(ctx)));
                    Helpful("Done.");
                }
                else
                {
                    work(_commandOuptut.CreateStatus(null));
                }

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
