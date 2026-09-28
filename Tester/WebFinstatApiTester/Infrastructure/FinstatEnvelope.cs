using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace WebFinstatApiTester.Infrastructure;

/// <summary>
/// Envelope returned by every FinStat-backed endpoint.
/// <para>
/// <see cref="Result"/> is the typed C# object serialised as JSON (for the tree/history viewer).
/// <see cref="RawBody"/> is the same object re-serialised in the user's chosen wire format
/// (JSON or XML, per <c>X-Finstat-ResponseType</c>) so the "Raw" tabs show a faithful
/// shape preview — matches DesktopFinstatApiTester's OutputWindow.
/// </para>
/// <para>
/// <see cref="Result"/> is typed as <c>object</c> so the ASP.NET JSON serialiser emits the
/// runtime-type members (handles abstract/interface return types like
/// <c>AbstractStatementLegendResult</c>).
/// </para>
/// <para>
/// NOTE — the RawBody is our re-serialisation, not FinStat's literal bytes. Element names
/// and shape match but whitespace / namespace decorations may differ slightly.
/// </para>
/// </summary>
public sealed record FinstatEnvelope(object? Result, string? RawBody, string RawContentType);

public static class FinstatEnvelopeBuilder
{
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public static FinstatEnvelope Build<T>(T? value, bool json)
    {
        if (value is null)
        {
            return new FinstatEnvelope(null, null, json ? "application/json" : "application/xml");
        }

        // Use the runtime type for both serialisers so abstract / interface return types
        // emit their concrete-subclass members.
        var runtimeType = value.GetType();

        if (json)
        {
            var body = JsonSerializer.Serialize(value, runtimeType, s_jsonOptions);
            return new FinstatEnvelope(value, body, "application/json");
        }

        using var sw = new StringWriter();
        using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false }))
        {
            new XmlSerializer(runtimeType).Serialize(xw, value);
        }
        return new FinstatEnvelope(value, sw.ToString(), "application/xml");
    }
}
