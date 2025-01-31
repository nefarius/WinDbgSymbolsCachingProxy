using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Windows;

using Nefarius.Utilities.ExceptionEnricher;

namespace WpfTestApp;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
    private async void App_OnStartup(object sender, StartupEventArgs e)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        try
        {
            Foo();
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
            
            this.Shutdown();
        }

        /*
         * Some nested local functions to make a demo call stack
         */

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        void Foo(int firstArg = 1, int secondArg = 2)
        {
            Bar();
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        void Bar(params List<int> someArgs)
        {
            Baz();
        }

        void Baz( /* no args */)
        {
            throw new InvalidOperationException("Test exception");
        }
    }
}