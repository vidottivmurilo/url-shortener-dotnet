---
id: "US-002"
epic: "EPIC-01"
title: "Redirecionamento HTTP 302 com Cache Redis e Mensageria RabbitMQ"
priority: "P1"
story_points: 3
scope: "full-stack"
covers_requirements: ["FR-002"]
contract_operations: ["resolveShortUrl"]
status: "ready-for-dev"
baseline_revision: ""
review_loop_iteration: 0
---

<frozen-after-approval reason="human-and-analyst-owned intent — Dev and QA agents MUST NOT modify this block">

## 1. User Story & Valor de Negócio
- **Como** um visitante acessando uma URL curta,
- **Quero** ser redirecionado instantaneamente (HTTP 302) para a URL original,
- **Para que** eu acesse o destino em menos de 10ms enquanto a métrica do meu acesso é processada de forma assíncrona.

## 2. Fronteiras e Invariantes da Story (Boundaries)
- **Referências Arquiteturais:** `specs/architecture-spine.md#AD-01`, `specs/architecture-spine.md#AD-04`
- **Contrato Vinculado:** `specs/contracts/openapi.yaml` (`operationId: resolveShortUrl`)
- **Always (Obrigatório):**
  - Consultar cache Redis antes do PostgreSQL (padrão Cache-Aside).
  - Publicar evento assíncrono de clique no broker sem bloquear a resposta HTTP.
  - Retornar HTTP 302 com cabeçalho `Location` apontando para a URL original.
  - Tratar códigos inexistentes com HTTP 404 e links expirados ou inativos com HTTP 410.
- **Never (Proibido):**
  - Executar INSERT síncrono de métrica de clique durante a requisição de redirecionamento.

## 3. Critérios de Aceite em BDD (Gherkin)

### Cenário 1: Redirecionamento com sucesso
- **Given** que existe um link ativo com `shortCode` "a7B9x12" apontando para "https://exemplo.com"
- **When** o usuário faz GET em "/a7B9x12"
- **Then** a resposta deve ser HTTP 302 Found
- **And** o cabeçalho "Location" deve ser "https://exemplo.com"
- **And** um evento de métrica deve ser despachado.

### Cenário 2: ShortCode inexistente
- **Given** que o código "naoExiste" não está cadastrado
- **When** o usuário faz GET em "/naoExiste"
- **Then** a resposta deve ser HTTP 404 Not Found com ProblemDetails.

### Cenário 3: Link desativado ou expirado
- **Given** que o link com `shortCode` "expirado1" tem `isActive = false` ou `expiresAt` anterior à data atual
- **When** o usuário faz GET em "/expirado1"
- **Then** a resposta deve ser HTTP 410 Gone com ProblemDetails.

## 4. Matriz I/O & Edge Cases
| Cenário | Input / Estado | Output Esperado | Comportamento Frontend / Browser |
| :--- | :--- | :--- | :--- |
| `HAPPY_PATH` | GET `/{shortCode}` ativo | `HTTP 302` + Location header | Browser segue o redirecionamento para a URL original |
| `NOT_FOUND` | GET `/{shortCode}` inexistente | `HTTP 404` + ProblemDetails | Exibe tela amigável de link não encontrado |
| `EXPIRED_OR_INACTIVE` | GET `/{shortCode}` inativo | `HTTP 410` + ProblemDetails | Exibe tela de link expirado/desativado |

</frozen-after-approval>

---

## 5. Mapa de Arquivos (Code Map)
- **Testes Backend:** `tests/Shortener.UnitTests/Application/ResolveShortUrlTests.cs`
- **Implementação Application / Infra:** `src/Shortener.Application/Urls/ResolveShortUrlQuery.cs`, `src/Shortener.Infrastructure/Caching/RedisCacheService.cs`, `src/Shortener.Infrastructure/Messaging/RabbitMqPublisher.cs`
- **Implementação API:** `src/Shortener.Api/Endpoints/RedirectEndpoints.cs`
