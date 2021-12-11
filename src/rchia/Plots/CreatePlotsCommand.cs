using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;

namespace rchia.Plots;

internal sealed class CreatePlotsCommand : EndpointOptions
{
    [Option("k", "size", Default = KValues.K32, Description = "Plot szie")]
    public KValues Size { get; init; } = KValues.K32;

    [Option("o", "override-k", Default = false, Description = "Force size smaller than 32")]
    public bool OverrideK { get; init; }

    [Option("n", "num", Default = 1, Description = "Number of plots or challenges")]
    public int Num { get; init; } = 1;

    [Option("b", "buffer", Default = 3389, Description = "Megabytes for sort/plot buffer")]
    public int Buffer { get; init; } = 3389;

    [Option("r", "num-threads", Default = 2, Description = "Number of threads to use")]
    public int NumThreads { get; init; } = 2;

    [Option("u", "buckets", Default = 128, Description = "Number of buckets")]
    public int Buckets { get; init; } = 128;

    [Option("a", "alt-fingerprint", Description = "Enter the alternative fingerprint of the key you want to use")]
    public uint? AltFingerprint { get; init; }

    [Option("c", "pool-contract-address", Description = "Address of where the pool reward will be sent to.Only used if\nalt_fingerprint and pool public key are not set")]
    public string? PoolContractAddress { get; init; }

    [Option("f", "farmer-public-key", Description = "Hex farmer public key")]
    public string? FarmerPublicKey { get; init; }

    [Option("p", "pool-public-key", Description = "Hex public key of pool")]
    public string? PoolPublicKey { get; init; }

    [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
    public string FinalDir { get; init; } = ".";

    [Option("t", "tmp-dir", Default = ".", Description = "Temporary directory for plotting files")]
    public string TmpDir { get; init; } = ".";

    [Option("2", "tmp2-dir", Description = "Second temporary directory for plotting files")]
    public string? Tmp2Dir { get; init; }

    [Option("e", "nobitfield", Default = false, Description = "Disable bitfield")]
    public bool NoBitField { get; init; }

    [Option("x", "exclude-final-dir", Default = false, Description = "Skips adding [final-dir] to harvester for farming")]
    public bool ExcludeFinalDir { get; init; }

    [CommandTarget]
    public async Task<int> Run()
    {
        return await DoWorkAsync("Adding request to the plot queue...", async output =>
        {
            using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(output, this);
            var proxy = new PlotterProxy(rpcClient, ClientFactory.Factory.OriginService);

            using var cts = new CancellationTokenSource(TimeoutMilliseconds);
            _ = await proxy.RegisterPlotter(cts.Token);
            var ids = await proxy.StartPlotting(CreateConfig(), cts.Token);

            output.WriteOutput(ids);

            output.WriteLine($"Plot{(ids.Count() == 1 ? string.Empty : "s")} queued:");
            output.Helpful("Run '[grey]rchia plots queue -v[/]' or '[grey]rchia plots log[/]' to check status", true);
        });
    }

    private PlotterConfig CreateConfig()
    {
        return new PlotterConfig()
        {
            Size = Size,
            OverrideK = OverrideK,
            Number = Num,
            Buffer = Buffer,
            NumThreads = NumThreads,
            Buckets = Buckets,
            AltFingerprint = AltFingerprint,
            PoolContractAddress = PoolContractAddress,
            FarmerPublicKey = FarmerPublicKey,
            PoolPublicKey = PoolPublicKey,
            DestinationDir = FinalDir,
            TempDir = TmpDir,
            TempDir2 = Tmp2Dir,
            NoBitField = NoBitField,
            ExcludeFinalDir = ExcludeFinalDir
        };
    }
}