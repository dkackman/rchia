using System;

namespace rchia;

public sealed class StatusMessage : IDisposable
{
    private readonly IStatus _status;
    private readonly string _originalMessage;

    public StatusMessage(IStatus status, string msg)
    {
        _status = status ?? throw new ArgumentNullException(nameof(status));
        if (string.IsNullOrEmpty(msg))
        {
            throw new ArgumentException($"{nameof(msg)} cannot be null or empty");
        }

        _originalMessage = _status.Status;
        _status.Status = msg;
    }

    public void Dispose()
    {
        _status.Status = _originalMessage;
        GC.SuppressFinalize(this);
    }
}
