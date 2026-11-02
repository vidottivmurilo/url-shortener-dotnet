---
id: "US-003"
epic: "EPIC-01"
title: "Métricas e Dashboard Analítico de Acessos"
priority: "P2"
story_points: 3
scope: "full-stack"
covers_requirements: ["FR-003"]
contract_operations: ["getUrlStats"]
status: "done"
baseline_revision: "f83ccd0bfe5029e691afb9e2a956f05271089b11"
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
- **Implementação Application:** `src/Shortener.Application/Urls/GetUrlStatsHandler.cs`, `src/Shortener.Application/Common/Interfaces/IClickMetricRepository.cs`
- **Implementação Infra & API:** `src/Shortener.Infrastructure/Persistence/InMemoryClickMetricRepository.cs`, `src/Shortener.Api/Program.cs`
- **Implementação Frontend UI:** `src/Shortener.Api/wwwroot/index.html`, `src/Shortener.Api/wwwroot/app.js`

---

## 6. Checklist Passo a Passo de Execução
- [x] **Passo 1 (QA - TDD RED):** Criar testes de consulta de métricas com cliques, sem cliques e erro 404 (Clean Red comprovado com 3 falhas).
- [x] **Passo 2 (Dev - TDD GREEN):** Implementar `GetUrlStatsHandler`, repositório de métricas e endpoint GET `/api/v1/urls/{shortCode}/stats` (42 testes aprovados).
- [x] **Passo 3 (Dev - Frontend UI):** Implementar modal de métricas com estados Loading, Empty (0 cliques), Error e Success em `index.html` e `app.js`.
- [x] **Passo 4 (Auto-Verificação & Mutation Check):** Mover status para `built`.

---

## 7. Evidências de Execução (TDD Logs)
- **Fase RED:** `dotnet test` -> `Com falha! – Com falha: 3, Aprovado: 39, Total: 42` (Clean Red verificado).
- **Fase GREEN:** `dotnet test` -> `Aprovado! – Com falha: 0, Aprovado: 42, Total: 42` (100% GREEN verificado).

---

## 8. Histórico de Code Review (Gate de Qualidade)
- **Data da Auditoria:** 2026-11-02
- **Auditor:** Staff Engineer / Tech Code Reviewer
- **Diff Inspecionado:** `baseline: f83ccd0bfe5029e691afb9e2a956f05271089b11` até `HEAD`
- **Dimensão 1 (Spec Compliance & Paridade):** `✅ APROVADO` (Endpoint getUrlStats, contagem de cliques, modal responsivo com os 4 estados visuais cumpridos).
- **Dimensão 2 (Code Quality & Arquitetura):** `✅ APROVADO` (Separação estrita de interfaces na Application e persistência na Infrastructure; 42 testes passando).
- **Severidades:** 0 Critical, 0 Important.
- **Decisão:** Aprovado para promoção a `done`.


