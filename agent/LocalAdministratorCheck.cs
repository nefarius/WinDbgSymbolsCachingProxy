using System.Security.Claims;

namespace HarvestingAgent;

/// <summary>
///     Decides whether the authenticated Windows caller belongs to the local Administrators group.
/// </summary>
internal static class LocalAdministratorCheck
{
    private const string BuiltinAdministratorsSid = "S-1-5-32-544";

    /// <summary>
    ///     A non-elevated administrator carries the Administrators SID as a deny-only group, which
    ///     <c>IsInRole</c> reports as false. Accept both group and deny-only SID claims so UAC does not lock admins out;
    ///     the check is by well-known SID so it works on localized Windows.
    /// </summary>
    public static bool IsLocalAdministrator(ClaimsPrincipal user)
    {
        return user.Claims.Any(c =>
            (c.Type == ClaimTypes.GroupSid || c.Type == ClaimTypes.DenyOnlySid) &&
            string.Equals(c.Value, BuiltinAdministratorsSid, StringComparison.OrdinalIgnoreCase));
    }
}
