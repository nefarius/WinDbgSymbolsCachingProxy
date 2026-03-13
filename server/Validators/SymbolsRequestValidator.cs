using FastEndpoints;

using FluentValidation;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Validators;

/// <summary>
///     Validates <see cref="SymbolsRequest"/>: ensures Symbol, SymbolKey and FileName are non-empty.
/// </summary>
public sealed class SymbolsRequestValidator : Validator<SymbolsRequest>
{
    /// <summary>
    ///     Configures validation rules for Symbol, SymbolKey and FileName.
    /// </summary>
    public SymbolsRequestValidator()
    {
        RuleFor(request => request.Symbol)
            .NotNull()
            .NotEmpty()
            .WithMessage("Symbol cannot be empty");

        RuleFor(request => request.SymbolKey)
            .NotNull()
            .NotEmpty()
            .WithMessage("Symbol key cannot be empty");

        RuleFor(request => request.FileName)
            .NotNull()
            .NotEmpty()
            .WithMessage("FileName cannot be empty");
    }
}