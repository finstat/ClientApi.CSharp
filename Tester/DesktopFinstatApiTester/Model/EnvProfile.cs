using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DesktopFinstatApiTester.Model
{
    /// <summary>Marks a profile wherever the active one is shown, so production is not called by mistake.</summary>
    public enum ProfileColor
    {
        None,
        Green,
        Amber,
        Red,
    }

    /// <summary>
    /// A named FinStat environment: the SK and the CZ API host the tester calls, one port for both,
    /// optional API keys that replace the global ones, and a colour. Kept in step with the web tester's
    /// <c>EnvProfile</c> in <c>src/settings/settings-store.ts</c>.
    /// </summary>
    public class EnvProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        /// <summary>Built-in profiles keep their name and hosts; only the port, the keys and the colour can change.</summary>
        public bool BuiltIn { get; set; }
        public string SkHost { get; set; }
        public string CzHost { get; set; }
        /// <summary>null = the default HTTPS port.</summary>
        public int? Port { get; set; }
        /// <summary>null or empty = use the global key.</summary>
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        /// <summary>null only in settings saved before colours existed; <see cref="EnvProfiles.Merge"/> fills it in.</summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ProfileColor? Color { get; set; }

        [JsonIgnore]
        public string SkUrl => EnvProfiles.Url(SkHost, Port);

        [JsonIgnore]
        public string CzUrl => EnvProfiles.Url(CzHost, Port);

        /// <summary>Name shown in history and the comparison; a port is part of it.</summary>
        [JsonIgnore]
        public string Label => Port.HasValue ? string.Format("{0}:{1}", Name, Port) : Name;

        public EnvProfile Clone() => (EnvProfile)MemberwiseClone();

        public override string ToString() => Name;
    }

    public static class EnvProfiles
    {
        public static IReadOnlyList<EnvProfile> BuiltIns { get; } = new[]
        {
            BuiltIn("finstat-sk",   "finstat.sk",   "www.finstat.sk",  "cz.finstat.sk",  ProfileColor.Red),
            BuiltIn("finstat-cz",   "finstat.cz",   "sk.finstat.cz",   "www.finstat.cz", ProfileColor.Red),
            BuiltIn("zdrojak-eu",   "zdrojak.eu",   "zdrojak.eu",      "cz.zdrojak.eu"),
            BuiltIn("zdrojakcz-eu", "zdrojakcz.eu", "sk.zdrojakcz.eu", "zdrojakcz.eu"),
        };

        private static EnvProfile BuiltIn(string id, string name, string skHost, string czHost, ProfileColor color = ProfileColor.None) =>
            new EnvProfile { Id = id, Name = name, BuiltIn = true, SkHost = skHost, CzHost = czHost, Color = color };

        public static string Url(string host, int? port) =>
            string.Format("https://{0}{1}/api", host, port.HasValue ? ":" + port.Value : string.Empty);

        public static EnvProfile Find(IReadOnlyList<EnvProfile> profiles, string id) =>
            profiles.FirstOrDefault(p => p.Id == id) ?? profiles[0];

        /// <summary>
        /// Built-ins always come from code (a stored copy only keeps its port, keys and colour), custom
        /// profiles as stored. A profile saved before colours existed gets the built-in's colour, a custom one none.
        /// </summary>
        public static List<EnvProfile> Merge(IEnumerable<EnvProfile> stored)
        {
            var list = stored?.Where(p => p != null).ToList() ?? new List<EnvProfile>();
            var builtIns = BuiltIns.Select(b =>
            {
                var s = list.FirstOrDefault(x => x.Id == b.Id);
                var p = b.Clone();
                p.Port = s?.Port;
                p.PublicKey = s?.PublicKey;
                p.PrivateKey = s?.PrivateKey;
                p.Color = s?.Color ?? b.Color;
                return p;
            });
            var custom = list.Where(p => BuiltIns.All(b => b.Id != p.Id)).Select(p =>
            {
                var c = p.Clone();
                c.BuiltIn = false;
                c.Color = p.Color ?? ProfileColor.None;
                return c;
            });
            return builtIns.Concat(custom).ToList();
        }

        /// <summary>Settings saved before profiles existed hold two URLs: pick the built-in they point at, or keep them as a custom profile.</summary>
        public static (List<EnvProfile> Profiles, string ActiveId) Migrate(string skUrl, string czUrl)
        {
            var profiles = Merge(null);
            var sk = ParseUrl(skUrl);
            var cz = ParseUrl(czUrl);
            if (sk == null || cz == null)
            {
                return (profiles, profiles[0].Id);
            }
            var port = sk.Value.Port ?? cz.Value.Port;
            var match = profiles.FirstOrDefault(p => SameHost(p.SkHost, sk.Value.Host) && SameHost(p.CzHost, cz.Value.Host));
            if (match != null)
            {
                match.Port = port;
                return (profiles, match.Id);
            }
            var custom = new EnvProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = sk.Value.Host,
                SkHost = sk.Value.Host,
                CzHost = cz.Value.Host,
                Port = port,
                Color = ProfileColor.None,
            };
            profiles.Add(custom);
            return (profiles, custom.Id);
        }

        // finstat.sk and www.finstat.sk are the same server
        private static bool SameHost(string x, string y)
        {
            string Bare(string h) => h.ToLowerInvariant().StartsWith("www.") ? h.ToLowerInvariant().Substring(4) : h.ToLowerInvariant();
            return Bare(x) == Bare(y);
        }

        private static (string Host, int? Port)? ParseUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }
            if (!Uri.TryCreate(url.Contains("://") ? url : "https://" + url, UriKind.Absolute, out var uri))
            {
                return null;
            }
            return (uri.Host, uri.IsDefaultPort ? (int?)null : uri.Port);
        }
    }
}
