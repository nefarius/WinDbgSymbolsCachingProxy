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
        TaskCompletionSource firstUploadEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstUpload = new(TaskCreationOptions.RunContinuationsAsynchronously);

        byte[] payload = [1, 2, 3];
        Task<SymbolUploadBatchResult> upload = SymbolUploadClient.UploadAsync(
            uploadUrl,
            [
                new SymbolUploadFile("foo.pdb", () => new MemoryStream(payload)),
                new SymbolUploadFile("foo.pdb", () => new MemoryStream(payload))
            ],
            async (url, form) =>
            {
                int invocation = Interlocked.Increment(ref requestCount);
                if (invocation == 1)
                {
                    firstUploadEntered.SetResult();
                    await releaseFirstUpload.Task;
                }

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

        await firstUploadEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(1, Volatile.Read(ref requestCount));
        releaseFirstUpload.SetResult();
        SymbolUploadBatchResult result = await upload.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(["foo.pdb", "foo.pdb"], requestedNames);
        Assert.Equal(2, requestCount);
        Assert.False(result.Succeeded);
        Assert.Equal(["foo.pdb: already exists", "foo.pdb: already exists"], result.Failures);
    }
}
