using System.Threading.Tasks;
using chia.dotnet;
using rchia.Commands;
using rchia.Endpoints;

namespace rchia
{
    internal abstract class WalletCommand : SharedOptions
    {
        [Option("fp", "fingerprint", Description = "Set the fingerprint to specify which wallet to use")]
        public uint? Fingerprint { get; set; }

        [Option("i", "id", Default = 1, Description = "Id of the wallet to use")]
        public uint Id { get; set; } = 1;

        protected async Task<uint> GetWalletId(WalletProxy wallet)
        {
            if (Fingerprint.HasValue)
            {
                return await wallet.GetWalletId(Fingerprint.Value);
            }

            return Id;
        }
    }
}