# API de Agendamentos

Esta API é responsável por gerenciar os agendamentos de procedimentos e exames cirúrgicos em um sistema de gerenciamento hospitalar. Ela permite criar, atualizar, recuperar e cancelar agendamentos, além de integrar-se com outros serviços para processar exames e procedimentos.

## Funcionalidades
- **Criar Agendamento**: Permite criar um novo agendamento para um procedimento ou exame cirúrgico.
- **Atualizar Agendamento**: Permite atualizar os detalhes de um agendamento existente.
- **Recuperar Agendamentos**: Permite recuperar uma lista de agendamentos ou detalhes de um agendamento específico.
- **Cancelar Agendamento**: Permite cancelar um agendamento existente.
- **Integração com Serviços Externos**: Publica eventos para outros serviços, como o serviço cirúrgico, para processar exames e procedimentos agendados.

## Tecnologias Utilizadas
- .NET 9
- Entity Framework Core
- RabbitMQ para mensageria
- Swagger para documentação da API
## Endpoints Principais
- `POST /api/agendamentos`: Cria um novo agendamento.
- `PUT /api/agendamentos/{id}`: Atualiza um agendamento existente.
- `GET /api/agendamentos`: Recupera todos os agendamentos.
- `GET /api/agendamentos/{id}`: Recupera um agendamento específico.
- `DELETE /api/agendamentos/{id}`: Cancela um agendamento existente.

## Configuração e Execução
1. Clone o repositório do projeto.
2. Configure as variáveis de ambiente necessárias, como a string de conexão do banco de dados e as configurações do RabbitMQ.
3. Execute o projeto usando o .NET CLI ou uma IDE como o Visual Studio.
4. Acesse a documentação da API via Swagger em `http://localhost:{porta}/swagger` para testar os endpoints.

