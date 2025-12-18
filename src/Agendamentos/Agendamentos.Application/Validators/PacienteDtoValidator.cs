using FluentValidation;

namespace Agendamentos.Application.Validators;

public class PacienteDtoValidator : AbstractValidator<Agendamentos.Application.DTOs.PacienteDto>
{
    public PacienteDtoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3);
        RuleFor(x => x.CPF).NotEmpty().Matches("^[0-9]{11}$").WithMessage("CPF deve ter 11 dígitos numéricos");
        RuleFor(x => x.Idade).InclusiveBetween(0, 130);
        RuleFor(x => x.Sexo).NotEmpty();
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!).EmailAddress();
        });
        When(x => !string.IsNullOrWhiteSpace(x.Telefone), () =>
        {
            RuleFor(x => x.Telefone!).Matches(@"^[0-9+()\-\s]{8,20}$");
        });
        When(x => x.DataNascimento.HasValue, () =>
        {
            RuleFor(x => x.DataNascimento!.Value).LessThan(DateTime.UtcNow.Date);
        });
    }
}