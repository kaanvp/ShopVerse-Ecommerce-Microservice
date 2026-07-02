using FluentValidation;

namespace ShopVerse.Catalog.Application.Commands.DeleteProductCommand
{
    public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Ürün ID zorunludur.");
        }
    }
}
