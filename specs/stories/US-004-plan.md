---
id: "US-004"
epic: "EPIC-01"
title: "Desativação Manual de Link Encurtado"
priority: "P2"
story_points: 2
scope: "full-stack"
covers_requirements: ["FR-004"]
contract_operations: ["deactivateShortUrl"]
status: "done"
baseline_revision: "e06d73f00bc929ce87030c2675a51587ce5a969b"
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
- **Implementação Application:** `src/Shortener.Application/Urls/DeactivateShortUrlHandler.cs`
- **Implementação API & UI:** `src/Shortener.Api/Program.cs`, `src/Shortener.Api/wwwroot/app.js`

---

## 6. Checklist Passo a Passo de Execução
- [x] **Passo 1 (QA - TDD RED):** Criar testes de desativação (soft-delete, purga de cache e erro 404) garantindo Clean Red (2 falhas).
- [x] **Passo 2 (Dev - TDD GREEN):** Implementar `DeactivateShortUrlHandler`, remoção de chave Redis e endpoint DELETE `/api/v1/urls/{shortCode}` retornando HTTP 204 (44 testes aprovados).
- [x] **Passo 3 (Dev - Frontend UI):** Implementar ação de desativação com confirmação e atualização reativa do badge na tabela.
- [x] **Passo 4 (Auto-Verificação & Mutation Check):** Mover status para `built`.

---

## 7. Evidências de Execução (TDD Logs)
- **Fase RED:** `dotnet test` -> `Com falha! – Com falha: 2, Aprovado: 42, Total: 44` (Clean Red comprovado).
- **Fase GREEN:** `dotnet test` -> `Aprovado! – Com falha: 0, Aprovado: 44, Total: 44` (100% GREEN verificado).

---

## 8. Histórico de Code Review (Gate de Qualidade)
- **Data da Auditoria:** 2026-11-02
- **Auditor:** Staff Engineer / Tech Code Reviewer
- **Diff Inspecionado:** `baseline: e06d73f00bc929ce87030c2675a51587ce5a969b` até `HEAD`
- **Dimensão 1 (Spec Compliance & Paridade):** `✅ APROVADO` (Endpoint DELETE /api/v1/urls/{shortCode}, purga de cache, soft-delete e ação na interface com confirmação).
- **Dimensão 2 (Code Quality & Arquitetura):** `✅ APROVADO` (Isolamento de camadas e consistência transacional em memória; 44 testes passando).
- **Severidades:** 0 Critical, 0 Important.
- **Decisão:** Aprovado para promoção a `done`.


