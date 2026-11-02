# 📋 Regras de Domínio e Análise de Backend

## 1. Glossário Ubíquo Canônico (DDD)

Para assegurar consistência em todas as camadas e evitar ambiguidades conceituais, os seguintes termos são oficiais e imutáveis:

| Termo Canônico | Tipo / Formato | Descrição | Sinônimos Proibidos |
| :--- | :--- | :--- | :--- |
| `shortCode` | `string` (alfanumérico Base62, 7 chars) | Identificador curto e único gerado para representar a URL longa. | `slug`, `hash`, `alias`, `code` |
| `originalUrl` | `string` (URI válida, max 2048 chars) | URL de destino fornecida pelo usuário que receberá o redirecionamento. | `longUrl`, `targetUrl`, `destination` |
| `shortenedUrl` | Entidade de Domínio | Registro mestre contendo `id`, `shortCode`, `originalUrl`, `createdAt`, `expiresAt`, `isActive`. | `link`, `urlMapping` |
| `clickMetric` | Entidade / Evento de Domínio | Registro de acesso a um link contendo `shortCode`, `clickedAt`, `ipAddress`, `userAgent`. | `hit`, `accessLog`, `analytics` |
| `expiresAt` | `DateTime?` (UTC, ISO 8601) | Data/hora limite de validade do link encurtado. Se nulo, link é permanente. | `expirationDate`, `validUntil` |
| `isActive` | `bool` (default: true) | Flag para soft-delete e desativação manual do link pelo proprietário. | `enabled`, `deleted` |

---

## 2. Requisitos Funcionais (FR)

### `FR-001`: Criação de Link Encurtado
- **Entrada:** `originalUrl` (obrigatória, formato HTTP/HTTPS válido), `expiresAt` (opcional, deve ser futura caso informada).
- **Processamento:**
  1. Valida conformidade e tamanho da `originalUrl` ($10 \le \text{length} \le 2048$).
  2. Gera ID numérico sequencial/distribuído único.
  3. Codifica o ID em `shortCode` utilizando `Base62Encoder` (7 caracteres).
  4. Persiste no PostgreSQL com `isActive = true`.
  5. Popula o cache Redis com a chave `short:{shortCode}` e TTL condizente com a expiração.
- **Saída:** Objeto com `shortCode`, `originalUrl`, `shortUrl` completa, `createdAt`, `expiresAt`.

### `FR-002`: Resolução de Link Encurtado & Redirecionamento
- **Entrada:** `shortCode` via rota GET `/{shortCode}`.
- **Processamento:**
  1. Consulta cache Redis (`short:{shortCode}`).
  2. Se cache hit: verifica `isActive` e `expiresAt`. Se válido, prepara redirecionamento.
  3. Se cache miss: busca no banco relacional PostgreSQL. Se encontrado, ativo e não expirado, popula o Redis e avança. Se não encontrado/inativo/expirado, retorna HTTP 404/410.
  4. Publica mensagem assíncrona `LinkClickedEvent` no RabbitMQ (`shortCode`, `clickedAt`, `ipAddress`, `userAgent`).
  5. Retorna `HTTP 302 Found` com header `Location: {originalUrl}`.

### `FR-003`: Consulta de Métricas e Estatísticas
- **Entrada:** `shortCode` via rota GET `/api/v1/urls/{shortCode}/stats`.
- **Processamento:**
  1. Verifica se o `shortCode` existe.
  2. Retorna contagem total de cliques, data do último acesso e distribuição recente por navegador/SO extraídos dos eventos persistidos.
- **Saída:** HTTP 200 com resumo consolidado de acessos.

### `FR-004`: Desativação de Link
- **Entrada:** `shortCode` via DELETE `/api/v1/urls/{shortCode}`.
- **Processamento:**
  1. Localiza a entidade correspondente.
  2. Altera `isActive = false`.
  3. Invalida imediatamente a chave `short:{shortCode}` no Redis.
- **Saída:** HTTP 204 No Content.

---

## 3. Matriz I/O de Erros e Casos de Borda

| Cenário de Negócio | HTTP Status | RFC 7807 Title | Detail / Causa Raiz |
| :--- | :--- | :--- | :--- |
| URL original vazia ou com formato inválido | `400 Bad Request` | `Validation Error` | A URL informada não inicia com `http://` ou `https://` ou excede 2048 chars. |
| Data de expiração informada já expirada no passado | `400 Bad Request` | `Invalid Expiration Date` | `expiresAt` deve ser estritamente posterior a `DateTime.UtcNow`. |
| `shortCode` inexistente | `404 Not Found` | `Link Not Found` | Nenhum link registrado corresponde ao código requisitado. |
| Link existente porém desativado (`isActive = false`) | `410 Gone` | `Link Deactivated` | O link foi desativado manualmente pelo criador. |
| Link existente porém expirado (`expiresAt < UtcNow`) | `410 Gone` | `Link Expired` | O link atingiu o tempo limite de expiração estipulado. |
| Falha transitória de infraestrutura (Postgres/Redis) | `503 Service Unavailable` | `Service Degraded` | Erro de conexão gerenciado com política de retry e circuit breaker. |
