using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Birko.Xaml.Core.Dialogs;
using Birko.Xaml.Core.Forms;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// Parameter prompts. The bespoke WPF dialog windows (Input / DateTime / Pick) are gone — Birko's
    /// <see cref="IDialogService"/> already provides themed, awaitable equivalents, and the file and
    /// folder pickers come from Avalonia's <see cref="IStorageProvider"/> instead of WinForms (which is
    /// what made the old project Windows-only).
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>Single-field holder for the date prompt — Birko's <c>Form</c> binds a model property.</summary>
        private sealed class DateHolder
        {
            public DateTime? Date { get; set; }
        }

        private Task<bool> GetPromptAsync(ApiCallParameter parameter)
        {
            var title = !string.IsNullOrEmpty(parameter?.Title) ? parameter.Title : "Confirm action?";
            return _dialogs.ConfirmAsync(title, new ConfirmOptions
            {
                Title = "Prompt",
                ConfirmText = "Yes",
                CancelText = "No",
            });
        }

        private async Task<object> GetPickAsync(ApiCallParameter parameter)
        {
            var options = (parameter?.Values ?? Enumerable.Empty<object>())
                .Select(value => new ChooseOption<object> { Label = value?.ToString(), Value = value })
                .ToList();

            return await _dialogs.ChooseAsync(
                !string.IsNullOrEmpty(parameter?.Title) ? parameter.Title : "Choose a value",
                options,
                "Choose");
        }

        private async Task<string> GetInputAsync(ApiCallParameter parameter)
        {
            // The title doubles as the placeholder — the WPF version faked a watermark by seeding the
            // textbox with the title and clearing it on focus.
            var label = !string.IsNullOrEmpty(parameter?.Title) ? parameter.Title : "Value";
            var value = await _dialogs.PromptAsync(label, new PromptOptions
            {
                Title = "Input",
                Placeholder = label,
                ConfirmText = "Send",
            });
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private async Task<DateTime?> GetDatePickerAsync(ApiCallParameter parameter)
        {
            var label = !string.IsNullOrEmpty(parameter?.Title) ? parameter.Title : "Date";
            var result = await _dialogs.PromptFormAsync(
                new DateHolder(),
                new[] { new FormField { Name = nameof(DateHolder.Date), Label = label, Type = FieldType.Date } },
                "Date");
            return result?.Date;
        }

        private async Task<string> GetFolderBrowserDialogAsync(ApiCallParameter parameter)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = !string.IsNullOrEmpty(parameter?.Title) ? parameter.Title : "Select folder",
                AllowMultiple = false,
            });
            return folders?.FirstOrDefault()?.TryGetLocalPath();
        }

        private async Task<string> GetFileBrowserDialogAsync(ApiCallParameter parameter)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = !string.IsNullOrEmpty(parameter?.Title) ? parameter.Title : "Select file",
                AllowMultiple = false,
            });
            return files?.FirstOrDefault()?.TryGetLocalPath();
        }
    }
}
