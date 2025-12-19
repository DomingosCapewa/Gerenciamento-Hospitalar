# Clinica - Atendimento de Consultas

## Endpoints

- Atender por horário: POST /api/clinica/consultas/atender-horario
- Atender por código: POST /api/clinica/consultas/atender-codigo

## Exemplos de entrada

Atendimento por horário (JSON):
# Clínica API

API clínica que consome consultas do Agendamentos via RabbitMQ e expõe endpoints para médicos e administração.

## URLs
- Direto (dev): http://localhost:7265
- Via API Gateway: http://localhost:8085/clinica
- Swagger: `/swagger`

## Autenticação e Roles
- Autenticação: JWT (Keycloak).
- Políticas: `Medico` para atender consultas; `Admin` para gestão de médicos; leituras públicas podem estar liberadas.

## Endpoints Principais
- GET `/api/medicos` – lista médicos (público).
- POST `/api/medicos` – cria médico (role `Admin`).
- POST `/api/atendimento/consultas/{id}/atender` – atende consulta (role `Medico`).

### Exemplo: atender consulta
Request
```json
{
	"observacoes": "Sintomas gripais",
	"solicitarExamesAvancados": false
}
```
Response 200
```json
{ "mensagem": "Consulta atendida com sucesso", "codigo": 1 }
```
Response 404
```json
{ "mensagem": "Consulta não encontrada" }
```

## Mensageria
- Consumer `ConsultaCriadaBackgroundService` da fila `clinica-consultas` para criação de consultas clínicas a partir de eventos de Agendamentos.

## Variáveis de Ambiente
- `ConnectionStrings__ClinicaDb`
- `RabbitMQ__Host|Port|User|Password`
- `Authentication__Authority|Audience|RequireHttpsMetadata`

## Executar
- Local: `dotnet run --project src/Clinica/Clinica.API/Clinica.API.csproj`
- Docker: `docker compose up -d --build`
