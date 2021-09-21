﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Keys
{
    internal sealed class AddKeyCommand : EndpointOptions
    {
        [Argument(0, Name = "mnemonic", Description = "The 24 word mnemonic key phrase")]
        public List<string> Mnemonic { get; init; } = new List<string>();

        [Option("f", "filename", Description = "A filename containing the secret key mnemonic to add")]
        public FileInfo? Filename { get; init; }

        [CommandTarget]
        public async override Task<int> Run()
        {
            return await Execute(async () =>
            {
                var mnemonic = Mnemonic;

                if (Filename is not null)
                {
                    using var reader = Filename.OpenText();
                    var contents = reader.ReadToEnd();
                    contents = contents.Replace('\n', ' '); // this way we can have all words on one line or line per each 
                    mnemonic = contents.Split(' ').ToList();
                }

                if (Mnemonic.Count != 24)
                {
                    throw new InvalidOperationException("Exactly 24 words are required in the mnenomic passphrase");
                }

                using var tasks = await CreateTasks<KeysTasks, WalletProxy>(ServiceNames.Wallet);

                await tasks.Add(Mnemonic);
            });
        }
    }
}
