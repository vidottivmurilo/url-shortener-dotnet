# 🖥️ Especificação de Frontend & Experiência do Usuário

## 1. Visão Geral da Interface

A interface do encurtador é uma SPA ágil, limpa e responsiva (HTML5 semântico, Tailwind CSS e Vanilla/TypeScript client tipado), servida nativamente pela Web API (`src/Shortener.Api/wwwroot`) ou desacoplada, garantindo latência zero e experiência fluida.

---

## 2. Rotas e Hierarquia de Componentes

```text
/ (Página Principal)
└── ShortenerApp
    ├── Navbar (Logo, Status da Conexão da API)
    ├── UrlShortenerForm (Input de URL original, data de expiração opcional, botão Encurtar)
    ├── ShortenedLinkCard (Resultado com URL encurtada, botão de cópia com feedback, QR Code)
    ├── RecentLinksTable (Listagem local de links gerados com botões de Ver Estatísticas e Desativar)
    └── UrlStatsModal (Modal com contagem de cliques, último clique e detalhes analíticos)
```

---

## 3. Especificação dos 4 Estados Visuais Obrigatórios por Operação

### `createShortUrl` (Formulário de Encurtamento)
* **⏳ Loading:** O botão de submit exibe spinner animado, texto "Gerando link curto..." e fica desabilitado (`disabled`) para prevenir submissões duplicadas.
* **📭 Empty:** O formulário inicia limpo com placeholder explicativo e atalho para colar da área de transferência.
* **❌ Error:** Banner vermelho de erro ou texto abaixo do input exibindo a mensagem exata do `detail` retornado no payload RFC 7807 (`ProblemDetails`).
* **✅ Success:** O formulário destaca o `ShortenedLinkCard` verde com a URL curta gerada, botão "Copiar" que muda temporariamente para "Copiado! ✓", e adiciona o link à tabela de links recentes.

### `getUrlStats` (Modal de Métricas)
* **⏳ Loading:** O modal abre exibindo skeleton loaders no lugar dos contadores numéricos de cliques.
* **📭 Empty:** Caso o link ainda não possua cliques, exibe: "Nenhum clique registrado até o momento. Compartilhe seu link para começar a coletar dados!" com ícone amigável.
* **❌ Error:** Exibe alerta vermelho: "Não foi possível carregar as métricas deste link. Verifique se o link ainda está ativo." com botão de retry.
* **✅ Success:** Exibe contador numérico de destaque com o total de cliques, badge de status (Ativo/Inativo) e timestamp formatado do último clique.

### `deactivateShortUrl` (Desativação de Link)
* **⏳ Loading:** Botão de desativação exibe ícone de carregamento e desabilita ações concorrentes.
* **❌ Error:** Exibe toast de erro: "Falha ao desativar link. Tente novamente."
* **✅ Success:** Atualiza imediatamente o badge do link na tabela para "Desativado" e desabilita novas tentativas de acesso.
