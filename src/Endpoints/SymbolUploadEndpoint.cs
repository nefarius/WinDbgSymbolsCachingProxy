using FastEndpoints;

using Smx.PDBSharp;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class SymbolUploadEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/uploads/symbol");
        AllowFileUploads(dontAutoBindFormData: true);
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await foreach (var section in FormFileSectionsAsync(ct))
        {
            if (section is null)
            {
                continue;
            }

            using var ms = new MemoryStream();

            await section.Section.Body.CopyToAsync(ms, 1024 * 64, ct);

            // TODO: finish me
        }

        await SendOkAsync("upload complete!", ct);
    }
}