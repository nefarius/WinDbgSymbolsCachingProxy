using System.Net.Http;
using System.Windows;

using Nefarius.Utilities.ExceptionEnricher;

namespace WpfTestApp;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            object obj = null;
            obj.ToString();
        }
        catch (Exception ex)
        {
            // just a demo, do not do IRL ofc. ;)
            // always use IHttpClientFactory or you will get socket exhaustion!
            using HttpClient client = new();
            client.BaseAddress = new Uri("http://192.168.2.12:5000");

            // original
            Console.WriteLine("=== ORIGINAL ===");
            Console.WriteLine(ex);
            // enriched
            Console.WriteLine("=== ENRICHED ===");
            Console.WriteLine(ex.ToRemotelyEnrichedException(client));
        }
    }
}