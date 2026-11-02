// Cliente Tipado e Gerenciador de Estados da SPA
const state = {
  recentLinks: []
};

const dom = {
  form: document.getElementById('shortenerForm'),
  originalUrlInput: document.getElementById('originalUrlInput'),
  expiresAtInput: document.getElementById('expiresAtInput'),
  submitButton: document.getElementById('submitButton'),
  submitButtonText: document.getElementById('submitButtonText'),
  submitSpinner: document.getElementById('submitSpinner'),
  errorBanner: document.getElementById('errorBanner'),
  errorTitle: document.getElementById('errorTitle'),
  errorDetail: document.getElementById('errorDetail'),
  successCard: document.getElementById('successCard'),
  shortUrlLink: document.getElementById('shortUrlLink'),
  originalUrlPreview: document.getElementById('originalUrlPreview'),
  copyButton: document.getElementById('copyButton'),
  copyIcon: document.getElementById('copyIcon'),
  copyText: document.getElementById('copyText'),
  openButton: document.getElementById('openButton'),
  emptyLinksState: document.getElementById('emptyLinksState'),
  linksTableContainer: document.getElementById('linksTableContainer'),
  linksTableBody: document.getElementById('linksTableBody'),
  linksCountBadge: document.getElementById('linksCountBadge')
};

// Funções de Gestão de Estado de UI (Loading, Empty, Error, Success)
function setLoading(isLoading) {
  if (isLoading) {
    dom.submitButton.disabled = true;
    dom.submitButtonText.textContent = 'Gerando link...';
    dom.submitSpinner.classList.remove('hidden');
    dom.errorBanner.classList.add('hidden');
  } else {
    dom.submitButton.disabled = false;
    dom.submitButtonText.textContent = 'Encurtar URL';
    dom.submitSpinner.classList.add('hidden');
  }
}

function showError(title, detail) {
  dom.errorTitle.textContent = title || 'Erro ao Encurtar';
  dom.errorDetail.textContent = detail || 'Verifique se a URL informada é válida.';
  dom.errorBanner.classList.remove('hidden');
  dom.successCard.classList.add('hidden');
}

function showSuccess(linkData) {
  dom.errorBanner.classList.add('hidden');
  dom.shortUrlLink.textContent = linkData.shortUrl;
  dom.shortUrlLink.href = linkData.shortUrl;
  dom.originalUrlPreview.textContent = `Destino: ${linkData.originalUrl}`;
  dom.openButton.href = linkData.shortUrl;
  dom.successCard.classList.remove('hidden');

  addRecentLink(linkData);
}

function addRecentLink(link) {
  state.recentLinks.unshift(link);
  renderRecentLinks();
}

function renderRecentLinks() {
  if (state.recentLinks.length === 0) {
    dom.emptyLinksState.classList.remove('hidden');
    dom.linksTableContainer.classList.add('hidden');
    dom.linksCountBadge.textContent = '0 links';
    return;
  }

  dom.emptyLinksState.classList.add('hidden');
  dom.linksTableContainer.classList.remove('hidden');
  dom.linksCountBadge.textContent = `${state.recentLinks.length} ${state.recentLinks.length === 1 ? 'link' : 'links'}`;

  dom.linksTableBody.innerHTML = '';
  state.recentLinks.forEach(link => {
    const tr = document.createElement('tr');
    tr.className = 'hover:bg-slate-800/40 transition';

    const formattedDate = new Date(link.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

    tr.innerHTML = `
      <td class="px-4 py-3 font-mono font-medium text-indigo-400">
        <a href="${link.shortUrl}" target="_blank" class="hover:underline">${link.shortCode}</a>
      </td>
      <td class="px-4 py-3 max-w-xs truncate text-slate-300" title="${link.originalUrl}">
        ${link.originalUrl}
      </td>
      <td class="px-4 py-3 text-xs text-slate-500 whitespace-nowrap">
        ${formattedDate}
      </td>
      <td class="px-4 py-3 text-right">
        <button onclick="navigator.clipboard.writeText('${link.shortUrl}')" class="text-xs bg-slate-800 hover:bg-slate-700 text-slate-300 px-2.5 py-1 rounded transition">
          Copiar
        </button>
      </td>
    `;
    dom.linksTableBody.appendChild(tr);
  });
}

// Cliente da API (Consumo do endpoint POST /api/v1/urls)
async function createShortUrlApi(originalUrl, expiresAt) {
  const payload = { originalUrl };
  if (expiresAt) {
    payload.expiresAt = new Date(expiresAt).toISOString();
  }

  const response = await fetch('/api/v1/urls', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    const error = new Error(errorData.detail || 'Falha ao processar solicitação');
    error.title = errorData.title;
    error.status = response.status;
    throw error;
  }

  return await response.json();
}

// Event Listeners
dom.form.addEventListener('submit', async (e) => {
  e.preventDefault();

  const originalUrl = dom.originalUrlInput.value.trim();
  const expiresAt = dom.expiresAtInput.value;

  if (!originalUrl) {
    showError('Campo Obrigatório', 'Por favor, informe uma URL original.');
    return;
  }

  setLoading(true);
  try {
    const data = await createShortUrlApi(originalUrl, expiresAt);
    showSuccess(data);
    dom.originalUrlInput.value = '';
    dom.expiresAtInput.value = '';
  } catch (err) {
    showError(err.title || 'Falha na Validação', err.message);
  } finally {
    setLoading(false);
  }
});

dom.copyButton.addEventListener('click', async () => {
  const shortUrl = dom.shortUrlLink.textContent;
  if (!shortUrl) return;

  try {
    await navigator.clipboard.writeText(shortUrl);
    dom.copyText.textContent = 'Copiado! ✓';
    dom.copyIcon.textContent = '🎉';
    setTimeout(() => {
      dom.copyText.textContent = 'Copiar';
      dom.copyIcon.textContent = '📋';
    }, 2000);
  } catch (e) {
    console.error('Falha ao copiar:', e);
  }
});

// Inicialização
renderRecentLinks();
