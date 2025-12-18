# Clinica - Atendimento de Consultas

## Endpoints

- Atender por horário: POST /api/clinica/consultas/atender-horario
- Atender por código: POST /api/clinica/consultas/atender-codigo

## Exemplos de entrada

Atendimento por horário (JSON):

{
	"PacienteCPF": "123.123.123-12",
	"Horario": "2025-12-18T14:00:00",
	"SintomasNomes": [
		"febre",
		"nausea",
		"dor de cabeça"
	]
}

Atendimento por código (JSON):

{
	"PacienteCPF": "123.123.123-12",
	"ConsultaId": 123,
	"SintomasNomes": [
		"febre",
		"nausea",
		"dor de cabeça"
	]
}

Observações:
- Também é possível informar os sintomas como objetos usando `Sintomas` com `SintomaId`, `Prioridade` e `Observacao`.
- Ao atender por código, quando enviado `DataHora` a consulta tem o horário atualizado antes do processamento.
