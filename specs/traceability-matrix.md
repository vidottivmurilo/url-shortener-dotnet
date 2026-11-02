# 🔗 Matriz de Rastreabilidade e Paridade Full-Stack: Encurtador de Links

> **Invariante de Paridade:** Todo `operationId` do tipo `user-facing` definido em `specs/contracts/openapi.yaml` DEVE possuir mapeamento completo nesta tabela (Backend Handler + Componente Frontend + Estados de UI + Testes Automatizados).

---

## 1. Mapeamento Ponta-a-Ponta (Contrato ↔ Backend ↔ Frontend ↔ Testes)

| Requisito (`FR`) | Story ID | `operationId` (OpenAPI) | Método & Rota HTTP | Handler / Caso de Uso (Backend) | Rota & Componente (Frontend) | Estados de UI Exigidos | Teste Backend | Teste Frontend / UI | Status de Paridade |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `FR-001` | `US-001` | `createShortUrl` | `POST /api/v1/urls` | `CreateShortUrlHandler` | `/` (`UrlShortenerForm`, `ShortenedLinkCard`) | `Loading`, `Error 400`, `Success` | `CreateShortUrlTests` | `UrlShortenerFormTests` | `[X] Paridade OK (done)` |
| `FR-002` | `US-002` | `resolveShortUrl` | `GET /{shortCode}` | `ResolveShortUrlHandler` | `/` (`RedirectRouter`) | `Success 302`, `Error 404/410` | `ResolveShortUrlTests` | `RedirectTests` | `[X] Paridade OK (done)` |
| `FR-003` | `US-003` | `getUrlStats` | `GET /api/v1/urls/{shortCode}/stats` | `GetUrlStatsHandler` | `/` (`UrlStatsModal`) | `Loading`, `Empty`, `Error 404`, `Success` | `GetUrlStatsTests` | `UrlStatsModalTests` | `[X] Paridade OK (done)` |
| `FR-004` | `US-004` | `deactivateShortUrl` | `DELETE /api/v1/urls/{shortCode}` | `DeactivateShortUrlHandler` | `/` (`RecentLinksTable` action) | `Loading`, `Error 404`, `Success 204` | `DeactivateShortUrlTests` | `DeactivateActionTests` | `[X] Paridade OK (done)` |

---

## 2. Checklist de Verificação de Paridade (Executado no Gate de Code Review)
- [ ] Todos os `operationId`s de `specs/contracts/openapi.yaml` constam na tabela acima?
- [ ] Nenhum componente Frontend utiliza dados mockados/hardcoded no código de produção em vez de chamar o cliente da API?
- [ ] Todos os 4 estados visuais (`Loading`, `Empty`, `Error`, `Success`) possuem cobertura nos testes de componente/UI?
