# Sistema de Gerenciamento Hospitalar
Este repositório contém um sistema de gerenciamento hospitalar modular, desenvolvido em .NET, que abrange funcionalidades essenciais para a administração eficiente de clínicas e hospitais. O sistema é dividido em vários módulos, cada um responsável por uma área específica, como agendamentos, clínica e cirúrgico.
 
# Tecnologias Utilizadas
- .NET 9
- ASP.NET Core
- Entity Framework Core
- MySQL
- RabbitMQ
- Docker

# Como Executar o Projeto
1. Clone o repositório:
   ```bash
   git clone

2. Navegue até o diretório do projeto:
   ```bash
   cd Gerenciamento-hospitalar
   ```
3. Certifique-se de ter o Docker e o Docker Compose instalados em sua máquina.
4. Inicie os serviços necessários (MySQL, RabbitMQ, Keycloak) usando o Docker Compose:
   ```bash
   docker-compose -f src/infrastructure/docker-compose.yml up -d
   ```
5. Navegue até o diretório `src` e abra a solução `HospitalSolution.sln` em sua IDE favorita (Visual Studio, VS Code, etc.).
6. Restaure as dependências e construa o projeto.

7. Execute os projetos individualmente ou todos juntos conforme necessário.

8. Acesse as APIs através dos endpoints configurados (verifique os controladores para detalhes).

9. Utilize ferramentas como Postman ou Swagger UI para testar as APIs.

# Estrutura do projeto 
````
/Gerenciamento-hospitalar 
   /src
      /Agendamentos.API
      /Clinica.API
      /Cir
# Estrutura do projeto 
````
/Gerenciamento-hospitalar 
   /src
      /Agendamentos
         /Agendamentos.API
         /Agendamentos.Application
         /Agendamentos.Domain
         /Agendamentos.Infrastructure
      /Cirurgico
         /Cirurgico.API
         /Cirurgico.Application
         /Cirurgico.Domain
         /Cirurgico.Infrastructure
      /Clinica
         /Clinica.API
         /Clinica.Application
         /Clinica.Domain
         /Clinica.Infrastructure
      /Gateway
      /infrastructure	
         /mysql
         /rabbitmq
         /keycloak
         docker-compose.yml
      /Shared.Contracts
      HospitalSolution.sln
   README.md
   ````urgico.API
      /Gateway
   /infrastructure	
      /mysql
      /rabbitmq
      /keycloak
   docker-compose.yml
   README.md
   ````
