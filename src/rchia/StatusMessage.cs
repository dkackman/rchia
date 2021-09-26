using System;
using Spectre.Console;

namespace rchia
{
    internal class StatusMessage : IDisposable
    {
        private readonly StatusContext _ctx;
        private readonly string _originalMessage = string.Empty;

        public StatusMessage(StatusContext ctx, string msg)
        {
            _ctx = ctx;
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
