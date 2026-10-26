using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace DesktopFinstatApiTester.Model
{
    public enum ResponseType
    {
        XML,
        JSON
    }

    /// <summary>FinStat response text language (Lang parameter); Default leaves it to the server. Honoured on finstat.cz only.</summary>
    public enum ResponseLang
    {
        Default,
        CS,
        SK
    }
    public class Settings
    {
        public ResponseType ResponseType { get; set; } = ResponseType.XML;
        public ResponseLang Lang { get; set; } = ResponseLang.Default;
        public ApiKeys ApiKeys { get; set; } = new ApiKeys();
        public string StationName { get; set; } = "Api Tester Desktop";
        public string StationID { get; set; } = "api-tester";
        public int TimeOut { get; set; } = 3000;

        /// <summary>null until <see cref="Normalize"/>; then the built-ins followed by the custom profiles.</summary>
        public List<EnvProfile> Profiles { get; set; }
        public string ActiveProfileId { get; set; }

        /// <summary>The ribbon's Quick Access Toolbar as the ribbon's references; null = its defaults.</summary>
        public List<string> QuickAccess { get; set; }

        /// <summary>The two URLs of settings saved before profiles existed; read once by <see cref="Normalize"/>, never written.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string FinStatApiUrl { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string FinStatApiUrlCZ { get; set; }

        [JsonIgnore]
        public EnvProfile ActiveProfile => EnvProfiles.Find(Profiles, ActiveProfileId);

        /// <summary>Fills in the profiles of fresh or older settings, migrating the two URLs of settings saved before profiles existed.</summary>
        public Settings Normalize()
        {
            if (Profiles == null)
            {
                var (profiles, activeId) = EnvProfiles.Migrate(FinStatApiUrl, FinStatApiUrlCZ);
                Profiles = profiles;
                ActiveProfileId = activeId;
            }
            else
            {
                Profiles = EnvProfiles.Merge(Profiles);
            }
            ActiveProfileId = ActiveProfile.Id;
            FinStatApiUrl = null;
            FinStatApiUrlCZ = null;
            return this;
        }

        /// <summary>The profile's own key, else the global one.</summary>
        public string PublicKeyFor(EnvProfile profile) =>
            !string.IsNullOrEmpty(profile?.PublicKey) ? profile.PublicKey : ApiKeys?.PublicKey;

        public string PrivateKeyFor(EnvProfile profile) =>
            !string.IsNullOrEmpty(profile?.PrivateKey) ? profile.PrivateKey : ApiKeys?.PrivateKey;
    }
}
