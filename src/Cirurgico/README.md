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
- POST `/api/exames/solicitacoes` – registra solicitação de exame e retorna protocolo.

### Exemplo: solicitar exame
Request
```json
{
	"consultaId": 10,
	"cpf": "123.123.123-12",
	"exame": "Tomografia Computadorizada",
	"observacao": "Paciente com dores fortes"
}
```
Response 200
```json
{
	"protocolo": "PRT-...",
	"exame": "Tomografia Computadorizada",
	"cpf": "123.123.123-12",
	"consultaId": 10,
	"status": "Recebido",
	"observacao": "Paciente com dores fortes"
}
```

## Mensageria
- Consumers:
	- `ExameSolicitadoConsumer` – processa fila `cirurgico-procedimentos` (criação).
	- `ProcedimentoAtualizacaoConsumer` – processa fila `cirurgico-procedimentos-atualizacoes` (prioridade/status).

## Variáveis de Ambiente
- `ConnectionStrings__CirurgicoDb`
- `RabbitMQ__Host|Port|User|Password`
- `Authentication__Authority|Audience|RequireHttpsMetadata`

## Executar
- Local: `dotnet run --project src/Cirurgico/Cirurgico.API/Cirurgico.API.csproj`
- Docker: `docker compose -f Infrastructure/docker-compose.yml up -d --build`
