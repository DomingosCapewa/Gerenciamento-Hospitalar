# Sistema de Gerenciamento Hospitalar

Sistema completo de gerenciamento hospitalar baseado em microserviços, desenvolvido com .NET 9 e arquitetura orientada a eventos.

## Visão Geral

Plataforma modular para gestão hospitalar composta por três microserviços independentes:
- **Agendamentos**: Gerenciamento de consultas e exames
- **Clínica**: Atendimento clínico e gestão de médicos
- **Cirúrgico**: Solicitações e controle de procedimentos cirúrgicos

### Arquitetura
- Microserviços REST (ASP.NET Core 9.0)
- Clean Architecture com Domain-Driven Design
- Persistência com Entity Framework Core + MySQL
- Mensageria assíncrona via RabbitMQ
- Autenticação e autorização com Keycloak (JWT)
- API Gateway (YARP) para roteamento centralizado
- Frontend Angular 19 (em desenvolvimento)
- Containerização com Docker

## Serviços e Portas

| Serviço | Porta | URL |
|---------|-------|-----|
| API Gateway | 8085 | http://localhost:8085 |
| Agendamentos API | 7264 | http://localhost:7264 |
| Clínica API | 7265 | http://localhost:7265 |
| Cirúrgico API | 7108 | http://localhost:7108 |

### Pré-requisitos
- Docker Desktop 4.0+
- .NET SDK 9.0+ (para desenvolvimento local)
- Node.js 20+ e npm (para o frontend)

### Execução com Docker (Recomendado)

1. **Iniciar toda a infraestrutura:**
   ```bash
   docker compose -f Infrastructure/docker-compose.yml up -d --build
   ```
Configuração do Keycloak

### Setup Automático (via Import)
O arquivo [Infrastructure/keycloak/hospital-realm.json](Infrastructure/keycloak/hospital-realm.json) contém a configuração completa do realm.

### Setup Manual

1. **Criar Realm:**
   - Acesse http://localhost:8080
   - Login: admin/admin
   - Criar novo realm: `hospital`

2. **Configurar Client:**
   - Nome: `hospital-api`
   - Access Type: `public`
   - Valid Redirect URIs: `http://localhost:4200/*`
   - Web Origins: `*`

3. **Criar Roles:**
   - Realm Roles: `Usuario`, `Medico`, `Admin`

4. **Criar Usuários de Teste:**
   ```
   Usuário Comum:
   - Username: usuario
   - Password: senha123
   - Role: Usuario

   Médiando a API

### 1. Obter Token de Autenticação

```bash
curl -X POST "http://localhost:8080/realms/hospital/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=hospital-api" \
  -d "username=usuario" \
  -d "password=senha123"
```

Copie o `access_token` da resposta.

### 2. Criar Agendamento de Consulta

```bash
curl -X POST "http://localhost:8085/agendamentos/api/agendamentos/cadastro/consulta" \
  -H "AuthorizationLocal

### Backend (.NET)

1. **Abrir solução:**
   ```bash
   cd src
   dotnet restore HospitalSolution.sln
   ```

2. **Iniciar infraestrutura (apenas bancos, RabbitMQ e Keycloak):**
   ```bash
   docker compose -f Infrastructure/docker-compose.yml up -d mysql_agendamentos mysql_clinica mysql_cirurgico rabbitmq keycloak
   ```

3. **Executar migrations:**
   ```bash
   cd src/Agendamentos/Agendamentos.Infrastructure
   dotnet ef database update --startup-project ../Agendamentos.API

   cd ../../Clinica/Clinica.Infrastructure
   dotnet ef database update --startup-project ../Clinica.API

   cd ../../Cirurgico/Cirurgico.Infrastructure
   dotnet ef database update --startup-project ../Cirurgico.API
   ```

4. **Executar APIs:**
   ```bash
   dotnet run --project src/Agendamentos/Agendamentos.API
   dotnet run --project src/Clinica/Clinica.API
   dotnet run --project src/Cirurgico/Cirurgico.API
   dotnet run --project src/api-gateway/Gateway.csproj
   ```

### Frontend (Angular)

```bash
cd Gerenciamento-Hospiltar_Frontend
npm install
npm start
```

Acesse: http://localhost:4200

### 3. Consultar Agendamentos

## Stack Tecnológica

### Backend
- **.NET 9.0** - Framework principal
- **ASP.NET Core** - APIs REST
- **Entity Framework Core 9.0** - ORM
- **MySQL 8.0** - Banco de dados relacional
- **RabbitMQ** - Message broker
- **Keycloak** - Identity and Access Management
- **YARP** - API Gateway (reverse proxy)
- **FluentValidation** - Validação de DTOs
- **Swashbuckle** - Documentação OpenAPI/Swagger

### Frontend
- **Angular 19** - Framework SPA
- **TypeScript** - Linguagem
- **RxJS** - Programação reativa
- **Angular Material** (planejado)

### DevOps
- **Docker & Docker Compose** - Containerização
- **GitHub Actions** (CI/CD planejado)

## Estrutura do Projeto

```
├── src/
│   ├── HospitalSolution.sln
│   ├── Agendamentos/              # Microserviço de Agendamentos
│   │   ├── Agendamentos.API/
│   │   ├── Agendamentos.Application/
│   │   ├── Agendamentos.Domain/
│   │   └── Agendamentos.Infrastructure/
│   ├── Clinica/                   # Microserviço Clínica
│   ├── Cirurgico/                 # Microserviço Cirúrgico
│   ├── Shared.Contracts/          # Contratos compartilhados
│   └── api-gateway/               # API Gateway
├── Infrastructure/
│   ├── docker-compose.yml
│   ├── keycloak/
│   └── rabbitmq/
└── Gerenciamento-Hospiltar_Frontend/  # Frontend Angular
```

## Troubleshooting

### Erros de Autenticação (401/403)
- Verifique se o token JWT está válido
- Confirme que o usuário possui as roles necessárias no Keycloak
- Verifique o `Audience` do token: deve ser `hospital-api`

### Problemas com Mensageria
- Acesse RabbitMQ Management: http://localhost:15672
- Verifique se as filas estão criadas:
  - `clinica-consultas`
  - `cirurgico-procedimentos`
  - `cirurgico-procedimentos-atualizacoes`
- Confirme que os consumers estão ativos

### Erros de Conexão com Banco de Dados
- Aguarde 30-60 segundos após `docker compose up` para os bancos iniciarem
- Verifique logs: `docker compose -f Infrastructure/docker-compose.yml logs mysql_agendamentos`
- Confirme as connection strings no docker-compose.yml

### Portas em Uso
```bash
# Windows
netstat -ano | findstr :8085
# Linux/Mac
lsof -i :8085
```

## Documentação Adicional

- [Agendamentos API](src/Agendamentos/README.md)
- [Clínica API](src/Clinica/README.md)
- [Cirúrgico API](src/Cirurgico/README.md)

## Contribuição

Este é um projeto acadêmico/demonstrativo. Sugestões e melhorias são bem-vindas.

## Testes rápidos (via Gateway)
1. Obtenha um token (Password Grant) do Keycloak:
   - POST `http://localhost:8080/realms/hospital/protocol/openid-connect/token`
   - `Content-Type: application/x-www-form-urlencoded`
   - Body: `grant_type=password&client_id=hospital-api&username=<user>&password=<pass>`
   - Copie `access_token` do retorno.
2. Chame o Agendamentos criando uma consulta:
   - POST `http://localhost:8085/agendamentos/api/agendamentos/cadastro/consulta`
   - Header: `Authorization: Bearer <token>`
   - Body (exemplo):
     ```json
     {
       "paciente": {"nome":"João","cpf":"123.123.123-12","idade":30,"sexo":"M"},
       "horario":"2025-12-20T10:00:00",
       "medico":"pediatra"
     }
     ```
3. Verifique a criação/consumo na Clínica (fila `clinica-consultas`).

## Desenvolvimento local (SDK)
1. Restaure a solução: abra [src/HospitalSolution.sln](src/HospitalSolution.sln) no Visual Studio/VS Code.
2. Suba apenas infraestrutura com Docker (opcional):
   ```bash
   docker compose -f Infrastructure/docker-compose.yml up -d mysql_agendamentos mysql_clinica mysql_cirurgico rabbitmq keycloak
   ```
3. Rode cada API:
   ```bash
   dotnet run --project src/Agendamentos/Agendamentos.API/Agendamentos.API.csproj
   dotnet run --project src/Clinica/Clinica.API/Clinica.API.csproj
   dotnet run --project src/Cirurgico/Cirurgico.API/Cirurgico.API.csproj
   dotnet run --project src/api-gateway/api-gateway.csproj
   ```

## Documentação por serviço
- [src/Agendamentos/README.md](src/Agendamentos/README.md)
- [src/Clinica/README.md](src/Clinica/README.md)
- [src/Cirurgico/README.md](src/Cirurgico/README.md)

## Troubleshooting
- HTTP/HTTPS: nos containers `DisableHttpsRedirection=true` mantém HTTP simples; em dev local pode usar HTTPS. 
- 401/403 pelo Gateway: confira roles do usuário no Keycloak e o `Audience` (`hospital-api`).
- Filas: RabbitMQ em http://localhost:15672 (guest/guest). Verifique que consumidores das APIs estão “running”.
- Conexões MySQL: variáveis definidas no [Infrastructure/docker-compose.yml](Infrastructure/docker-compose.yml). Aguarde bancos subirem antes de subir as APIs.
