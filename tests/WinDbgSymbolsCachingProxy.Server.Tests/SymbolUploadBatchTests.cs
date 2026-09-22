using System.Net;
using System.Net.Http.Headers;

using WinDbgSymbolsCachingProxy.Components.Pages;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

public sealed class SymbolUploadBatchTests
{
    [Fact]
    public async Task Sequential_uploads_keep_same_filenames_and_aggregate_failures()
    {
        List<string> requestedNames = [];
        int requestCount = 0;
        string uploadUrl = Upload.BuildUploadUrl("https://symbols.example/", force: true);

        byte[] payload = [1, 2, 3];
        SymbolUploadBatchResult result = await SymbolUploadClient.UploadAsync(
            uploadUrl,
            [
                new SymbolUploadFile("foo.pdb", new MemoryStream(payload)),
                new SymbolUploadFile("foo.pdb", new MemoryStream(payload))
            ],
            async (url, form) =>
            {
                requestCount++;
                Assert.Equal(uploadUrl, url);
                HttpContent fileContent = Assert.Single(form);
                ContentDispositionHeaderValue? header = fileContent.Headers.ContentDisposition;
                Assert.NotNull(header);
                Assert.Equal("symbol", header.Name);
                Assert.Equal("foo.pdb", header.FileName);
                requestedNames.Add(header.FileName!);

                byte[] bytes = await fileContent.ReadAsByteArrayAsync();
                Assert.Equal(payload, bytes);
                return new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent("""{"detail":"already exists"}""")
                };
            });

        Assert.Equal(["foo.pdb", "foo.pdb"], requestedNames);
        Assert.Equal(2, requestCount);
        Assert.False(result.Succeeded);
        Assert.Equal(["foo.pdb: already exists", "foo.pdb: already exists"], result.Failures);
    }
}
