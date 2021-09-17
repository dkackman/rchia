using Newtonsoft.Json;

namespace rchia
{
    internal static class Extensions
    {
        public static string ToJson(this object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.Indented);
        }
    }
}
