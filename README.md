# Encurtador de Links de Alta Performance (.NET 8 + SPA)

Sistema resiliente e de latência ultrabaixa para encurtamento de URLs, redirecionamento com cache e processamento assíncrono de métricas analíticas.

---

## 🚀 Arquitetura & Stack

* **Backend:** C# (.NET 8) estruturado sob Clean Architecture (`Domain`, `Application`, `Infrastructure`, `Api`).
* **Frontend:** Single Page Application (HTML5, Tailwind CSS, JavaScript Vanilla com cliente tipado) servida diretamente via Web API em `src/Shortener.Api/wwwroot`.
* **Codificação:** Algoritmo alfanumérico Base62 de alta densidade (`[0-9, a-z, A-Z]`).
* **Cache & Mensageria:** Redis 7 (Cache-Aside) e RabbitMQ 3 (Painel de Gestão em `:15672`).
* **Banco de Dados:** PostgreSQL 16.

---

## 🛠️ Como Executar Localmente

### 1. Subir Infraestrutura (Postgres, Redis, RabbitMQ)
```bash
docker compose up -d
```

### 2. Executar a Aplicação Web & API
```bash
dotnet run --project src/Shortener.Api
```
* **Frontend SPA:** Abra `http://localhost:5030` no navegador.
* **Swagger OpenAPI:** Acesse `http://localhost:5030/swagger`.

### 3. Executar os Testes Automatizados (TDD)
```bash
dotnet test
```

---

## 📋 Endpoints da API

| Método | Rota | Descrição | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/urls` | Cria um novo link encurtado com Base62 | `201`, `400`, `500` |
| `GET` | `/{shortCode}` | Redireciona para a URL original (HTTP 302) | `302`, `404`, `410` |
| `GET` | `/api/v1/urls/{shortCode}/stats` | Retorna métricas consolidadas de cliques | `200`, `404` |
| `DELETE` | `/api/v1/urls/{shortCode}` | Desativa manualmente um link encurtado | `204`, `404` |

---

## 📚 Documentação Técnica e Especificações
* Decisões Arquiteturais: [`docs/architecture-decisions.md`](./docs/architecture-decisions.md)
* Espinha Arquitetural: [`specs/architecture-spine.md`](./specs/architecture-spine.md)
* Contrato OpenAPI: [`specs/contracts/openapi.yaml`](./specs/contracts/openapi.yaml)
* Matriz de Rastreabilidade Full-Stack: [`specs/traceability-matrix.md`](./specs/traceability-matrix.md)
* Quadro Kanban: [`specs/kanban-board.md`](./specs/kanban-board.md)
