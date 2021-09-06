using System;
using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia.Plots
{
    internal sealed class CreatePlotsCommand : SharedOptions
    {
        [Option("k", "size", Default = KValues.K32, Description = "Plot szie")]
        public KValues Size { get; set; } = KValues.K32;

        [Option("o", "override-k", Default = false, Description = "Force size smaller than 32")]
        public bool OverrideK { get; set; }

        [Option("n", "num", Default = 1, Description = "Number of plots or challenges")]
        public int Num { get; set; } = 1;

        [Option("b", "buffer", Default = 3389, Description = "Megabytes for sort/plot buffer")]
        public int Buffer { get; set; } = 3389;

        [Option("r", "num-threads", Default = 2, Description = "Number of threads to use")]
        public int NumThreads { get; set; } = 2;

        [Option("u", "buckets", Default = 128, Description = "Number of buckets")]
        public int Buckets { get; set; } = 128;

        [Option("a", "alt-fingerprint", Description = "Enter the alternative fingerprint of the key you want to use")]
        public uint? AltFingerprint { get; set; }

        [Option("c", "pool-contract-address", Description = "Address of where the pool reward will be sent to.Only used if\nalt_fingerprint and pool public key are not set")]
        public string? PoolContractAddress { get; set; }

        [Option("f", "farmer-public-key", Description = "Hex farmer public key")]
        public string? FarmerPublicKey { get; set; }

        [Option("p", "pool-public-key", Description = "Hex public key of pool")]
        public string? PoolPublicKey { get; set; }

        [Option("d", "final-dir", Default = ".", Description = "Final directory for plots (relative or absolute)")]
        public string FinalDir { get; set; } = ".";

        [Option("t", "tmp-dir", Default = ".", Description = "Temporary directory for plotting files")]
        public string TmpDir { get; set; } = ".";

        [Option("2", "tmp2-dir", Description = "Second temporary directory for plotting files")]
        public string? Tmp2Dir { get; set; }

        [Option("e", "nobitfield", Default = false, Description = "Disable bitfield")]
        public bool NoBitField { get; set; }

        [Option("x", "exclude-final-dir", Default = false, Description = "Skips adding [final dir] to harvester for farming")]
        public bool ExcludeFinalDir { get; set; }

        [CommandTarget]
        public override async Task<int> Run()
        {
            try
            {
                using var rpcClient = await ClientFactory.Factory.CreateWebSocketClient(this, ServiceNames.Farmer);
                var harvester = new HarvesterProxy(rpcClient, ClientFactory.Factory.OriginService);
                var tasks = new PlotsTasks(harvester, this);

                await tasks.CreatePlots(CreateConfig());

                return 0;
            }
            catch (Exception e)
            {
                Message(e);

                return -1;
            }
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
}
