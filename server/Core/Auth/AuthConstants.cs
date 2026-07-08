namespace WinDbgSymbolsCachingProxy.Core.Auth;

internal static class AuthConstants
{
    /// <summary>Claim type used to carry individual permission strings on the principal.</summary>
    internal const string PermissionClaimType = "permission";

    /// <summary>Name of the policy-forwarding scheme that auto-selects between Cookie and ApiKey.</summary>
    internal const string AutoSchemeName = "Auto";
}
