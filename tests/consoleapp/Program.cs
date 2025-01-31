// See https://aka.ms/new-console-template for more information


using System.Diagnostics.CodeAnalysis;

using Nefarius.Utilities.ExceptionEnricher;

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
}

return;

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

void Baz(/* no args */)
{
    throw new InvalidOperationException("Test exception");
}