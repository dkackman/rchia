using System.Collections.Generic;

namespace rchia.Services
{
    internal static class ServiceGroups
    {
        static ServiceGroups()
        {
            Groups = new Dictionary<string, IEnumerable<string>>()
            {

                { "all",  "chia_harvester chia_timelord_launcher chia_timelord chia_farmer chia_full_node chia_wallet".Split(' ')},
                { "node", "chia_full_node".Split(' ')},
                { "harvester", "chia_harvester".Split(' ')},
                { "farmer", "chia_harvester chia_farmer chia_full_node chia_wallet".Split(' ')},
                { "farmer-no-wallet", "chia_harvester chia_farmer chia_full_node".Split(' ')},
                { "farmer-only", "chia_farmer".Split(' ')},
                { "timelord", "chia_timelord_launcher chia_timelord chia_full_node".Split(' ')},
                { "timelord-only", "chia_timelord".Split(' ')},
                { "timelord-launcher-only", "chia_timelord_launcher".Split(' ')},
                { "wallet", "chia_wallet chia_full_node".Split(' ')},
                { "wallet-only", "chia_wallet".Split(' ')},
                { "introducer", "chia_introducer".Split(' ')},
                { "simulator", "chia_full_node_simulator".Split(' ')}
            };
        }

        public static IReadOnlyDictionary<string, IEnumerable<string>> Groups { get; private set; }
    }
}
