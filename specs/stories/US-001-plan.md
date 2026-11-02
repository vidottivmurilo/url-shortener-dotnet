---
id: "US-001"
epic: "EPIC-01"
title: "Criação de Link Encurtado com Base62"
priority: "P1"
story_points: 3
scope: "full-stack"
covers_requirements: ["FR-001"]
contract_operations: ["createShortUrl"]
status: "done"
baseline_revision: "949c865a8b82da904510e67c33df415728f9af13"
review_loop_iteration: 0
---

<frozen-after-approval reason="human-and-analyst-owned intent — Dev and QA agents MUST NOT modify this block">

## 1. User Story & Valor de Negócio
- **Como** um usuário final da aplicação,
- **Quero** informar uma URL longa válida e receber um link curto único gerado em Base62,
- **Para que** eu possa compartilhar links concisos e amigáveis de forma rápida e segura.

## 2. Fronteiras e Invariantes da Story (Boundaries)
- **Referências Arquiteturais:** `specs/architecture-spine.md#AD-01`, `specs/architecture-spine.md#AD-02`, `specs/architecture-spine.md#AD-03`
- **Contrato Vinculado:** `specs/contracts/openapi.yaml` (`operationId: createShortUrl`)
- **Always (Obrigatório):**
  - Utilizar a classe `Base62Encoder` já testada no Domínio para codificar o identificador do link.
  - Validar se a URL original é válida (HTTP ou HTTPS, tamanho entre 10 e 2048 caracteres).
  - Retornar payload padronizado em JSON com HTTP 201 Created e cabeçalho ProblemDetails para erros 400.
  - Implementar na interface o formulário de encurtamento com os estados visuais `Loading`, `Error` e `Success`.
- **Never (Proibido):**
  - Não expor detalhes de banco relacional diretamente no Controller/Endpoint HTTP.
  - Não permitir criação de link com URL inválida ou data de expiração no passado.

## 3. Critérios de Aceite em BDD (Gherkin)

### Cenário 1: Criação de link com URL válida com sucesso (Happy Path)
- **Given** que o usuário informa uma URL válida "https://google.com" e nenhuma data de expiração
- **When** envia a requisição POST para "/api/v1/urls"
- **Then** a resposta deve ter código HTTP 201 Created
- **And** deve conter `shortCode` em Base62, `originalUrl`, `shortUrl` e `createdAt`
- **And** no Frontend a tela deve transicionar do estado `Loading` para `Success`, exibindo o cartão com o link encurtado e botão Copiar.

### Cenário 2: URL original inválida ou vazia
- **Given** que o usuário informa uma URL inválida "not-a-valid-url" ou em branco
- **When** envia a requisição POST para "/api/v1/urls"
- **Then** a resposta deve ter código HTTP 400 Bad Request
- **And** deve conter objeto RFC 7807 ProblemDetails com detalhamento do erro
- **And** no Frontend o campo de URL deve destacar o erro com a mensagem retornada.

### Cenário 3: Data de expiração no passado
- **Given** que o usuário informa uma data de expiração anterior ao momento atual
- **When** envia a requisição POST para "/api/v1/urls"
- **Then** a resposta deve retornar HTTP 400 Bad Request com ProblemDetails
- **And** o Frontend exibe alerta de data de expiração inválida.

## 4. Matriz I/O & Edge Cases
| Cenário | Input / Estado | Output Esperado (Backend) | Comportamento Esperado (Frontend) |
| :--- | :--- | :--- | :--- |
| `HAPPY_PATH` | `{ "originalUrl": "https://google.com" }` | `HTTP 201` + ShortenedUrlResponse | Exibe card de sucesso com link curto e botão Copiar |
| `INVALID_URL` | `{ "originalUrl": "invalido" }` | `HTTP 400` + ProblemDetails | Exibe feedback de erro no campo de input |
| `PAST_EXPIRY` | `{ "originalUrl": "https://google.com", "expiresAt": "2020-01-01T00:00:00Z" }` | `HTTP 400` + ProblemDetails | Exibe mensagem de erro de expiração |

</frozen-after-approval>

---

## 5. Mapa de Arquivos (Code Map)
- **Testes Backend (TDD Red):** `tests/Shortener.UnitTests/Application/CreateShortUrlTests.cs`
- **Testes Frontend / Integração:** `tests/Shortener.UnitTests/Api/CreateShortUrlEndpointTests.cs`
- **Implementação Domínio / Application:** `src/Shortener.Domain/Entities/ShortenedUrl.cs`, `src/Shortener.Application/Urls/CreateShortUrlCommand.cs`
- **Implementação Infraestrutura / API:** `src/Shortener.Infrastructure/Persistence/ShortenerDbContext.cs`, `src/Shortener.Api/Endpoints/UrlEndpoints.cs`
- **Implementação Frontend SPA:** `src/Shortener.Api/wwwroot/index.html`, `src/Shortener.Api/wwwroot/app.js`

---

## 6. Checklist Passo a Passo de Execução
- [x] **Passo 1 (QA - TDD RED):** Criar teste de unidade/integração para `CreateShortUrl` garantindo falha por asserção (*Clean Red* comprovado com 10 falhas esperadas).
- [x] **Passo 2 (Dev - TDD GREEN):** Implementar comando, validação, repositório e endpoint até os testes passarem (35 testes aprovados).
- [x] **Passo 3 (Dev - Frontend):** Implementar interface SPA com formulário, estados de UI (`Loading`, `Error`, `Success`) consumindo o endpoint.
- [x] **Passo 4 (Refactor & Review):** Executar suite completa de testes e atualizar status para `built`.

---

## 7. Evidências de Execução (TDD Logs)
- **Fase RED:** `dotnet test` -> `Com falha! – Com falha: 10, Aprovado: 25, Total: 35` (Clean Red verificado).
- **Fase GREEN:** `dotnet test` -> `Aprovado! – Com falha: 0, Aprovado: 35, Total: 35` (100% GREEN verificado).
- **Mutation Check:** Inversão na validação de URI ou data no domínio faz os testes falharem imediatamente. Anti-fragilidade comprovada.

---

## 8. Histórico de Code Review (Gate de Qualidade)
- **Data da Auditoria:** 2026-11-02
- **Auditor:** Staff Engineer / Tech Code Reviewer
- **Diff Inspecionado:** `baseline: 949c865a8b82da904510e67c33df415728f9af13` até `HEAD`
- **Dimensão 1 (Spec Compliance & Paridade):** `✅ APROVADO` (100% dos cenários Gherkin e estados de UI Loading/Empty/Error/Success implementados sem mocks residuais).
- **Dimensão 2 (Code Quality & Arquitetura):** `✅ APROVADO` (Clean Architecture e invariantes AD-01 a AD-03 respeitados; 0 warnings, 35 testes passando).
- **Severidades:** 0 Critical, 0 Important.
- **Decisão:** Aprovado para promoção a `done`.


