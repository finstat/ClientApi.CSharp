using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DesktopFinstatApiTester.ViewModel
{
    /// <summary>One difference between the baseline body (A) and a compared one (B).</summary>
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
    /// STOPGAP until Birko.Xaml.Core ships its diff engine (Birko.Framework FEATURE-020, the port of birko-web-core's
    /// <c>diffJson</c> / <c>xmlToTree</c>); then this file goes and the comparison calls that engine. Kept to what the
    /// comparison window needs: a structural diff of two bodies, lists compared by position, XML read as a tree
    /// (repeated elements become a list, attributes <c>@name</c>, xsi:nil null), strings passed through a normalizer.
    /// </summary>
    public static class BodyDiffer
    {
        private static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        public static BodyDiff Diff(string a, string b, bool xml, Func<string, string> normalize)
        {
            var result = new BodyDiff();
            Walk(Parse(a, xml), Parse(b, xml), string.Empty, normalize ?? (s => s), result.Changes);
            return result;
        }

        /// <summary>A body that does not parse (an HTML error page) is compared as one text value.</summary>
        private static object Parse(string text, bool xml)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            try
            {
                return xml ? FromXml(XDocument.Parse(text).Root) : FromJson(JsonNode.Parse(text));
            }
            catch (Exception)
            {
                return text;
            }
        }

        private static object FromJson(JsonNode node)
        {
            switch (node)
            {
                case null: return null;
                case JsonObject o: return o.ToDictionary(p => p.Key, p => FromJson(p.Value));
                case JsonArray arr: return arr.Select(FromJson).ToList();
                default: return node.ToJsonString().Trim('"');
            }
        }

        private static object FromXml(XElement e)
        {
            if ((string)e.Attribute(Xsi + "nil") == "true")
            {
                return null;
            }
            var attributes = e.Attributes().Where(a => !a.IsNamespaceDeclaration && a.Name.Namespace != Xsi).ToList();
            if (!e.HasElements && attributes.Count == 0)
            {
                return e.Value;
            }
            var map = new Dictionary<string, object>();
            foreach (var a in attributes)
            {
                map["@" + a.Name.LocalName] = a.Value;
            }
            foreach (var group in e.Elements().GroupBy(c => c.Name.LocalName))
            {
                var items = group.Select(FromXml).ToList();
                map[group.Key] = items.Count == 1 ? items[0] : items;
            }
            return map;
        }

        private static void Walk(object a, object b, string path, Func<string, string> normalize, List<BodyChange> changes)
        {
            if (a is Dictionary<string, object> da && b is Dictionary<string, object> db)
            {
                foreach (var key in da.Keys.Union(db.Keys))
                {
                    var child = Child(path, key);
                    if (!db.ContainsKey(key)) changes.Add(new BodyChange { Kind = "removed", Path = child, OldValue = Show(da[key]) });
                    else if (!da.ContainsKey(key)) changes.Add(new BodyChange { Kind = "added", Path = child, NewValue = Show(db[key]) });
                    else Walk(da[key], db[key], child, normalize, changes);
                }
                return;
            }
            if (a is List<object> la && b is List<object> lb)
            {
                for (int i = 0; i < Math.Max(la.Count, lb.Count); i++)
                {
                    var child = string.Format("{0}[{1}]", path, i);
                    if (i >= lb.Count) changes.Add(new BodyChange { Kind = "removed", Path = child, OldValue = Show(la[i]) });
                    else if (i >= la.Count) changes.Add(new BodyChange { Kind = "added", Path = child, NewValue = Show(lb[i]) });
                    else Walk(la[i], lb[i], child, normalize, changes);
                }
                return;
            }
            var sa = a is string s1 ? normalize(s1) : Show(a);
            var sb = b is string s2 ? normalize(s2) : Show(b);
            // a null and the text "null" are different values
            if (sa != sb || (a == null) != (b == null))
            {
                changes.Add(new BodyChange { Kind = "changed", Path = path, OldValue = Show(a), NewValue = Show(b) });
            }
        }

        private static readonly Regex Identifier = new Regex(@"^[A-Za-z_$][A-Za-z0-9_$]*$");

        private static string Child(string path, string key) =>
            Identifier.IsMatch(key) ? (path.Length == 0 ? key : path + "." + key) : string.Format("{0}[\"{1}\"]", path, key);

        private static string Show(object value)
        {
            switch (value)
            {
                case null: return "null";
                case string s: return s;
                case List<object> l: return string.Format("[{0} items]", l.Count);
                case Dictionary<string, object> d: return string.Format("{{{0} fields}}", d.Count);
                default: return value.ToString();
            }
        }
    }
}
