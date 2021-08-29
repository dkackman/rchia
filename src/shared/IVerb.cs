using System.Threading.Tasks;

namespace chia.dotnet.console
{
    internal interface IVerb
    {
        Task<int> Run();
    }
}
