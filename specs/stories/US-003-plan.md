---
id: "US-003"
epic: "EPIC-01"
title: "Métricas e Dashboard Analítico de Acessos"
priority: "P2"
story_points: 3
scope: "full-stack"
covers_requirements: ["FR-003"]
contract_operations: ["getUrlStats"]
status: "ready-for-dev"
baseline_revision: ""
review_loop_iteration: 0
---

<frozen-after-approval reason="human-and-analyst-owned intent — Dev and QA agents MUST NOT modify this block">

## 1. User Story & Valor de Negócio
- **Como** criador de um link encurtado,
- **Quero** visualizar estatísticas em tempo real sobre o total de cliques e data do último acesso,
- **Para que** eu acompanhe o engajamento e o alcance da minha campanha.

## 2. Fronteiras e Invariantes da Story (Boundaries)
- **Referências Arquiteturais:** `specs/architecture-spine.md#AD-02`, `specs/architecture-spine.md#AD-03`
- **Contrato Vinculado:** `specs/contracts/openapi.yaml` (`operationId: getUrlStats`)
- **Always (Obrigatório):**
  - Retornar objeto estruturado com `shortCode`, `originalUrl`, `totalClicks`, `lastClickedAt`, `createdAt`.
  - Exibir modal na interface com skeleton loader (Loading), zero cliques (Empty), dados consolidados (Success) e erro de carga (Error).

## 3. Critérios de Aceite em BDD (Gherkin)

### Cenário 1: Consulta de métricas de link com cliques registrados
- **Given** que o link "a7B9x12" possui 42 cliques registrados
- **When** o usuário consulta GET "/api/v1/urls/a7B9x12/stats"
- **Then** a resposta deve retornar HTTP 200 OK com `totalClicks = 42` e `lastClickedAt` preenchido
- **And** o modal no Frontend exibe o número "42" com destaque visual.

### Cenário 2: Consulta de métricas de link sem cliques (Empty State)
- **Given** que o link "novoLink1" foi recém-criado e possui 0 cliques
- **When** o usuário consulta GET "/api/v1/urls/novoLink1/stats"
- **Then** a resposta deve retornar HTTP 200 OK com `totalClicks = 0` e `lastClickedAt = null`
- **And** o Frontend exibe mensagem "Nenhum clique registrado até o momento".

</frozen-after-approval>

---

## 5. Mapa de Arquivos (Code Map)
- **Testes Backend:** `tests/Shortener.UnitTests/Application/GetUrlStatsTests.cs`
- **Implementação Application:** `src/Shortener.Application/Urls/GetUrlStatsQuery.cs`
- **Implementação API & UI:** `src/Shortener.Api/Endpoints/StatsEndpoints.cs`, `src/Shortener.Api/wwwroot/app.js`
