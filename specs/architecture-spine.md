# 🏛️ Architecture Spine: Encurtador de Links de Alta Performance (.NET 8 + SPA)

## 1. Stack Tecnológica & Comandos Oficiais (Single Source of Truth)

| Camada | Tecnologia / Versão | Comando de Build | Comando de Teste | Comando de Lint |
| :--- | :--- | :--- | :--- | :--- |
| **Backend** | C# .NET 8 (Clean Architecture) | `dotnet build` | `dotnet test` | `dotnet format --verify-no-changes` |
| **Frontend** | SPA Responsiva (HTML5 / TypeScript / Tailwind CSS / Fetch Client tipado em `src/Shortener.Api/wwwroot`) | `dotnet build` | `dotnet test --filter Category=UiIntegration` | `dotnet format --verify-no-changes` |
| **InfraLocal** | Docker Compose (PostgreSQL 16 + Redis 7 + RabbitMQ 3) | `docker compose build` | `docker compose ps` | N/A |

---

## 2. Árvore de Diretórios Canônica

```text
encurtador-links-csharp/
├── docker-compose.yml
├── README.md
├── Shortener.sln
├── docs/
│   └── architecture-decisions.md
├── specs/
│   ├── architecture-spine.md
│   ├── domain-rules.md
│   ├── frontend-spec.md
│   ├── traceability-matrix.md
│   ├── kanban-board.md
│   ├── contracts/
│   │   └── openapi.yaml
│   └── stories/
│       ├── US-001-plan.md
│       ├── US-002-plan.md
│       ├── US-003-plan.md
│       └── US-004-plan.md
├── src/
│   ├── Shortener.Domain/           # Entidades, Value Objects, Regras Puras (Base62)
│   ├── Shortener.Application/      # Casos de uso, DTOs, Interfaces de Repositório/Fila/Cache
│   ├── Shortener.Infrastructure/   # EF Core (Postgres), Redis Cache, RabbitMQ Producer/Consumer
│   └── Shortener.Api/              # Endpoints REST, Middlewares, wwwroot (Frontend SPA)
└── tests/
    └── Shortener.UnitTests/        # Testes de unidade e integração rápida
```

---

## 3. Regras Invariantes de Arquitetura (`AD-01..AD-05`)

> **Atenção Subagentes:** Violar qualquer invariante abaixo resulta em reprovação imediata no Code Review Gate.

### `AD-01`: Isolamento de Domínio e Inversão de Dependência (Clean Architecture)
- **Binds:** `src/Shortener.Domain/`, `src/Shortener.Application/`
- **Rule:** A camada de Domínio não possui dependências externas ou referências a frameworks de infraestrutura (EF Core, Redis, RabbitMQ). Casos de uso na `Application` dependem apenas de abstrações (`IRepository`, `ICacheService`, `IMessagePublisher`).
- **Prevents:** Acoplamento direto de Controllers/Endpoints com bancos de dados concretos ou bibliotecas de mensageria.

### `AD-02`: Consumo Tipado e Centralizado no Frontend
- **Binds:** `src/Shortener.Api/wwwroot/`
- **Rule:** Componentes de interface nunca realizam chamadas HTTP arbitrárias. Toda comunicação com a API é centralizada em módulo de cliente (`apiClient`) aderente aos schemas de `specs/contracts/openapi.yaml`.
- **Prevents:** Divergência de parâmetros, URLs hardcoded soltas em marcação HTML/JS.

### `AD-03`: Tratamento Padronizado de Erros (RFC 7807 Problem Details)
- **Binds:** `src/Shortener.Api/`, `src/Shortener.Api/wwwroot/`
- **Rule:** O Backend retorna erros utilizando RFC 7807 (`application/problem+json`) com campos `title`, `status`, `detail` e dicionário `errors`. O Frontend intercepta e exibe alertas contextuais sem falha silenciosa.
- **Prevents:** Exceptions 500 sem tratamento vazando stacktrace ou formulários sem feedback visual de validação.

### `AD-04`: Redirecionamento Ultrarrápido e Métricas Assíncronas
- **Binds:** `src/Shortener.Application/`, `src/Shortener.Infrastructure/`
- **Rule:** O fluxo de redirecionamento HTTP 302 consulta prioritariamente o Redis (Cache-Aside) e despacha a métrica de clique para o RabbitMQ de forma assíncrona (`fire-and-forget` para a requisição HTTP). A persistência relacional do clique é executada por um `BackgroundService`.
- **Prevents:** Operações síncronas de escrita em banco durante o redirecionamento que gerem contenção de conexões (*Row Lock Contention*).

### `AD-05`: Invariante Contract-First & Zero Endpoints Órfãos
- **Binds:** `specs/contracts/openapi.yaml`, `specs/traceability-matrix.md`
- **Rule:** Todo endpoint `user-facing` possui tela correspondente mapeada com os 4 estados visuais: **Loading**, **Empty**, **Error**, **Success**.
- **Prevents:** Criação de endpoints de API que não possam ser acionados ou visualizados pelo usuário final.
