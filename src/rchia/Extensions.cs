using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using chia.dotnet;
using Newtonsoft.Json;

namespace rchia;

internal static class Extensions
{
    public static IDictionary<K, V> Sort<K, V>(this IDictionary<K, V> dict) where K : notnull
    {
        return dict.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static IEnumerable<IDictionary<K, V>> SortAll<K, V>(this IEnumerable<IDictionary<K, V>> dict) where K : notnull
    {
        return dict.Select(d => d.Sort());
    }

    public static string FromSnakeCase(this string s)
    {
        return string.IsNullOrEmpty(s)
            ? string.Empty
            : new StringBuilder(s.Length)
            .Append(char.ToUpper(s[0]))
            .Append(s.Replace('_', ' ')[1..])
            .ToString();
    }

    public static string ToJson(this object o)
    {
        return JsonConvert.SerializeObject(o, Formatting.Indented);
    }

    public static string ToJson(this IDictionary<string, object?> o)
    {
        // this is kinda hacky but will remove formatting hints from the json
        return new StringBuilder(JsonConvert.SerializeObject(o, Formatting.Indented))
            .Replace("[/]", string.Empty)
            .Replace("[red]", string.Empty)
            .Replace("[green]", string.Empty)
            .Replace("[grey]", string.Empty)
            .ToString();
    }

    public static string FormatTimeSpan(this TimeSpan t)
    {
        var builder = new StringBuilder();
        if (t.Days > 0)
        {
            _ = builder.Append($"{t.Days} day{(t.Days != 1 ? "s" : "")} ");
        }

        if (t.Hours > 0)
        {
            _ = builder.Append($"{t.Hours} hour{(t.Hours != 1 ? "s" : "")} ");
        }

        if (t.Minutes > 0)
        {
            _ = builder.Append($"{t.Minutes} minute{(t.Minutes != 1 ? "s" : "")} ");
        }

        return builder.ToString();
    }

    public async static Task<string> ValidatePoolingOptions(this WalletProxy wallet, bool pooling, Uri? poolUri, int timeoutMilliseconds)
    {
        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        var (NetworkName, NetworkPrefix) = await wallet.GetNetworkInfo(cts.Token);

        if (pooling && NetworkName == "mainnet" && poolUri is not null && poolUri.Scheme != "https")
        {
            throw new InvalidOperationException($"Pool URLs must be HTTPS on mainnet {poolUri}. Aborting.");
        }

        return $"This operation will join the wallet with fingerprint [wheat1]{wallet.Fingerprint}[/] to [wheat1]{poolUri}[/].\nDo you want to proceed?";
    }

    public async static Task<PoolInfo> GetPoolInfo(this Uri uri, int timeoutMilliseconds)
    {
        using var cts = new CancellationTokenSource(timeoutMilliseconds);
        var info = await WalletProxy.GetPoolInfo(uri, cts.Token);

        if (info.RelativeLockHeight > 1000)
        {
            throw new InvalidOperationException("Relative lock height too high for this pool, cannot join");
        }

        if (info.ProtocolVersion != PoolInfo.POOL_PROTOCOL_VERSION)
        {
            throw new InvalidOperationException($"Unsupported version: {info.ProtocolVersion}, should be {PoolInfo.POOL_PROTOCOL_VERSION}");
        }

        return info;
    }
}
