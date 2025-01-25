// See https://aka.ms/new-console-template for more information


using Nefarius.Utilities.ExceptionEnricher;

try
{
    Foo();
}
catch (Exception ex)
{
    // just a demo, do not do IRL ofc. ;)
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

void Foo()
{
    Bar();
}

void Bar()
{
    Baz();
}

void Baz()
{
    throw new InvalidOperationException("Test exception");
}