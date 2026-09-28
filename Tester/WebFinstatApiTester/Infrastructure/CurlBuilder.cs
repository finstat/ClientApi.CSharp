using System.Globalization;
using System.Text;
using System.Text.Json;
using FinstatApi;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Builds the three FinStat-targeted curl commands DesktopFinstatApiTester generates
/// (GET with query string, POST form, POST JSON). One mapping per exposed endpoint:
/// FinStat path + how to build the param list + what string to sign for the hash.
/// </summary>
public static class CurlBuilder
{
    public sealed record Request(string Endpoint, Dictionary<string, JsonElement> Params);
    public sealed record Result(IReadOnlyList<string> Commands);

    private sealed record Mapping(
        string FinstatPath,
        string? Country,                                   // "CZ" selects the CZ base URL
        Action<Dictionary<string, string>, Dictionary<string, JsonElement>> ParamBuilder,
        Func<Dictionary<string, JsonElement>, string?> HashBuilder);

    // Param extractor helpers — tolerate missing/null without crashing.
    private static string Str(Dictionary<string, JsonElement> p, string key) =>
        p.TryGetValue(key, out var v) && v.ValueKind is JsonValueKind.String ? (v.GetString() ?? "") :
        p.TryGetValue(key, out var v2) && v2.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined
            ? v2.ToString() : "";

    private static string Dob(Dictionary<string, JsonElement> p, string key) =>
        DateTime.TryParse(Str(p, key), CultureInfo.InvariantCulture, out var d) ? d.ToString("yyyy-MM-dd") : Str(p, key);

    // ── Mapping table ──────────────────────────────────────────────────────────
    private static readonly Dictionary<string, Mapping> s_map = new()
    {
        // SK Detail
        ["api/sk/detail/basic"]    = new("/basic",    null, (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/sk/detail/detail"]   = new("/detail",   null, (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/sk/detail/extended"] = new("/extended", null, (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/sk/detail/ultimate"] = new("/ultimate", null, (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),

        // SK Auto
        ["api/sk/autocomplete"] = new("/autocomplete", null,
            (d, p) => d["query"] = Str(p, "query"),
            p => Str(p, "query")),
        ["api/sk/autologin"] = new("/autologin", null,
            (d, p) => { d["url"] = Str(p, "url"); if (Str(p, "email") is { Length: > 0 } e) d["email"] = e; },
            p => Str(p, "url")),

        // SK Monitoring — ICO
        ["api/sk/monitoring/categories"]  = new("/MonitoringCategories",  null, (_, _) => {},                          _  => "monitoringcategories"),
        ["api/sk/monitoring/list"]        = new("/MonitoringList",        null, (_, _) => {},                          _  => "list"),
        ["api/sk/monitoring/add"]         = new("/AddToMonitoring",       null, (d, p) => d["ico"] = Str(p, "ico"),    p  => Str(p, "ico")),
        ["api/sk/monitoring/remove"]      = new("/RemoveFromMonitoring",  null, (d, p) => d["ico"] = Str(p, "ico"),    p  => Str(p, "ico")),
        ["api/sk/monitoring/report"]      = new("/MonitoringReport",      null, (_, _) => {},                          _  => "report"),
        ["api/sk/monitoring/proceedings"] = new("/MonitoringProceedings", null, (_, _) => {},                          _  => "proceedings"),
        // SK Monitoring — Date
        ["api/sk/monitoring/date/list"]        = new("/MonitoringDateList",        null, (d, p) => AddCategory(d, p),                            _ => "datelist"),
        ["api/sk/monitoring/date/add"]         = new("/AddDateToMonitoring",       null, (d, p) => { d["date"] = Str(p, "date"); AddCategory(d, p); }, p => Str(p, "date")),
        ["api/sk/monitoring/date/remove"]      = new("/RemoveDateFromMonitoring",  null, (d, p) => { d["date"] = Str(p, "date"); AddCategory(d, p); }, p => Str(p, "date")),
        ["api/sk/monitoring/date/report"]      = new("/MonitoringDateReport",      null, (d, p) => AddCategory(d, p),                            _ => "datereport"),
        ["api/sk/monitoring/date/proceedings"] = new("/MonitoringDateProceedings", null, (_, _) => {},                                           _ => "dateproceedings"),

        // SK Distraint
        ["api/sk/distraint/search"] = new("/distraintSearch", null,
            (d, p) => d["search"] = JoinDistraint(p, "|"),
            p => JoinDistraint(p, "|")),
        ["api/sk/distraint/results"] = new("/distraintResults", null,
            (d, p) => d["search"] = JoinDistraint(p, "|"),
            p => JoinDistraint(p, "|")),
        ["api/sk/distraint/detail"] = new("/distraintDetail", null,
            (d, p) => d["search"] = Str(p, "token") + "," + string.Join(",", IdsAsStrings(p)),
            p => Str(p, "token") + string.Join(string.Empty, IdsAsStrings(p))),
        ["api/sk/distraint/results-by-token"] = new("/distraintResultsByToken", null,
            (d, p) => d["token"] = Str(p, "token"),
            p => Str(p, "token")),
        ["api/sk/distraint/stored-detail"] = new("/distraintStoredDetail", null,
            (d, p) => d["id"] = Str(p, "id"),
            p => Str(p, "id")),

        // SK Daily Diff
        ["api/sk/diff/daily/list"]     = new("/GetListOfDiffs",           null, (_, _) => {},                                _ => null),
        ["api/sk/diff/daily/download"] = new("/GetFile",                  null, (d, p) => d["fileName"] = Str(p, "fileName"), p => Str(p, "fileName")),
        ["api/sk/diff/ultimate/list"]     = new("/GetListOfDailyUltimateDiffs",  null, (_, _) => {},                                _ => null),
        ["api/sk/diff/ultimate/download"] = new("/GetDailyUltimateDiffFile",     null, (d, p) => d["fileName"] = Str(p, "fileName"), p => Str(p, "fileName")),
        ["api/sk/diff/statement/list"]     = new("/GetListOfDailyStatementDiffs", null, (_, _) => {},                                _ => null),
        ["api/sk/diff/statement/download"] = new("/GetDailyStatementDiffFile",    null, (d, p) => d["fileName"] = Str(p, "fileName"), p => Str(p, "fileName")),
        ["api/sk/diff/statement/legend"]   = new("/GetStatementLegend",           null, (d, p) => d["lang"] = Str(p, "lang"),          p => Str(p, "lang")),
        ["api/sk/diff/statement2014/list"]     = new("/GetListOfDailyStatement2014Diffs", null, (_, _) => {},                                _ => null),
        ["api/sk/diff/statement2014/download"] = new("/GetDailyStatement2014DiffFile",    null, (d, p) => d["fileName"] = Str(p, "fileName"), p => Str(p, "fileName")),
        ["api/sk/diff/statement2014/legend"]   = new("/GetStatement2014Legend",           null, (d, p) => d["lang"] = Str(p, "lang"),          p => Str(p, "lang")),

        // SK Bankruptcy
        ["api/sk/bankruptcy/persons"] = new("/PersonBankruptcyProceedings", null,
            (d, p) => {
                d["name"]        = Str(p, "name");
                d["surname"]     = Str(p, "surname");
                d["dateofbirth"] = Dob(p, "dateOfBirth");
            },
            p => $"{Str(p, "name")}|{Str(p, "surname")}|{Dob(p, "dateOfBirth")}"),
        ["api/sk/bankruptcy/company"] = new("/CompanyBankruptcyRestructuring", null,
            (d, p) => { d["ico"] = Str(p, "ico"); d["name"] = Str(p, "name"); },
            p => Str(p, "ico") + Str(p, "name")),

        // SK Statement
        ["api/sk/statement/list"]   = new("/GetStatements", null,
            (d, p) => d["ico"] = Str(p, "ico"),
            p => Str(p, "ico")),
        ["api/sk/statement/detail"] = new("/GetStatementDetail", null,
            (d, p) => { d["ico"] = Str(p, "ico"); d["year"] = Str(p, "year"); d["template"] = Str(p, "template"); },
            p => $"{Str(p, "ico")}|{Str(p, "year")}"),
        ["api/sk/statement/legend"] = new("/GetStatementTemplateLegend", null,
            (d, p) => { d["lang"] = Str(p, "lang"); d["template"] = Str(p, "template"); },
            p => Str(p, "lang")),

        // SK Reporting
        ["api/sk/reporting/topics"]   = new("/GetReportingTopics", null, (_, _) => {}, _ => "reporting-topics"),
        ["api/sk/reporting/list"]     = new("/GetReportingList",   null,
            (d, p) => d["topic"] = Str(p, "topic"),
            p => "reporting-list|" + Str(p, "topic")),
        ["api/sk/reporting/download"] = new("/GetReportingOutput", null,
            (d, p) => d["FileName"] = Str(p, "fileName"),
            p => Str(p, "fileName")),

        // CZ Detail
        ["api/cz/detail/basic"]   = new("/basic",    "CZ", (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/cz/detail/detail"]  = new("/detail",   "CZ", (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/cz/detail/premium"] = new("/premiumcz","CZ", (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/cz/detail/elite"]   = new("/elitecz",  "CZ", (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),

        // CZ Auto
        ["api/cz/autocomplete"] = new("/autocomplete", "CZ", (d, p) => d["query"] = Str(p, "query"), p => Str(p, "query")),
        ["api/cz/autologin"]    = new("/autologin",    "CZ",
            (d, p) => { d["url"] = Str(p, "url"); if (Str(p, "email") is { Length: > 0 } e) d["email"] = e; },
            p => Str(p, "url")),

        // CZ Monitoring
        ["api/cz/monitoring/categories"] = new("/MonitoringCategories", "CZ", (_, _) => {}, _ => "monitoringcategories"),
        ["api/cz/monitoring/list"]       = new("/MonitoringList",       "CZ", (_, _) => {}, _ => "list"),
        ["api/cz/monitoring/add"]        = new("/AddToMonitoring",      "CZ", (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/cz/monitoring/remove"]     = new("/RemoveFromMonitoring", "CZ", (d, p) => d["ico"] = Str(p, "ico"), p => Str(p, "ico")),
        ["api/cz/monitoring/report"]     = new("/MonitoringReport",     "CZ", (_, _) => {}, _ => "report"),
        // CZ Monitoring — Date
        ["api/cz/monitoring/date/list"]   = new("/MonitoringDateList",       "CZ", (d, p) => AddCategory(d, p),                            _ => "datelist"),
        ["api/cz/monitoring/date/add"]    = new("/AddDateToMonitoring",      "CZ", (d, p) => { d["date"] = Str(p, "date"); AddCategory(d, p); }, p => Str(p, "date")),
        ["api/cz/monitoring/date/remove"] = new("/RemoveDateFromMonitoring", "CZ", (d, p) => { d["date"] = Str(p, "date"); AddCategory(d, p); }, p => Str(p, "date")),
        ["api/cz/monitoring/date/report"] = new("/MonitoringDateReport",     "CZ", (d, p) => AddCategory(d, p),                            _ => "datereport"),
    };

    private static void AddCategory(Dictionary<string, string> d, Dictionary<string, JsonElement> p)
    {
        if (Str(p, "category") is { Length: > 0 } cat) d["category"] = cat;
    }

    private static string JoinDistraint(Dictionary<string, JsonElement> p, string sep) => string.Join(sep, new[]
    {
        Str(p, "ico"), Str(p, "surname"), Str(p, "dateOfBirth"),
        Str(p, "city"), Str(p, "companyName"), Str(p, "fileReference"),
    });

    private static IEnumerable<string> IdsAsStrings(Dictionary<string, JsonElement> p)
    {
        if (!p.TryGetValue("ids", out var ids) || ids.ValueKind != JsonValueKind.Array) return Array.Empty<string>();
        return ids.EnumerateArray().Select(e => e.ToString());
    }

    public static Result Build(Request req, FinstatCredentials creds)
    {
        if (!s_map.TryGetValue(req.Endpoint, out var m))
            throw new BadHttpRequestException($"No curl mapping for endpoint '{req.Endpoint}'.");

        var baseUrl = (m.Country == "CZ" ? creds.UrlCz : creds.UrlSk).TrimEnd('/');
        var suffix  = creds.Json ? ".json" : "";
        var hash    = CommonAbstractApiClient.ComputeVerificationHash(creds.ApiKey, creds.PrivateKey, m.HashBuilder(req.Params) ?? string.Empty);

        var urlParams = new Dictionary<string, string>();
        m.ParamBuilder(urlParams, req.Params);
        urlParams["apiKey"]     = creds.ApiKey;
        urlParams["Hash"]       = hash;
        urlParams["StationId"]  = "curl-test";
        urlParams["StationName"]= "curl-test";

        var qs   = string.Join("&", urlParams.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        var form = string.Join("&", urlParams.Select(kv => $"{kv.Key}={kv.Value}"));
        var jsonBody = "{" + string.Join(",", urlParams.Select(kv => $"\"{kv.Key}\":\"{Escape(kv.Value)}\"")) + "}";

        var url = $"{baseUrl}{m.FinstatPath}{suffix}";

        return new Result(new[]
        {
            $"curl '{url}?{qs}' -v",
            $"curl -X POST '{url}' -d '{form}' -v",
            $"curl -H \"Content-Type: application/json\" -X POST '{url}' -d '{jsonBody}' -v",
        });
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>Endpoints we have a curl mapping for — the frontend uses this to hide the button otherwise.</summary>
    public static IEnumerable<string> Supported => s_map.Keys;
}
