# Agendamentos API

API responsável por cadastrar e pesquisar consultas e exames. Gera eventos para Clínica e Cirúrgico via RabbitMQ.

## URLs
- Direto (dev): http://localhost:7264
- Via API Gateway: http://localhost:8085/agendamentos
- Swagger: `/swagger`

## Autenticação e Roles
- Autenticação: JWT (Keycloak). 
- Políticas: `Usuario` para cadastrar/pesquisar. `Admin` herda acesso.

## Endpoints Principais
- POST `/api/agendamentos/cadastro/consulta` – cria consulta; 409 se já existir consulta/exame para o CPF no mesmo dia/hora.
- GET `/api/agendamentos/pesquisar/consultas/{cpf}` – consulta por CPF.
- POST `/api/agendamentos/cadastro/exame` – cria exame; 409 em conflito de horário.

### Exemplo: criar consulta
Request
```json
{
	"paciente": {"nome":"João","cpf":"123.123.123-12","idade":30,"sexo":"M"},
	"horario":"2025-12-20T10:00:00",
	"medico":"pediatra"
}
```
Response 200
```json
{ "mensagem": "O medico pediatra de João foi marcado para 20/12/2025 10:00", "codigo": 1 }
```
Response 409
```json
{ "mensagem": "O paciente já possui agendamento no dia e hora informados." }
```

## Mensageria
- Consulta criada → fila `clinica-consultas` (evento `ConsultaCriadaEvent`).
- Exame criado → fila `cirurgico-procedimentos` (evento `ProcedimentoExameCriadoEvent`).

## Variáveis de Ambiente
- `ConnectionStrings__AgendamentosDb`
- `RabbitMQ__Host|Port|User|Password`
- `Authentication__Authority|Audience|RequireHttpsMetadata`

## Executar
- Local: `dotnet run --project src/Agendamentos/Agendamentos.API/Agendamentos.API.csproj`
- Docker: `docker compose up -d --build` (veja o compose em Infrastructure)

