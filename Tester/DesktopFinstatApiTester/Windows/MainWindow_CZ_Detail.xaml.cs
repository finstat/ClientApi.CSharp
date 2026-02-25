extern alias CZ;
using System.Windows;

namespace DesktopFinstatApiTester.Windows
{
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private void buttonCZFree_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Basic", "CZ", CZBasic, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO")
            });
        }

        private object CZBasic(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestBasic((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonCZDetail_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("Detail", "CZ", CZDetail, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO")
            });
        }

        private object CZDetail(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestDetail((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }

        private void buttonCZPremium_Click(object sender, RoutedEventArgs e)
        {
            DoApiRequest("PremiumCZ", "CZ", CZPremium, new[] {
                new ApiCallParameter(ParameterTypeEnum.String, "IČO")
            });
        }

        private object CZPremium(object[] parameters)
        {
            var client = CreateCZApiClient();
            var result = client.RequestPremium((string)parameters[0], IsJSON()).GetAwaiter().GetResult();
            AppInstance.Limits.FromModel(client.Limits);
            return result;
        }
    }
}
