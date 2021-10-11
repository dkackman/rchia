using System;
using Spectre.Console;

namespace rchia
{
    public sealed class StatusMessage : IDisposable
    {
        private readonly StatusContext _ctx;
        private readonly string _originalMessage;

        public StatusMessage(StatusContext ctx, string msg)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException($"{nameof(msg)} cannot be null or empty");
            }

            _originalMessage = _ctx.Status;
            _ctx.Status = msg;
        }

        public void Dispose()
        {
            _ctx.Status = _originalMessage;
            GC.SuppressFinalize(this);
        }
    }
}
