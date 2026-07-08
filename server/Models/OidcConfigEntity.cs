using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     Singleton configuration document that controls OIDC authentication.
///     Stored in the <c>oidc-config</c> collection; the first document found is used.
/// </summary>
[Collection("oidc-config")]
public class OidcConfigEntity : Entity
{
    /// <summary>Whether OIDC is active. Requires a service restart after toggling.</summary>
    public bool Enabled { get; set; }

    /// <summary>OIDC authority URL (issuer), e.g. <c>https://sso.example.com/realms/myrealm</c>.</summary>
    public string? Authority { get; set; }

    /// <summary>OIDC client ID registered with the identity provider.</summary>
    public string? ClientId { get; set; }

    /// <summary>
    ///     OIDC client secret. Stored as plaintext; protect the MongoDB instance appropriately.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>OAuth2 scopes to request. Defaults to <c>openid profile email</c>.</summary>
    public List<string> Scopes { get; set; } = ["openid", "profile", "email"];

    /// <summary>Claim type used as the unique user subject identifier. Defaults to <c>sub</c>.</summary>
    public string SubjectClaimType { get; set; } = "sub";

    /// <summary>Claim type for the user's e-mail address. Defaults to <c>email</c>.</summary>
    public string EmailClaimType { get; set; } = "email";

    /// <summary>Claim type for the user's display name. Defaults to <c>name</c>.</summary>
    public string NameClaimType { get; set; } = "name";

    /// <summary>
    ///     Subject values or e-mail addresses that automatically receive the <c>Admin</c> role on first login.
    ///     Clear the list after the first admin has been provisioned.
    /// </summary>
    public List<string> BootstrapAdminIdentifiers { get; set; } = [];

    /// <summary>
    ///     Paths that always allow anonymous access even when OIDC is enabled.
    ///     Uses prefix matching (e.g. <c>/api/badges</c>).
    /// </summary>
    public List<string> AnonymousPathAllowlist { get; set; } = [];
}
