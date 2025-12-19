# Sistema de Gerenciamento Hospitalar
Monorepo .NET para um sistema hospitalar com três serviços (Agendamentos, Clínica e Cirúrgico), API Gateway, autenticação via Keycloak, bancos MySQL por serviço e mensageria RabbitMQ.

## Arquitetura
- Serviços REST (ASP.NET Core) com EF Core + MySQL.
- Integração assíncrona via RabbitMQ.
- Autenticação JWT (Keycloak) com roles (`Usuario`, `Medico`, `Admin`).
- API Gateway (YARP) roteando e exigindo autorização.

## Serviços e Portas (host)
- API Gateway: http://localhost:8085
- Agendamentos API: http://localhost:7264
- Clínica API: http://localhost:7265
- Cirúrgico API: http://localhost:7108
- Keycloak: http://localhost:8080
- RabbitMQ Management: http://localhost:15672 (guest/guest)

## Como executar (Docker)
1. Pré-requisitos: Docker Desktop e Docker Compose.
2. Suba toda a stack:
   ```bash
   docker compose -f Infrastructure/docker-compose.yml up -d --build
   ```
3. Acesse o Keycloak em http://localhost:8080 (admin/admin) e configure conforme abaixo.
4. Teste pelos endpoints do Gateway (ver seção Testes Rápidos).

## Keycloak (setup rápido)
1. Crie o Realm `hospital`.
2. Crie o Client `hospital-api` (confidencial ou público – para backend, público costuma ser suficiente para validação de token):
   - Access Type: Public.
   - Valid Redirect URIs: pode deixar vazio por enquanto (sem frontend).
   - Web Origins: `*` (ambiente dev).
3. Crie Realm Roles: `Usuario`, `Medico`, `Admin`.
4. Crie usuários e atribua roles conforme necessidade.

Observação: as APIs mapeiam `realm_access.roles` e `resource_access.hospital-api.roles` para claims de role do ASP.NET.

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

## Tecnologias
- .NET 9, ASP.NET Core, EF Core (Pomelo MySQL)
- RabbitMQ
- Keycloak (JWT)
- YARP (API Gateway)
