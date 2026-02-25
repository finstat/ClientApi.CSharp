using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonFree_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Basic", "SK", SKBasic, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO")
            });
        }

        private object SKBasic(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestBasic((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonDetail_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Detail", "SK", SKDetail, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO")
            });
        }

        private object SKDetail(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonExtended_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Extended", "SK", SKExtended, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO")
            });
        }

        private object SKExtended(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestExtendedDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonUltimate_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Ultimate", "SK", SKUltimate, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO")
            });
        }

        private object SKUltimate(object[] parameters)
        {
            var client = CreateSKApiClient();
            var result = client.RequestUltimateDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
