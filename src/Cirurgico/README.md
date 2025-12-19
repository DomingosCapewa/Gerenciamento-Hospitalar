# Cirúrgico API

API responsável por gerenciar solicitações de exames e procedimentos cirúrgicos. Consome eventos do Agendamentos.

## URLs
- Direto (dev): http://localhost:7108
- Via API Gateway: http://localhost:8085/cirurgico
- Swagger: `/swagger`

## Autenticação e Roles
- Autenticação: JWT (Keycloak) via gateway.
- Políticas: normalmente `Medico`/`Admin` para operação; algumas rotas podem estar públicas durante desenvolvimento.

## Endpoints Principais
- POST `/api/exames/solicitar` – registra solicitação de exame e retorna protocolo.

### Exemplo: solicitar exame
Request
```json
{
	"paciente": {"nome":"João","cpf":"123.123.123-12"},
	"tipo":"hemograma",
	"prioridade":"normal"
}
```
Response 200
```json
{ "protocolo": "EX-20251220-0001", "status": "recebido" }
```

## Mensageria
- Consumers:
	- `ExameSolicitadoBackgroundService` – processa fila `cirurgico-procedimentos` (criação).
	- `ProcedimentoAtualizacaoBackgroundService` – processa fila `cirurgico-procedimentos-atualizacoes` (prioridade/status).

## Variáveis de Ambiente
- `ConnectionStrings__CirurgicoDb`
- `RabbitMQ__Host|Port|User|Password`
- `Authentication__Authority|Audience|RequireHttpsMetadata`

## Executar
- Local: `dotnet run --project src/Cirurgico/Cirurgico.API/Cirurgico.API.csproj`
- Docker: `docker compose -f Infrastructure/docker-compose.yml up -d --build`
