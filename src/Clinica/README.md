# Clinica - Atendimento de Consultas

## Endpoints

- Atender por horário: POST /api/clinica/consultas/atender-horario
- Atender por código: POST /api/clinica/consultas/atender-codigo

## Exemplos de entrada

Atendimento por horário (JSON):
# Clínica API

Microserviço responsável pelo gerenciamento de atendimentos clínicos e cadastro de médicos. Consome eventos de agendamentos e processa atendimentos médicos.

## Características

- Gestão de cadastro de médicos
- Processamento de consultas via eventos
- Atendimento de consultas clínicas
- Solicitação de exames avançados
- Integração assíncrona com Agendamentos
- Documentação OpenAPI/Swagger

## URLs

| Ambiente | URL |
|----------|-----|
| Desenvolvimento direto | http://localhost:7265 |
| Via API Gateway | http://localhost:8085/clinica |
| Swagger UI | http://localhost:7265/swagger |

## Autenticação e Autorização

- **Autenticação**: JWT Bearer Token (Keycloak)
- **Roles necessárias**:
  - `Usuario` - Consultar médicos e consultas
  - `Medico` - Atender consultas
  - `Admin` - Cadastrar e gerenciar médicos

## Endpoints

### Médicos

#### GET `/api/medicos`
Lista todos os médicos cadastrados.

**Autorização**: Público

**Response**:
```json
[
  {
    "id": 1,
    "nome": "Dr. Carlos Santos",
    "crm": "123456",
    "especialidade": "Cardiologia",
    "telefone": "(11) 98765-4321"
  }
]
```

---

#### POST `/api/medicos`
Cadastra um novo médico.

**Autorização**: Requer role `Admin`

**Request Body**:
```json
{
  "nome": "Dr. Carlos Santos",
  "crm": "123456",
  "especialidade": "Cardiologia",
  "telefone": "(11) 98765-4321"
}
```

**Response**: `201 Created`

### Consultas Clínicas

#### GET `/api/clinica/consultas`
Lista todas as consultas clínicas.

**Autorização**: Requer role `Medico`

---

#### GET `/api/clinica/consultas/{id}`
Busca uma consulta específica por ID.

**Autorização**: Requer role `Medico`

---

#### POST `/api/clinica/consultas/{id}/atender`
Registra o atendimento de uma consulta.

**Autorização**: Requer role `Medico`

**Request Body**:
```json
{
  "observacoes": "Paciente apresenta sintomas gripais. Prescrito repouso e medicação sintomática.",
  "solicitarExamesAvancados": true
}
```

**Responses**:
- `200 OK` - Atendimento registrado
  ```json
  {
    "mensagem": "Consulta atendida com sucesso",
    "codigo": 1
  }
  ```
- `404 Not Found` - Consulta não encontrada
  ```json
  {
    "mensagem": "Consulta não encontrada"
  }
  ```

**Comportamento**:
- Se `solicitarExamesAvancados = true`, publica evento para o serviço Cirúrgico

## Mensageria (RabbitMQ)

### Eventos Consumidos

| Evento | Fila | Descrição |
|--------|------|------------|
| `ConsultaCriadaEvent` | `clinica-consultas` | Cria registro de consulta clínica quando agendamento é feito |

### Consumers

**ConsultaCriadaBackgroundService**:
- Processa eventos de novas consultas agendadas
- Cria automaticamente o registro na base de dados da clínica
- Permite que o médico posteriormente atenda a consulta

## Arquitetura

```
Clinica.API/
├── Controllers/          # Endpoints REST
└── BackgroundServices/   # RabbitMQ consumers

Clinica.Application/
├── DTOs/                # Data Transfer Objects
└── Interfaces/          # Contratos de serviço

Clinica.Domain/
└── Entities/            # Entidades de domínio
    ├── Medico
    └── ConsultaClinica

Clinica.Infrastructure/
├── Persistence/         # DbContext
├── Repositories/        # Acesso a dados
├── Messaging/           # RabbitMQ consumers
└── Migrations/          # Entity Framework migrations
```

## Configuração

### Variáveis de Ambiente

```bash
# Banco de Dados
ConnectionStrings__ClinicaDb="server=localhost;port=3306;database=ClinicaDb;user=root;password=root"

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
docker compose -f Infrastructure/docker-compose.yml up -d clinica-api
```

### Desenvolvimento Local
```bash
cd src/Clinica/Clinica.API
dotnet run
```

### Executar Migrations
```bash
cd src/Clinica/Clinica.Infrastructure
dotnet ef database update --startup-project ../Clinica.API
```

## Dependências Principais

- Microsoft.EntityFrameworkCore 9.0
- Pomelo.EntityFrameworkCore.MySql 9.0
- RabbitMQ.Client 6.8.1
- Microsoft.AspNetCore.Authentication.JwtBearer 9.0
