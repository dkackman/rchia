using System;
using System.Collections.Generic;

using rchia.Commands;

namespace rchia.Bech32
{
    internal sealed class HashFromAddressCommand : Command
    {
        [Argument(0, Name = "address", Description = "The address to convert")]
        public string Address { get; init; } = string.Empty;

        [CommandTarget]
        public int Run()
        {
            return DoWork("Calculating hash...", output =>
            {
                if (string.IsNullOrEmpty(Address))
                {
                    throw new InvalidOperationException("An address must be provided");
                }

                var result = new Dictionary<string, string>()
                {
                    { "hash", Bech32M.AddressToPuzzleHash(Address).ToString() }
                };

                output.WriteOutput(result);
            });
        }
    }
}
