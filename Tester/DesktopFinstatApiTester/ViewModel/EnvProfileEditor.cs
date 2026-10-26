namespace DesktopFinstatApiTester.ViewModel
{
    /// <summary>
    /// The profile shown in the Settings dialog, as flat fields for Birko's <c>Form</c>. Writes straight
    /// into the working copy it wraps; the port is text, so an empty field means the default port.
    /// </summary>
    public class EnvProfileEditor : ViewModel
    {
        public EnvProfileEditor(Model.EnvProfile profile)
        {
            Profile = profile;
        }

        public Model.EnvProfile Profile { get; }

        public string Name
        {
            get { return Profile.Name; }
            set { Profile.Name = value?.Trim(); }
        }

        public string SkHost
        {
            get { return Profile.SkHost; }
            set { Profile.SkHost = value?.Trim(); }
        }

        public string CzHost
        {
            get { return Profile.CzHost; }
            set { Profile.CzHost = value?.Trim(); }
        }

        /// <summary>Empty = the default port; anything outside 1–65535 counts as empty.</summary>
        public string Port
        {
            get { return Profile.Port?.ToString(); }
            set { Profile.Port = int.TryParse(value?.Trim(), out var port) && port >= 1 && port <= 65535 ? port : (int?)null; }
        }

        public string PublicKey
        {
            get { return Profile.PublicKey; }
            set { Profile.PublicKey = value?.Trim(); }
        }

        public string PrivateKey
        {
            get { return Profile.PrivateKey; }
            set { Profile.PrivateKey = value?.Trim(); }
        }

        public Model.ProfileColor Color
        {
            get { return Profile.Color ?? Model.ProfileColor.None; }
            set { Profile.Color = value; }
        }
    }
}
