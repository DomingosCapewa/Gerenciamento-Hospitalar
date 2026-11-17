# Sistema de Gerenciamento Hospitalar

Plataforma hospitalar baseada em microserviços, desenvolvida com .NET 9, mensageria assíncrona e autenticação centralizada.

## Visão geral

O sistema é composto por:

- **Agendamentos:** consultas e exames;
- **Clínica:** atendimentos e médicos;
- **Cirúrgico:** procedimentos cirúrgicos;
- **API Gateway:** entrada única usando YARP;
- **Frontend:** aplicação Angular em desenvolvimento.

## Arquitetura

- ASP.NET Core 9 e APIs REST;
- Clean Architecture e conceitos de Domain-Driven Design;
- Entity Framework Core com MySQL;
- RabbitMQ para comunicação assíncrona;
- Keycloak para autenticação e autorização com JWT;
- Docker Compose para a infraestrutura.

## Requisitos

- Docker Desktop;
- .NET SDK 9.0 ou superior;
- Node.js 20 ou superior, para o frontend.

## Executando com Docker

Na raiz do repositório, inicie toda a infraestrutura:

```bash
docker compose -f Infrastructure/docker-compose.yml up -d --build
```

O arquivo `Infrastructure/keycloak/hospital-realm.json` contém a configuração do realm usada pelo projeto. Após a inicialização, verifique o arquivo `Infrastructure/docker-compose.yml` para as credenciais locais e serviços disponíveis.

Serviços principais:

| Serviço | Porta |
| --- | ---: |
| API Gateway | 8085 |
| Keycloak | 8080 |
| RabbitMQ Management | 15672 |
| Frontend | 4200 |

## Executando localmente

Para executar apenas a infraestrutura:

```bash
docker compose -f Infrastructure/docker-compose.yml up -d \
  mysql_agendamentos mysql_clinica mysql_cirurgico rabbitmq keycloak
```

Restaure a solução:

```bash
dotnet restore src/HospitalSolution.sln
```

Aplique as migrações:

```bash
dotnet ef database update \
  --project src/Agendamentos/Agendamentos.Infrastructure \
  --startup-project src/Agendamentos/Agendamentos.API

dotnet ef database update \
  --project src/Clinica/Clinica.Infrastructure \
  --startup-project src/Clinica/Clinica.API

dotnet ef database update \
  --project src/Cirurgico/Cirurgico.Infrastructure \
  --startup-project src/Cirurgico/Cirurgico.API
```

Em terminais separados, execute as APIs:

```bash
dotnet run --project src/Agendamentos/Agendamentos.API
dotnet run --project src/Clinica/Clinica.API
dotnet run --project src/Cirurgico/Cirurgico.API
dotnet run --project src/api-gateway
```

Para executar o frontend:

```bash
cd Gerenciamento-Hospiltar_Frontend
npm install
npm start
```

## Teste rápido pelo Gateway

Obtenha um token no Keycloak usando um usuário configurado no realm:

```bash
curl -X POST \
  "http://localhost:8080/realms/hospital/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=hospital-api" \
  -d "username=<usuario>" \
  -d "password=<senha>"
```

Use o `access_token` retornado para criar um agendamento:

```bash
curl -X POST \
  "http://localhost:8085/agendamentos/api/agendamentos/cadastro/consulta" \
  -H "Authorization: Bearer <access_token>" \
  -H "Content-Type: application/json" \
  -d '{
    "paciente": {
      "nome": "João da Silva",
      "cpf": "123.123.123-12",
      "idade": 30,
      "sexo": "M"
    },
    "horario": "2025-12-20T10:00:00",
    "medico": "pediatra"
  }'
```

## Estrutura

```text
src/
├── HospitalSolution.sln
├── Agendamentos/
├── Clinica/
├── Cirurgico/
├── Shared.Contracts/
└── api-gateway/
Infrastructure/
├── docker-compose.yml
└── keycloak/
Gerenciamento-Hospiltar_Frontend/
```

## Documentação por serviço

- [Agendamentos](src/Agendamentos/README.md)
- [Clínica](src/Clinica/README.md)
- [Cirúrgico](src/Cirurgico/README.md)

## Troubleshooting

- **401/403:** confirme o token, o realm e as roles do usuário no Keycloak.
- **RabbitMQ:** verifique as filas e os consumers em `http://localhost:15672`.
- **Banco indisponível:** aguarde os containers ficarem saudáveis e confira os logs do serviço correspondente.
- **Portas ocupadas:** altere as portas no Docker Compose ou finalize o processo que as utiliza.
