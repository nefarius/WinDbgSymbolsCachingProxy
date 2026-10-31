using FastEndpoints;

using FluentValidation;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Validators;

/// <summary>
///     Validates <see cref="SymbolsRequest"/>: ensures Symbol, SymbolKey and FileName are non-empty and
///     cannot be used to alter the upstream request path or query.
/// </summary>
public sealed class SymbolsRequestValidator : Validator<SymbolsRequest>
{
    private static readonly char[] ForbiddenSegmentChars = ['/', '\\', '?', '#', '%'];

    /// <summary>
    ///     Returns <c>true</c> when the value is a safe single URL path segment.
    /// </summary>
    internal static bool IsSafeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (value is "." or ".." || value.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        if (value.IndexOfAny(ForbiddenSegmentChars) >= 0)
        {
            return false;
        }

        foreach (char c in value)
        {
            if (char.IsControl(c))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Returns <c>true</c> when the value consists solely of hexadecimal digits.
    /// </summary>
    internal static bool IsHex(string? value)
    {
        return !string.IsNullOrEmpty(value) && value.All(Uri.IsHexDigit);
    }

    /// <summary>
    ///     Configures validation rules for Symbol, SymbolKey and FileName.
    /// </summary>
    public SymbolsRequestValidator()
    {
        RuleFor(request => request.Symbol)
            .NotEmpty()
            .WithMessage("Symbol cannot be empty")
            .Must(IsSafeSegment)
            .WithMessage("Symbol contains invalid characters");

        RuleFor(request => request.SymbolKey)
            .NotEmpty()
            .WithMessage("Symbol key cannot be empty")
            .Must(IsHex)
            .WithMessage("Symbol key must be hexadecimal");

        RuleFor(request => request.FileName)
            .NotEmpty()
            .WithMessage("FileName cannot be empty")
            .Must(IsSafeSegment)
            .WithMessage("FileName contains invalid characters");
    }
}
