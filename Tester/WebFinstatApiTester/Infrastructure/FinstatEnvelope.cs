using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Envelope returned by every FinStat-backed endpoint.
/// <para>
/// <see cref="Result"/> is the typed C# object serialised as JSON (for the tree/history viewer).
/// <see cref="RawBody"/> is the response body exactly as FinStat sent it (JSON or XML, per
/// <c>X-Finstat-ResponseType</c>), captured from the client's <c>OnResponseContent</c>, so it
/// also shows fields the client model does not know yet. Only when no body was captured is it
/// the typed object re-serialised in the same format.
/// </para>
/// <para>
/// <see cref="Result"/> is typed as <c>object</c> so the ASP.NET JSON serialiser emits the
/// runtime-type members (handles abstract/interface return types like
/// <c>AbstractStatementLegendResult</c>).
/// </para>
/// <para>
/// Null members are left out of the Result tree and of a re-serialised RawBody. One client
/// model serves several products (EliteCZ deserialises into the classes UltimateCZ fills), so
/// the fields a product does not return come back null — written out they would show up as if FinStat had sent them.
/// XML already omits them the same way.
/// </para>
/// </summary>
public sealed record FinstatEnvelope(object? Result, string? RawBody, string RawContentType);

public static class FinstatEnvelopeBuilder
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // the casing ASP.NET used when it serialised Result itself; the frontend reads these keys
    private static readonly JsonSerializerOptions s_treeOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private const string RawBodyKey = "FinstatRawBody";

    private static IHttpContextAccessor? s_accessor;

    public static void UseHttpContext(IHttpContextAccessor accessor) => s_accessor = accessor;

    // one FinStat call per request; a second call would overwrite the first body
    public static void CaptureRawBody(HttpContext? context, byte[] content)
    {
        if (context is not null && content is not null)
        {
            context.Items[RawBodyKey] = content;
        }
    }

    public static FinstatEnvelope Build<T>(T? value, bool json)
    {
        var contentType = json ? "application/json" : "application/xml";
        var raw = RawBody();
        if (value is null)
        {
            return new FinstatEnvelope(null, raw, contentType);
        }

        // Use the runtime type for both serialisers so abstract / interface return types
        // emit their concrete-subclass members.
        var runtimeType = value.GetType();

        if (raw is not null)
        {
            return new FinstatEnvelope(Tree(value, runtimeType), raw, contentType);
        }

        if (json)
        {
            var body = JsonSerializer.Serialize(value, runtimeType, s_jsonOptions);
            return new FinstatEnvelope(Tree(value, runtimeType), body, contentType);
        }

        using var sw = new StringWriter();
        using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false }))
        {
            new XmlSerializer(runtimeType).Serialize(xw, value);
        }
        return new FinstatEnvelope(Tree(value, runtimeType), sw.ToString(), contentType);
    }

    private static string? RawBody()
    {
        if (s_accessor?.HttpContext?.Items[RawBodyKey] is not byte[] bytes)
        {
            return null;
        }
        return Encoding.UTF8.GetString(bytes).TrimStart((char)0xFEFF);
    }

    // pre-serialised so the null filter applies to the tree only, not to the envelope's own null members
    private static JsonElement Tree(object value, Type runtimeType) =>
        JsonSerializer.SerializeToElement(value, runtimeType, s_treeOptions);
}
