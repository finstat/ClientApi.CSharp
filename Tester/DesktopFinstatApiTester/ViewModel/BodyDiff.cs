using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Birko.Xaml.Core.Diff;

namespace DesktopFinstatApiTester.ViewModel
{
    /// <summary>One difference between the baseline body (A) and a compared one (B), as the comparison window lists it.</summary>
    public sealed class BodyChange
    {
        /// <summary>added, removed or changed.</summary>
        public string Kind { get; set; }
        /// <summary>Reads like property access: <c>Items[2].Ref</c>; the root is "".</summary>
        public string Path { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public sealed class BodyDiff
    {
        public List<BodyChange> Changes { get; } = new List<BodyChange>();
        public bool Equal => Changes.Count == 0;
        public int Added => Changes.Count(c => c.Kind == "added");
        public int Removed => Changes.Count(c => c.Kind == "removed");
        public int Changed => Changes.Count(c => c.Kind == "changed");
    }

    /// <summary>
    /// Compares two bodies with Birko.Xaml.Core's diff engine (the ports of birko-web-core's <c>diffJson</c> and
    /// <c>xmlToTree</c>), strings passed through a normalizer on both sides.
    /// </summary>
    public static class BodyDiffer
    {

        /// <summary>The engine's options for a comparison: every string on both sides goes through <paramref name="normalize"/>.</summary>
        public static JsonDiffOptions Options(Func<string, string> normalize)
        {
            var options = new JsonDiffOptions();
            if (normalize != null)
            {
                options.Normalize.Add(new JsonNormalizer("**", (value, _) =>
                    value is JsonValue v && v.TryGetValue<string>(out var text) ? JsonValue.Create(normalize(text)) : value));
            }
            return options;
        }

        public static BodyDiff Diff(string a, string b, bool xml, Func<string, string> normalize)
        {
            var result = JsonDiff.Diff(Parse(a, xml), Parse(b, xml), Options(normalize));
            var diff = new BodyDiff();
            diff.Changes.AddRange(result.Changes.Select(c => new BodyChange
            {
                Kind = c.Kind.ToString().ToLowerInvariant(),
                Path = c.Path,
                OldValue = c.HasOldValue ? Show(c.OldValue) : null,
                NewValue = c.HasNewValue ? Show(c.NewValue) : null,
            }));
            return diff;
        }

        /// <summary>A body that does not parse (an HTML error page) is compared as one text value.</summary>
        private static JsonNode Parse(string text, bool xml)
        {
            if (string.IsNullOrEmpty(text))
            {
                return JsonValue.Create(text ?? string.Empty);
            }
            if (xml)
            {
                return XmlTree.TryToTree(text, out var tree) ? tree : JsonValue.Create(text);
            }
            try
            {
                return JsonNode.Parse(text);
            }
            catch (Exception)
            {
                return JsonValue.Create(text);
            }
        }

        private static string Show(JsonNode value)
        {
            switch (value)
            {
                case null: return "null";
                case JsonArray l: return string.Format("[{0} items]", l.Count);
                case JsonObject o: return string.Format("{{{0} fields}}", o.Count);
                default: return value is JsonValue v && v.TryGetValue<string>(out var s) ? s : value.ToJsonString();
            }
        }
    }
}
