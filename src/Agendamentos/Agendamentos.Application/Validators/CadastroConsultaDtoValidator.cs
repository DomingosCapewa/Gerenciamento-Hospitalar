using FluentValidation;
using Agendamentos.Application.DTOs;

namespace Agendamentos.Application.Validators;

public class CadastroConsultaDtoValidator : AbstractValidator<CadastroConsultaDto>
{
    public CadastroConsultaDtoValidator()
    {
        RuleFor(x => x.Paciente).NotNull().SetValidator(new PacienteDtoValidator());
        RuleFor(x => x.Horario).NotEqual(default(DateTime)).WithMessage("Horário é obrigatório");
        RuleFor(x => x.Medico).NotEmpty().MinimumLength(3);
    }
}