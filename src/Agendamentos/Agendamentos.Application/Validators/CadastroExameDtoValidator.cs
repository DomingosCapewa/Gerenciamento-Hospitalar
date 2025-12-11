using FluentValidation;
using Agendamentos.Application.DTOs;

namespace Agendamentos.Application.Validators;

public class CadastroExameDtoValidator : AbstractValidator<CadastroExameDto>
{
    public CadastroExameDtoValidator()
    {
        RuleFor(x => x.Paciente).NotNull().SetValidator(new PacienteDtoValidator());
        RuleFor(x => x.Horario).NotEqual(default(DateTime)).WithMessage("Horário é obrigatório");
        RuleFor(x => x.Exame).NotEmpty().MinimumLength(3);
    }
}