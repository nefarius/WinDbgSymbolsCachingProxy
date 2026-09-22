using System.Net.Http.Headers;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

internal sealed record SymbolUploadFile(string Name, Func<Stream> OpenContent);

internal sealed record SymbolUploadBatchResult(IReadOnlyList<string> Failures)
{
    public bool Succeeded => Failures.Count == 0;
}

internal static class SymbolUploadClient
{
    public static async Task<SymbolUploadBatchResult> UploadAsync(
        string uploadUrl,
        IEnumerable<SymbolUploadFile> files,
        Func<string, MultipartFormDataContent, Task<HttpResponseMessage>> postAsync)
    {
        List<string> failures = [];

        foreach (SymbolUploadFile file in files)
        {
            try
            {
                using StreamContent content = new(file.OpenContent());
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                using MultipartFormDataContent form = new();
                form.Add(content, "symbol", file.Name);

                using HttpResponseMessage response = await postAsync(uploadUrl, form);
                string body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    failures.Add($"{file.Name}: {Upload.TryParseErrorBody(body)}");
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{file.Name}: {ex.Message}");
            }
        }

        return new SymbolUploadBatchResult(failures);
    }
}
