using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using MudBlazor;

using WinDbgSymbolsCachingProxy.Core;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

[UsedImplicitly]
public partial class Upload
{
    private const string SymbolUploadHttpClientName = "SymbolUploadApi";

    [Inject]
    private IHttpClientFactory HttpClientFactory { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private MudFileUpload<IReadOnlyList<IBrowserFile>>? _fileUpload;
    private IReadOnlyList<IBrowserFile>? _files;
    private bool _force;
    private bool _uploading;

    private async Task UploadAsync()
    {
        if (_files is null || _files.Count == 0)
        {
            Snackbar.Add("Select at least one file.", Severity.Warning);
            return;
        }

        List<IBrowserFile> valid = [];
        List<string> skipped = [];

        foreach (IBrowserFile f in _files)
        {
            string ext = Path.GetExtension(f.Name);
            if (!SymbolUploadConstants.AllowedExtensions.Contains(ext))
            {
                skipped.Add(f.Name);
                continue;
            }

            valid.Add(f);
        }

        if (skipped.Count > 0)
        {
            Snackbar.Add($"Skipped (unsupported type): {string.Join(", ", skipped)}", Severity.Warning);
        }

        if (valid.Count == 0)
        {
            Snackbar.Add("No supported files to upload.", Severity.Warning);
            return;
        }

        _uploading = true;
        try
        {
            using MultipartFormDataContent form = new();
            foreach (IBrowserFile file in valid)
            {
                Stream stream = file.OpenReadStream(SymbolUploadConstants.MaxUploadBytesPerFile);
                StreamContent content = new(stream);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                form.Add(content, "symbol", file.Name);
            }

            HttpClient client = HttpClientFactory.CreateClient(SymbolUploadHttpClientName);
            client.BaseAddress = new Uri(Navigation.BaseUri);

            string query = _force ? "?force=true" : "";
            using HttpResponseMessage response =
                await client.PostAsync($"api/uploads/symbol{query}", form);

            string body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add(string.IsNullOrWhiteSpace(body) ? "Upload complete." : body.Trim('"'), Severity.Success);
                if (_fileUpload is not null)
                {
                    await _fileUpload.ClearAsync();
                }

                _files = null;
                return;
            }

            string message = TryParseErrorBody(body);
            Snackbar.Add(message, Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Upload failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _uploading = false;
        }
    }

    private static string TryParseErrorBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return "Upload failed.";
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;
            StringBuilder sb = new();

            if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty prop in errors.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement item in prop.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                sb.AppendLine(item.GetString());
                            }
                        }
                    }
                    else if (prop.Value.ValueKind == JsonValueKind.String)
                    {
                        sb.AppendLine(prop.Value.GetString());
                    }
                }
            }

            if (sb.Length > 0)
            {
                return sb.ToString().Trim();
            }

            if (root.TryGetProperty("detail", out JsonElement detail) &&
                detail.ValueKind == JsonValueKind.String)
            {
                return detail.GetString() ?? body;
            }

            if (root.TryGetProperty("message", out JsonElement message) &&
                message.ValueKind == JsonValueKind.String)
            {
                return message.GetString() ?? body;
            }

            if (root.TryGetProperty("title", out JsonElement title) && title.ValueKind == JsonValueKind.String)
            {
                return title.GetString() ?? body;
            }
        }
        catch
        {
            // fall through
        }

        return body;
    }
}
