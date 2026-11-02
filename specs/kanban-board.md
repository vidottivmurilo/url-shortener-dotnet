# 📋 Quadro Kanban & Manifesto de Backlog: Encurtador de Links

> **Regra de Governança:** O estado oficial de cada User Story reside no frontmatter do seu respectivo arquivo em `specs/stories/US-XXX-plan.md`. Este painel reflete a visão consolidada da Sprint.
> **Regra de Transição:** Agentes Desenvolvedores só podem mover cards até **`built`**. Apenas o **Code Reviewer** move cards para **`done`**.

---

## 📊 Resumo de Capacidade (Velocity & Story Points)
- **Total de Épicos:** 1 (`EPIC-01: MVP Encurtador de Links de Alta Performance`)
- **Total de User Stories:** 4
- **Story Points Planejados (Fibonacci):** 11 pts (3 + 3 + 3 + 2)
- **Story Points Concluídos (`done`):** 6 pts

---

## 🗂️ Visão Kanban (Status Atual)

| 📝 Backlog (`ready-for-dev`) | 🚧 Em Execução (`in-progress`) | 🧪 Construído (`built`) | 🔍 Em Code Review (`in-review`) | ✅ Concluído (`done`) |
| :--- | :--- | :--- | :--- | :--- |
| `US-003` (3 pts)<br/>`US-004` (2 pts) | *(Nenhuma)* | *(Nenhuma)* | *(Nenhuma)* | `US-001` (3 pts)<br/>`US-002` (3 pts) |

---

## 📑 Índice Detalhado de Épicos e User Stories

| ID | Épico | Título da User Story | Prioridade | Story Points | Escopo (Back/Front) | Contrato (`operationId`) | Status | Loops de Review | Arquivo do Plano |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `US-001` | `EPIC-01` | Criação de Link Encurtado com Base62 | `P1 (MVP)` | `3` | `Full-Stack` | `createShortUrl` | `done` | `0` | [US-001-plan.md](./stories/US-001-plan.md) |
| `US-002` | `EPIC-01` | Redirecionamento HTTP 302 com Cache Redis e Mensageria | `P1 (MVP)` | `3` | `Full-Stack` | `resolveShortUrl` | `done` | `0` | [US-002-plan.md](./stories/US-002-plan.md) |
| `US-003` | `EPIC-01` | Métricas e Dashboard Analítico de Acessos | `P2` | `3` | `Full-Stack` | `getUrlStats` | `ready-for-dev` | `0` | [US-003-plan.md](./stories/US-003-plan.md) |
| `US-004` | `EPIC-01` | Desativação Manual de Link Encurtado | `P2` | `2` | `Full-Stack` | `deactivateShortUrl` | `ready-for-dev` | `0` | [US-004-plan.md](./stories/US-004-plan.md) |
