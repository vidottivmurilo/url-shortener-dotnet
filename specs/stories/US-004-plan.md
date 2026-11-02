---
id: "US-004"
epic: "EPIC-01"
title: "Desativação Manual de Link Encurtado"
priority: "P2"
story_points: 2
scope: "full-stack"
covers_requirements: ["FR-004"]
contract_operations: ["deactivateShortUrl"]
status: "ready-for-dev"
baseline_revision: ""
review_loop_iteration: 0
---

<frozen-after-approval reason="human-and-analyst-owned intent — Dev and QA agents MUST NOT modify this block">

## 1. User Story & Valor de Negócio
- **Como** proprietário de um link encurtado,
- **Quero** desativar um link a qualquer momento,
- **Para que** ele pare de redirecionar visitantes imediatamente e saia do cache de acesso.

## 2. Fronteiras e Invariantes da Story (Boundaries)
- **Contrato Vinculado:** `specs/contracts/openapi.yaml` (`operationId: deactivateShortUrl`)
- **Always (Obrigatório):**
  - Executar soft-delete (`isActive = false`) no banco relacional.
  - Purgar a chave `short:{shortCode}` do cache Redis imediatamente.
  - Retornar HTTP 204 No Content para desativação com sucesso.
  - Atualizar o status visual na tabela de links recentes no Frontend.

## 3. Critérios de Aceite em BDD (Gherkin)

### Cenário 1: Desativação com sucesso
- **Given** que o link "a7B9x12" está ativo
- **When** o usuário envia DELETE "/api/v1/urls/a7B9x12"
- **Then** a resposta deve retornar HTTP 204 No Content
- **And** consultas subsequentes de redirecionamento devem retornar HTTP 410 Gone.

</frozen-after-approval>

---

## 5. Mapa de Arquivos (Code Map)
- **Testes Backend:** `tests/Shortener.UnitTests/Application/DeactivateShortUrlTests.cs`
- **Implementação Application:** `src/Shortener.Application/Urls/DeactivateShortUrlCommand.cs`
- **Implementação API & UI:** `src/Shortener.Api/Endpoints/UrlEndpoints.cs`, `src/Shortener.Api/wwwroot/app.js`
