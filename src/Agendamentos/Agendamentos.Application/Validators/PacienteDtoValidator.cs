using FluentValidation;
using Agendamentos.Application.DTOs;

namespace Agendamentos.Application.Validators;

public class PacienteDtoValidator : AbstractValidator<PacienteDto>
{
    public PacienteDtoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MinimumLength(3);
        RuleFor(x => x.CPF).NotEmpty().Matches("^[0-9]{11}$").WithMessage("CPF deve ter 11 dígitos numéricos");
        RuleFor(x => x.Idade).InclusiveBetween(0, 130);
        RuleFor(x => x.Sexo).NotEmpty();
    }
}