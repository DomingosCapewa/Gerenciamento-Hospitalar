# Agendamentos API

Microserviço responsável pelo gerenciamento de agendamentos de consultas e exames médicos. Implementa validações de conflitos de horários e publica eventos para os demais serviços.

## Características

- Cadastro e consulta de agendamentos (consultas e exames)
- Validação de conflitos de horário
- Validação de CPF único por horário
- Publicação de eventos via RabbitMQ
- HATEOAS para navegação de recursos
- Documentação OpenAPI/Swagger

## URLs

| Ambiente | URL |
|----------|-----|
| Desenvolvimento direto | http://localhost:7264 |
| Via API Gateway | http://localhost:8085/agendamentos |
| Swagger UI | http://localhost:7264/swagger |

## Autenticação e Autorização

- **Autenticação**: JWT Bearer Token (Keycloak)
- **Roles necessárias**:
  - `Usuario` - Criar e consultar agendamentos
  - `Admin` - Acesso total a todos os endpoints

## Endpoints

### Consultas

#### POST `/api/agendamentos/cadastro/consulta`
Cria um novo agendamento de consulta.

**Autorização**: Requer role `Usuario`

**Request Body**:
```json
{
  "paciente": {
    "nome": "João Silva",
    "cpf": "123.456.789-00",
    "idade": 35,
    "sexo": "M"
  },
  "horario": "2025-12-24T14:00:00",
  "medico": "Dr. Carlos Santos"
}
```

**Responses**:
- `200 OK` - Consulta criada com sucesso
  ```json
  {
    "mensagem": "O medico Dr. Carlos Santos de João Silva foi marcado para 24/12/2025 14:00",
    "codigo": 1
  }
  ```
- `409 Conflict` - Já existe agendamento para o CPF no horário
  ```json
  {
    "mensagem": "O paciente já possui agendamento no dia e hora informados."
  }
  ```
- `400 Bad Request` - Dados inválidos

**Evento Publicado**: `ConsultaCriadaEvent` → fila `clinica-consultas`

---

#### GET `/api/agendamentos/pesquisar/consultas/{cpf}`
Busca consultas por CPF do paciente.

**Autorização**: Requer role `Usuario`

**Path Parameters**:
- `cpf` (string) - CPF do paciente

**Response**:
```json
[
  {
    "id": 1,
    "paciente": {
      "nome": "João Silva",
      "cpf": "123.456.789-00",
      "idade": 35,
      "sexo": "M"
    },
    "horario": "2025-12-24T14:00:00",
    "medico": "Dr. Carlos Santos",
    "status": "Agendada"
  }
]
```

### Exames

#### POST `/api/agendamentos/cadastro/exame`
Cria um novo agendamento de exame.

**Autorização**: Requer role `Usuario`

**Request Body**:
```json
{
  "paciente": {
    "nome": "Maria Oliveira",
    "cpf": "987.654.321-00",
    "idade": 42,
    "sexo": "F"
  },
  "horario": "2025-12-25T09:00:00",
  "tipoExame": "Hemograma Completo"
}
```

**Responses**:
- `200 OK` - Exame agendado
- `409 Conflict` - Conflito de horário

**Evento Publicado**: `ProcedimentoExameCriadoEvent` → fila `cirurgico-procedimentos`

## Mensageria (RabbitMQ)

### Eventos Publicados

| Evento | Fila | Consumidor | Descrição |
|--------|------|------------|------------|
| `ConsultaCriadaEvent` | `clinica-consultas` | Clínica API | Notifica criação de consulta |
| `ProcedimentoExameCriadoEvent` | `cirurgico-procedimentos` | Cirúrgico API | Notifica criação de exame |

### Estrutura dos Eventos

**ConsultaCriadaEvent**:
```json
{
  "consultaId": 1,
  "paciente": {
    "nome": "João Silva",
    "cpf": "123.456.789-00",
    "idade": 35,
    "sexo": "M"
  },
  "horario": "2025-12-24T14:00:00",
  "medico": "Dr. Carlos Santos"
}
```

## Regras de Negócio

1. **Validação de Conflito**: Um paciente não pode ter dois agendamentos no mesmo horário
2. **Validação de CPF**: CPF deve ser válido (algoritmo de validação implementado)
3. **Horário Futuro**: Não é permitido agendar para datas passadas
4. **Formato de Dados**: Nome, médico e tipo de exame não podem estar vazios

## Arquitetura

```
Agendamentos.API/
├── Controllers/          # Endpoints REST
├── HATEOAS/             # Links de navegação
└── Services/            # Lógica de publicação de eventos

Agendamentos.Application/
├── DTOs/                # Data Transfer Objects
├── Interfaces/          # Contratos de serviço
└── Validators/          # FluentValidation

Agendamentos.Domain/
└── Entities/            # Entidades de domínio

Agendamentos.Infrastructure/
├── Persistence/         # DbContext
├── Repositories/        # Acesso a dados
├── Messaging/           # RabbitMQ publishers
└── Migrations/          # Entity Framework migrations
```

## Configuração

### Variáveis de Ambiente

```bash
# Banco de Dados
ConnectionStrings__AgendamentosDb="server=localhost;port=3306;database=AgendamentosDb;user=root;password=root"

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

### appsettings.json

Ver [appsettings.json](Agendamentos.API/appsettings.json) para configuração completa.

## Executar

### Com Docker
```bash
docker compose -f Infrastructure/docker-compose.yml up -d agendamentos-api
```

### Desenvolvimento Local
```bash
cd src/Agendamentos/Agendamentos.API
dotnet run
```

### Executar Migrations
```bash
cd src/Agendamentos/Agendamentos.Infrastructure
dotnet ef database update --startup-project ../Agendamentos.API
```

## Testes

Ver arquivo [Agendamentos.API.http](Agendamentos.API/Agendamentos.API.http) para exemplos de requisições HTTP.

## Dependências Principais

- Microsoft.EntityFrameworkCore 9.0
- Pomelo.EntityFrameworkCore.MySql 9.0
- RabbitMQ.Client 6.8.1
- FluentValidation.AspNetCore 11.3.0
- Microsoft.AspNetCore.Authentication.JwtBearer 9.0

