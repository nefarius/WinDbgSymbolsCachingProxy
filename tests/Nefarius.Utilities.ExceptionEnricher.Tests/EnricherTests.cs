using System.Net;
using System.Reflection;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Nefarius.Utilities.ExceptionEnricher.Tests;

public sealed class EnricherTests
{
    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.PathAndQuery);
            return respond(request);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Send(request, cancellationToken));
        }
    }

    private static Exception Throw()
    {
        try
        {
            throw new InvalidOperationException("boom") { Data = { ["key"] = "value" } };
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    [Fact]
    public void EnrichedException_PreservesOriginalDetails()
    {
        Exception original = Throw();

        EnrichedException enriched = new(original, "   at Fake()");

        Assert.Equal("boom", enriched.Message);
        Assert.Equal("   at Fake()", enriched.StackTrace);
        Assert.Equal(typeof(InvalidOperationException), enriched.OriginalType);
        Assert.Same(original, enriched.OriginalException);
        Assert.Equal("value", enriched.Data["key"]);
        Assert.Equal(original.HResult, enriched.HResult);
        Assert.StartsWith(typeof(InvalidOperationException).ToString(), enriched.ToString());
    }

    [Fact]
    public void ToRemotelyEnrichedException_DoesNotThrow_WhenServerFails()
    {
        RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using HttpClient client = new(handler) { BaseAddress = new Uri("http://localhost/") };

        // A CoreLib exception: its frames live in a module without a local PDB, so the resolver has to ask the server.
        Exception original = Assert.Throws<FormatException>(() => int.Parse("not a number"));

        EnrichedException enriched = original.ToRemotelyEnrichedException(client);

        Assert.NotEmpty(handler.Requests);
        Assert.Equal(original.Message, enriched.Message);
        Assert.Contains("Parse", enriched.StackTrace);
    }

    [Fact]
    public void Resolver_RequestsPortablePdbKey_AndReadsServedSymbols()
    {
        // Copy only the assembly so no PDB sits next to it and the resolver must go to the "server".
        Assembly self = typeof(EnricherTests).Assembly;
        string pdbPath = Path.ChangeExtension(self.Location, ".pdb");
        Assert.True(File.Exists(pdbPath), "test assembly must have a PDB");

        string temp = Directory.CreateTempSubdirectory("enricher-").FullName;
        try
        {
            string dllCopy = Path.Combine(temp, Path.GetFileName(self.Location));
            File.Copy(self.Location, dllCopy);

            byte[] pdb = File.ReadAllBytes(pdbPath);
            RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(pdb)
            });
            using HttpClient client = new(handler) { BaseAddress = new Uri("http://localhost/") };
            using OnlineServerSymbolsResolver resolver = new(client);

            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                dllCopy, new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = resolver });

            Assert.Single(handler.Requests);
            Assert.StartsWith("/download/symbols/", handler.Requests[0]);
            Assert.EndsWith("FFFFFFFF/" + Path.GetFileName(pdbPath), handler.Requests[0]);

            MethodDefinition method = assembly.MainModule.Types
                .Single(t => t.Name == nameof(EnricherTests)).Methods.Single(m => m.Name == nameof(Throw));
            Assert.NotEmpty(method.DebugInformation.SequencePoints.Where(p => p.StartLine != 0xFEEFEE));
        }
        finally
        {
            Directory.Delete(temp, true);
        }
    }

    [Fact]
    public void Resolver_ReturnsNull_WhenNotFoundAndNotThrowing()
    {
        string temp = Directory.CreateTempSubdirectory("enricher-").FullName;
        try
        {
            Assembly self = typeof(EnricherTests).Assembly;
            string dllCopy = Path.Combine(temp, Path.GetFileName(self.Location));
            File.Copy(self.Location, dllCopy);

            RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            using HttpClient client = new(handler) { BaseAddress = new Uri("http://localhost/") };
            using OnlineServerSymbolsResolver resolver = new(client, throwIfNoSymbol: false);

            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(
                dllCopy, new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = resolver });

            Assert.False(assembly.MainModule.HasSymbols);
            Assert.Equal(2, handler.Requests.Count); // portable key first, then the CodeView age
        }
        finally
        {
            Directory.Delete(temp, true);
        }
    }

    [Fact]
    public void Resolver_Throws_WhenNotFoundAndThrowing()
    {
        string temp = Directory.CreateTempSubdirectory("enricher-").FullName;
        try
        {
            Assembly self = typeof(EnricherTests).Assembly;
            string dllCopy = Path.Combine(temp, Path.GetFileName(self.Location));
            File.Copy(self.Location, dllCopy);

            RecordingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            using HttpClient client = new(handler) { BaseAddress = new Uri("http://localhost/") };
            using OnlineServerSymbolsResolver resolver = new(client);

            Assert.Throws<HttpRequestException>(() => AssemblyDefinition.ReadAssembly(
                dllCopy, new ReaderParameters { ReadSymbols = true, SymbolReaderProvider = resolver }));
        }
        finally
        {
            Directory.Delete(temp, true);
        }
    }
}