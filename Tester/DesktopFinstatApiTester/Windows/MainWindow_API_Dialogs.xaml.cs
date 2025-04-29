using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private bool GetPrompt(ApiCallParameter parameter)
        {
            var title = "Confirm action?";
            if (parameter != null && !string.IsNullOrEmpty(parameter?.Title))
            {
                title = parameter?.Title;
            }
            return (MessageBox.Show(title, "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes);
        }

        private object GetPick(ApiCallParameter parameter)
        {
            PickWindow dialog = new PickWindow(parameter.Values)
            {
                Owner = this,
            };

            if (parameter != null && !string.IsNullOrEmpty(parameter?.Title))
            {
                dialog.Title = parameter?.Title;
            }

            if (dialog.ShowDialog() == true)
            {
                return dialog.Value;
            }
            return null;
        }

        private string GetInput(ApiCallParameter parameter)
        {
            InputWindow dialog = new InputWindow()
            {
                Owner = this,
            };

            if (parameter != null && !string.IsNullOrEmpty(parameter?.Title))
            {
                dialog.Title = parameter?.Title;
                dialog.textBoxInput.Text = parameter.Title;
                dialog.textBoxInput.GotFocus += (sender, e) =>
                {
                    if (dialog.textBoxInput.Text == parameter?.Title)
                    {
                        dialog.textBoxInput.Text = string.Empty;
                    }
                };
                dialog.textBoxInput.LostFocus += (sender, e) =>
                {
                    if (String.IsNullOrWhiteSpace(dialog.textBoxInput.Text))
                    {
                        dialog.textBoxInput.Text = parameter?.Title;
                    }
                };
            }

            if (dialog.ShowDialog() == true)
            {
                return (!string.IsNullOrEmpty(dialog.Text) && dialog.Text != parameter?.Title) ? dialog.Text.Trim() : null;
            }
            return null;
        }

        private DateTime? GetDatePicker(ApiCallParameter parameter)
        {
            DateTimeWindow dialog = new DateTimeWindow()
            {
                Owner = this,
            };

            if (parameter != null && !string.IsNullOrEmpty(parameter?.Title))
            {
                dialog.Title = parameter?.Title;
                dialog.datePickerInput.Text = parameter.Title;
                dialog.datePickerInput.GotFocus += (sender, e) =>
                {
                    if (dialog.datePickerInput.Text == parameter?.Title)
                    {
                        dialog.datePickerInput.Text = string.Empty;
                    }
                };
                dialog.datePickerInput.LostFocus += (sender, e) =>
                {
                    if (dialog.Date == null)
                    {
                        dialog.datePickerInput.Text = parameter?.Title;
                    }
                };
            }

            if (dialog.ShowDialog() == true)
            {
                return (dialog.datePickerInput.Text != parameter.Title && dialog.Date != null) ? dialog.Date : null;
            }
            return null;
        }

        private string GetFolderBrowserDialog(ApiCallParameter parameter)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (parameter != null && !string.IsNullOrEmpty(parameter?.Title))
                {
                    dialog.Description = parameter?.Title;
                }
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK || dialog.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
                {
                    return dialog.SelectedPath;
                }
            }
            return null;
        }

        private string GetFileBrowserDialog(ApiCallParameter parameter)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                if (parameter != null && !string.IsNullOrEmpty(parameter?.Title))
                {
                    dialog.Title = parameter?.Title;
                }
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK || dialog.ShowDialog() == System.Windows.Forms.DialogResult.Yes)
                {
                    return dialog.FileName;
                }
            }
            return null;
        }
    }
}
