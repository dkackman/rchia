using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using chia.dotnet;

namespace rchia.Farm
{
    internal class FarmTasks
    {
        public FarmTasks(FarmerProxy farmer, bool verbose)
        {
            Farmer = farmer;
            Verbose = verbose;
        }

        public FarmerProxy Farmer { get; init; }

        public bool Verbose { get; init; }

        public async Task Services()
        {

        }
    }
}
