# Cirúrgico API

Microserviço responsável pelo gerenciamento de exames e procedimentos cirúrgicos. Processa solicitações de exames e controla o fluxo de procedimentos.

## Características

- Gestão de solicitações de exames
- Controle de status de procedimentos
- Gerenciamento de prioridades
- Geração de protocolos únicos
- Processamento assíncrono via RabbitMQ
- Documentação OpenAPI/Swagger

## URLs

| Ambiente | URL |
|----------|-----|
| Desenvolvimento direto | http://localhost:7108 |
| Via API Gateway | http://localhost:8085/cirurgico |
| Swagger UI | http://localhost:7108/swagger |

## Autenticação e Autorização

- **Autenticação**: JWT Bearer Token (Keycloak)
- **Roles necessárias**:
  - `Medico` - Solicitar e visualizar exames
  - `Admin` - Acesso total e gestão de procedimentos

## Endpoints

### Exames

#### POST `/api/exames/solicitacoes`
Registra uma nova solicitação de exame.

**Autorização**: Requer role `Medico`

**Request Body**:
```json
{
  "consultaId": 10,
  "cpf": "123.456.789-00",
  "exame": "Tomografia Computadorizada",
  "observacao": "Paciente com dores fortes na região abdominal"
}
```

**Response**: `200 OK`
```json
{
  "protocolo": "PRT-20251224-ABC123",
  "exame": "Tomografia Computadorizada",
  "cpf": "123.456.789-00",
  "consultaId": 10,
  "status": "Recebido",
  "observacao": "Paciente com dores fortes na região abdominal",
  "dataSolicitacao": "2025-12-24T10:30:00"
}
```

---

#### GET `/api/exames/solicitacoes/{protocolo}`
Busca uma solicitação de exame por protocolo.

**Autorização**: Requer role `Medico`

---

#### GET `/api/exames/solicitacoes/paciente/{cpf}`
Lista todas as solicitações de um paciente.

**Autorização**: Requer role `Medico`

---

#### PUT `/api/exames/solicitacoes/{protocolo}/status`
Atualiza o status de uma solicitação.

**Autorização**: Requer role `Admin`

**Request Body**:
```json
{
  "status": "EmAndamento",
  "observacao": "Exame agendado para amanhã"
}
```

**Status Possíveis**:
- `Recebido`
- `EmAndamento`
- `Concluido`
- `Cancelado`

## Mensageria (RabbitMQ)

### Eventos Consumidos

| Evento | Fila | Consumer | Descrição |
|--------|------|----------|------------|
| `ProcedimentoExameCriadoEvent` | `cirurgico-procedimentos` | `ExameSolicitadoConsumer` | Cria nova solicitação de exame |
| `ProcedimentoAtualizacaoEvent` | `cirurgico-procedimentos-atualizacoes` | `ProcedimentoAtualizacaoConsumer` | Atualiza prioridade e status |

### Consumers

**ExameSolicitadoConsumer**:
- Processa eventos de novos exames agendados
- Cria registro de solicitação com protocolo único
- Define status inicial como "Recebido"

**ProcedimentoAtualizacaoConsumer**:
- Atualiza informações de procedimentos existentes
- Gerencia alterações de prioridade
- Registra mudanças de status

## Arquitetura

```
Cirurgico.API/
├── Controllers/          # Endpoints REST
└── BackgroundServices/   # RabbitMQ consumers

Cirurgico.Application/
├── DTOs/                # Data Transfer Objects
└── Interfaces/          # Contratos de serviço

Cirurgico.Domain/
└── Entities/            # Entidades de domínio
    ├── Exame
    ├── SolicitacaoExame
    └── Procedimento

Cirurgico.Infrastructure/
├── Persistence/         # DbContext
├── Repositories/        # Acesso a dados
├── Messaging/           # RabbitMQ consumers
└── Migrations/          # Entity Framework migrations
```

## Configuração

### Variáveis de Ambiente

```bash
# Banco de Dados
ConnectionStrings__CirurgicoDb="server=localhost;port=3306;database=CirurgicoDb;user=root;password=root"

# RabbitMQ
RabbitMQ__Host="localhost"
RabbitMQ__Port="5672"
RabbitMQ__User="guest"
RabbitMQ__Password="guest"

# Autenticação
Authentication__Authority="http://localhost:8080/realms/hospital"
Authentication__Audience="hospital-api"
Authentication__RequireHttpsMetadata="false"
```

## Executar

### Com Docker
```bash
docker compose -f Infrastructure/docker-compose.yml up -d cirurgico-api
```

### Desenvolvimento Local
```bash
cd src/Cirurgico/Cirurgico.API
dotnet run
```

### Executar Migrations
```bash
cd src/Cirurgico/Cirurgico.Infrastructure
dotnet ef database update --startup-project ../Cirurgico.API
```

## Dependências Principais

- Microsoft.EntityFrameworkCore 9.0
- Pomelo.EntityFrameworkCore.MySql 9.0
- RabbitMQ.Client 6.8.1
- Microsoft.AspNetCore.Authentication.JwtBearer 9.0
