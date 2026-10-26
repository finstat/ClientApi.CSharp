using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// <summary>Trailing pick on every CZ request (only finstat.cz honours Lang); Default keeps the Settings language.</summary>
        private static ApiCallParameter LangParameter() => new ApiCallParameter(ParameterTypeEnum.Pick, "Response language (Default = Settings)")
        {
            Values = Enum.GetValues(typeof(Model.ResponseLang)).Cast<object>().ToList(),
        };

        /// <summary>
        /// Lang argument for the CZ client calls: the language picked for the request (its last parameter),
        /// else the Settings one; null for <see cref="Model.ResponseLang.Default"/>.
        /// </summary>
        private string RequestLang(IEnumerable<object> parameters = null)
        {
            var lang = parameters?.LastOrDefault() is Model.ResponseLang picked && picked != Model.ResponseLang.Default
                ? picked
                : AppInstance?.Settings?.Lang ?? Model.ResponseLang.Default;
            return lang == Model.ResponseLang.Default ? null : lang.ToString().ToLowerInvariant();
        }

        private bool IsJSON()
        {
            return s_override.Value?.Json ?? AppInstance?.Settings?.ResponseType == Model.ResponseType.JSON;
        }

        /// <summary>
        /// A call made for a comparison: the profile and format it goes to, and what came back. While one is set
        /// (it flows with the call through <see cref="AsyncLocal{T}"/>), the client hooks write here instead of
        /// onto the newest history row, and the status bar's limits stay those of the active profile.
        /// </summary>
        internal sealed class CallOverride
        {
            public Model.EnvProfile Profile { get; set; }
            public bool Json { get; set; }
            public byte[] Content { get; set; }
            public Dictionary<string, string[]> RequestHeaders { get; set; }
            public Dictionary<string, string[]> ResponseHeaders { get; set; }
        }

        private static readonly AsyncLocal<CallOverride> s_override = new AsyncLocal<CallOverride>();

        /// <summary>The profile a call goes to: the given one, else a comparison's, else the active one.</summary>
        private Model.EnvProfile CallProfile(Model.EnvProfile profile) =>
            profile ?? s_override.Value?.Profile ?? AppInstance.Settings.ActiveProfile;

        /// <summary>The status bar shows the quota of the active profile only.</summary>
        private bool IsActiveProfileCall() =>
            s_override.Value == null || s_override.Value.Profile.Id == AppInstance.Settings.ActiveProfileId;

        /// <summary>The outcome of one comparison call.</summary>
        internal sealed class ComparisonCall
        {
            public object Result { get; set; }
            public Exception Error { get; set; }
            public CallOverride Call { get; set; }
            public string Body => Call.Content == null ? null : System.Text.Encoding.UTF8.GetString(Call.Content);
        }

        /// <summary>
        /// Sends a recorded request (with <paramref name="args"/> in place of its parameters) to a profile in the given
        /// format, off the UI thread, without recording it. Null when the request cannot be repeated.
        /// </summary>
        internal async Task<ComparisonCall> CallForComparisonAsync(ViewModel.ResponseItem item, object[] args, Model.EnvProfile profile, bool json)
        {
            var apiCall = ApiCallFor(item);
            if (apiCall == null)
            {
                return null;
            }
            var call = new CallOverride { Profile = profile, Json = json };
            var outcome = new ComparisonCall { Call = call };
            await Task.Run(() =>
            {
                s_override.Value = call;
                try
                {
                    outcome.Result = apiCall(args);
                }
                catch (Exception ex)
                {
                    outcome.Error = ex;
                }
            });
            return outcome;
        }

        /// <summary>
        /// The profile a history row went to: recorded by name, plus ":port" when one was set. Older rows
        /// and a profile deleted since fall back to the active one.
        /// </summary>
        private Model.EnvProfile ProfileOfItem(ViewModel.ResponseItem item)
        {
            var active = AppInstance.Settings.ActiveProfile;
            if (string.IsNullOrEmpty(item?.Profile))
            {
                return active;
            }
            var match = System.Text.RegularExpressions.Regex.Match(item.Profile, @"^(.*):(\d+)$");
            var name = match.Success ? match.Groups[1].Value : item.Profile;
            var profile = (AppInstance.Settings.Profiles.FirstOrDefault(p => p.Name == name) ?? active).Clone();
            // a row without a port went to the default one, whatever the profile's port is now
            profile.Port = match.Success ? int.Parse(match.Groups[2].Value) : (int?)null;
            return profile;
        }

        private void Client_OnResponse(Dictionary<string, string[]> header)
        {
            if (s_override.Value is CallOverride call)
            {
                call.ResponseHeaders = header;
                return;
            }
            var first = AppInstance?.ResponseItems?.FirstOrDefault();
            if (first != null)
            {
                first.ResponseHeaders = header;
            }
        }

        private void Client_OnRequest(Dictionary<string, string[]> header)
        {
            if (s_override.Value is CallOverride call)
            {
                call.RequestHeaders = header;
                return;
            }
            var first = AppInstance?.ResponseItems?.FirstOrDefault();
            if (first != null)
            {
                first.RequestHeaders = header;
            }
        }

        private void Client_OnResponseContent(byte[] content)
        {
            if (s_override.Value is CallOverride call)
            {
                call.Content = content;
                return;
            }
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
            item.Profile = AppInstance.Settings.ActiveProfile.Label;

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
