using FastEndpoints;

using FluentValidation;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Validators;

public sealed class SymbolsRequestValidator : Validator<SymbolsRequest>
{
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