using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// Read-only text viewer for curl commands, raw response bodies, serialized nodes and stack
    /// traces. Read-only because nothing ever consumed edits — the WPF version left it editable.
    /// </summary>
    public partial class OutputWindow : Window
    {
        public string Text { get; }

        public string OutputText
        {
            get { return textBoxOutput.Text; }
            set { textBoxOutput.Text = value; }
        }

        /// <summary>Parameterless ctor so the AXAML stays loadable by the runtime XAML loader
        /// (AVLN3001); callers use the <see cref="OutputWindow(string)"/> overload.</summary>
        public OutputWindow() : this(string.Empty) { }

        public OutputWindow(string text)
        {
            InitializeComponent();
            Text = text;
            RefreshTextBox();
        }

        private void RefreshTextBox() => OutputText = Text;

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e) => RefreshTextBox();

        private void ButtonCopy_Click(object sender, RoutedEventArgs e) =>
            _ = Clipboard?.SetTextAsync(Text ?? string.Empty);
    }
}
