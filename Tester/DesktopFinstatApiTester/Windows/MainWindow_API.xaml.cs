using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private bool IsJSON()
        {
            return AppInstance?.Settings?.ResponseType == Model.ResponseType.JSON;
        }

        private void Client_OnResponse(Dictionary<string, string[]> header)
        {
            if (AppInstance?.ResponseItems != null && AppInstance.ResponseItems.Any())
            {
                var first = AppInstance.ResponseItems.First();
                if (first != null)
                {
                    first.ResponseHeaders = header;
                }
            }
        }

        private void Client_OnRequest(Dictionary<string, string[]> header)
        {
            if (AppInstance?.ResponseItems != null && AppInstance.ResponseItems.Any())
            {
                var first = AppInstance.ResponseItems.First();
                if (first != null)
                {
                    first.RequestHeaders = header;
                }
            }
        }

        private void Client_OnResponseContent(byte[] content)
        {
            if (AppInstance?.ResponseItems != null && AppInstance.ResponseItems.Any())
            {
                var first = AppInstance.ResponseItems.First();
                if (first != null)
                {
                    first.Content = content;
                }
            }
        }

        private void DoApiRequest(string requestname, string apisource, Func<object[], object> apiCallFunc, ApiCallParameter[] parameterTypes = null)
        {
            bool hasParameter =  parameterTypes?.Any() ?? false;

            List<object> parameters = new List<object>();
            if (hasParameter)
            {
                foreach (var parameterType in parameterTypes)
                {
                    object parameter = null;
                    bool valid = false;
                    while (!valid)
                    {
                        switch (parameterType.Type)
                        {
                            case ParameterTypeEnum.String: parameter = GetInput(parameterType); break;
                            case ParameterTypeEnum.Int: parameter = GetInput(parameterType); break;
                            case ParameterTypeEnum.Date: parameter = GetDatePicker(parameterType); break;
                            case ParameterTypeEnum.Folder: parameter = GetFolderBrowserDialog(parameterType); break;
                            case ParameterTypeEnum.File: parameter = GetFileBrowserDialog(parameterType); break;
                            case ParameterTypeEnum.Prompt: parameter = GetPrompt(parameterType); break;
                            case ParameterTypeEnum.Pick: parameter = GetPick(parameterType); break;
                        }
                        valid = (parameterType.ValidFunction != null)
                                ? parameterType.ValidFunction(parameter)
                                : (new[] { ParameterTypeEnum.String, ParameterTypeEnum.Int, ParameterTypeEnum.Folder, ParameterTypeEnum.File }.Contains(parameterType.Type))
                                    ? !string.IsNullOrEmpty((string)parameter)
                                    : parameterType.Type != ParameterTypeEnum.Pick || parameter != null;
                        if (
                            !valid
                            && MessageBox.Show("Value is not valid or empty. Do you want to fix it?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No
                        )
                        {
                            valid = true;
                        }
                        if (valid && parameterType.Type == ParameterTypeEnum.Int)
                        {
                            if (int.TryParse((string)parameter, out int result))
                            {
                                parameter = result;
                            }
                            else if (MessageBox.Show("Value is not valid for int number. Do you want to fix it?", "Prompt", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                            {
                                valid = false;
                            }
                        }
                    }
                    parameters.Add(parameter);
                }
            }

            RunApiRequest(requestname, apisource, apiCallFunc, parameters);
        }

        private void RunApiRequest(string requestname, string apisource, Func<object[], object> apiCallFunc, IEnumerable<object> parameters)
        {
            object detail = null;
            ViewModel.ResponseItem item = null;
            Exception ex = null;
            var statusWindow = new StatusWindow(3)
            {
                Owner = this
            };
            statusWindow.Start(() =>
            {
                try
                {
                    statusWindow.Update(1, "Creating Request");
                    Invoke(this, () => item = AppInstance.Add(requestname, apisource, parameters.ToArray()));
                    statusWindow.Update(2, "Requesting api call");
                    detail = apiCallFunc(parameters.ToArray());
                    statusWindow.Update(3, "Storing Response");
                }
                catch (Exception e)
                {
                    ex = e;
                }
            },
            () =>
            {
                if (ex != null)
                {
                    Invoke(this, () => ShowException(ex));
                }
                else
                {
                    Invoke(this, () =>
                    {
                        item.AddData(new[] { detail });
                        if (item.Data != null && item.Data.Any())
                        {
                            datagridResponse.SelectedIndex = 0;
                        }
                    });
                }
            });
        }
    }
}
