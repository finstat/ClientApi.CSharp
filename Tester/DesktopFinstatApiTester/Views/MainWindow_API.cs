using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Birko.Xaml.Core.Dialogs;

namespace DesktopFinstatApiTester.Views
{
    /// <summary>
    /// The request pipeline: collect the call's parameters through dialogs, run the (blocking) client
    /// call off the UI thread behind Birko's busy overlay, then fold the result into the history grid.
    /// </summary>
    public partial class MainWindow
    {
        private bool IsJSON()
        {
            return AppInstance?.Settings?.ResponseType == Model.ResponseType.JSON;
        }

        private void Client_OnResponse(Dictionary<string, string[]> header)
        {
            var first = AppInstance?.ResponseItems?.FirstOrDefault();
            if (first != null)
            {
                first.ResponseHeaders = header;
            }
        }

        private void Client_OnRequest(Dictionary<string, string[]> header)
        {
            var first = AppInstance?.ResponseItems?.FirstOrDefault();
            if (first != null)
            {
                first.RequestHeaders = header;
            }
        }

        private void Client_OnResponseContent(byte[] content)
        {
            var first = AppInstance?.ResponseItems?.FirstOrDefault();
            if (first != null)
            {
                first.Content = content;
            }
        }

        /// <summary>Prompt for each declared parameter, then run the call.</summary>
        private async Task DoApiRequestAsync(
            string requestname,
            string apisource,
            Func<object[], object> apiCallFunc,
            ApiCallParameter[] parameterTypes = null)
        {
            var parameters = new List<object>();

            foreach (var parameterType in parameterTypes ?? Array.Empty<ApiCallParameter>())
            {
                object parameter = null;
                bool valid = false;
                while (!valid)
                {
                    switch (parameterType.Type)
                    {
                        case ParameterTypeEnum.String: parameter = await GetInputAsync(parameterType); break;
                        case ParameterTypeEnum.Int: parameter = await GetInputAsync(parameterType); break;
                        case ParameterTypeEnum.Date: parameter = await GetDatePickerAsync(parameterType); break;
                        case ParameterTypeEnum.Folder: parameter = await GetFolderBrowserDialogAsync(parameterType); break;
                        case ParameterTypeEnum.File: parameter = await GetFileBrowserDialogAsync(parameterType); break;
                        case ParameterTypeEnum.Prompt: parameter = await GetPromptAsync(parameterType); break;
                        case ParameterTypeEnum.Pick: parameter = await GetPickAsync(parameterType); break;
                    }

                    valid = (parameterType.ValidFunction != null)
                            ? parameterType.ValidFunction(parameter)
                            : (new[] { ParameterTypeEnum.String, ParameterTypeEnum.Int, ParameterTypeEnum.Folder, ParameterTypeEnum.File }.Contains(parameterType.Type))
                                ? !string.IsNullOrEmpty((string)parameter)
                                : parameterType.Type != ParameterTypeEnum.Pick || parameter != null;

                    // Declining the retry accepts the value as-is, exactly as the WPF dialog did.
                    if (!valid
                        && !await _dialogs.ConfirmAsync(
                            "Value is not valid or empty. Do you want to fix it?",
                            new ConfirmOptions { Title = "Prompt", ConfirmText = "Fix it", CancelText = "Continue anyway" }))
                    {
                        valid = true;
                    }

                    if (valid && parameterType.Type == ParameterTypeEnum.Int)
                    {
                        if (int.TryParse((string)parameter, out int result))
                        {
                            parameter = result;
                        }
                        else if (await _dialogs.ConfirmAsync(
                            "Value is not valid for int number. Do you want to fix it?",
                            new ConfirmOptions { Title = "Prompt", ConfirmText = "Fix it", CancelText = "Continue anyway" }))
                        {
                            valid = false;
                        }
                    }
                }
                parameters.Add(parameter);
            }

            await RunApiRequestAsync(requestname, apisource, apiCallFunc, parameters);
        }

        /// <summary>
        /// Record the request, run it off the UI thread behind the busy overlay, then attach the
        /// response. The history row is inserted <i>before</i> the call so the client's
        /// request/response header callbacks have a row to write onto.
        /// </summary>
        private async Task RunApiRequestAsync(
            string requestname,
            string apisource,
            Func<object[], object> apiCallFunc,
            IEnumerable<object> parameters)
        {
            var args = parameters.ToArray();
            var item = AppInstance.Add(requestname, apisource, args);

            object detail = null;
            Exception error = null;

            // The generated clients are blocking (.GetAwaiter().GetResult()), so push them to the
            // thread pool — otherwise the overlay's spinner would never paint.
            await _dialogs.BusyAsync(
                async () =>
                {
                    try
                    {
                        detail = await Task.Run(() => apiCallFunc(args));
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                },
                string.Format("Requesting {0} ({1})…", requestname, apisource));

            if (error != null)
            {
                await ShowExceptionAsync(error);
                return;
            }

            item.AddData(new[] { detail });
            if (item.Data != null && item.Data.Any())
            {
                datagridResponse.SelectedIndex = 0;
            }
        }
    }
}
