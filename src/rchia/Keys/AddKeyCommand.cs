using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys;

internal sealed class AddKeyCommand : EndpointOptions
{
    [Argument(0, Name = "mnemonic", Description = "The 24 word mnemonic key phrase")]
    public List<string> Mnemonic { get; init; } = new List<string>();

    [Option("f", "filename", Description = "A filename containing the secret key mnemonic to add")]
    public FileInfo? Filename { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Adding key...", async output =>
        {
            var mnemonic = Mnemonic;

            if (Filename is not null)
            {
                using var reader = Filename.OpenText();
                var contents = reader.ReadToEnd();
                contents = contents.Replace('\n', ' '); // this way we can have all words on one line or line per each 
                mnemonic = contents.Split(' ').ToList();
            }

            if (mnemonic.Count != 24)
            {
                throw new InvalidOperationException("Exactly 24 words are required in the mnenomic passphrase");
            }

            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.Wallet);
            var proxy = new WalletProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var fingerprint = await proxy.AddKey(mnemonic, true, cts.Token);

            output.WriteOutput("fingerprint", new Formattable<uint>(fingerprint, fp => $"{fp}"), Verbose);
        });
    }
}
