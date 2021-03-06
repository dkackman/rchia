using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Blocks;

internal sealed class RecentBlocksCommand : EndpointOptions
{
    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Retrieving recent blocks...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateRpcClient(output, this, ServiceNames.FullNode);
            var proxy = new FullNodeProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            var state = await proxy.GetBlockchainState(cts.Token);

            var blocks = new List<BlockRecord>();
            if (state.Peak is not null)
            {
                var block = await proxy.GetBlockRecord(state.Peak.HeaderHash, cts.Token);

                while (block is not null && blocks.Count < 10 && block.Height > 0)
                {
                    blocks.Add(block);
                    using var cts1 = new CancellationTokenSource(TimeoutMilliseconds);
                    block = await proxy.GetBlockRecord(block.PrevHash, cts.Token);
                }
            }

            if (Json)
            {
                output.WriteOutput(blocks);
            }
            else
            {
                var table = from b in blocks
                            select new Dictionary<string, object?>()
                            {
                                { "height", b.Height },
                                { "hash", b.HeaderHash.Replace("0x", "") }
                            };

                output.WriteOutput(table);
            }
        });
    }
}
