const state = { structure: 'avl' };

const el = {
    segButtons: document.querySelectorAll('.segmented__btn'),
    activeLabel: document.getElementById('active-structure-label'),
    createForm: document.getElementById('create-form'),
    originalUrl: document.getElementById('original-url'),
    createBtn: document.getElementById('create-btn'),
    createResult: document.getElementById('create-result'),
    refreshBtn: document.getElementById('refresh-btn'),
    urlsBody: document.getElementById('urls-body'),
    urlsEmpty: document.getElementById('urls-empty'),
    benchmarkForm: document.getElementById('benchmark-form'),
    benchmarkOps: document.getElementById('benchmark-ops'),
    benchmarkBtn: document.getElementById('benchmark-btn'),
    benchmarkResult: document.getElementById('benchmark-result'),
    avlTotal: document.getElementById('avl-total'),
    hashTotal: document.getElementById('hash-total'),
    chart: document.getElementById('benchmark-chart'),
    toast: document.getElementById('toast')
};

let toastTimer = null;

function showToast(message, type) {
    el.toast.textContent = message;
    el.toast.className = 'toast is-visible' + (type ? ' toast--' + type : '');
    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(() => { el.toast.className = 'toast'; }, 3200);
}

function formatDate(value) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    return date.toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

function formatMs(value) {
    return Number(value).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[ch]));
}

async function apiFetch(path, options) {
    const response = await fetch(path, options);
    let payload = null;
    const text = await response.text();
    if (text) {
        try { payload = JSON.parse(text); } catch { payload = text; }
    }
    if (!response.ok) {
        const message = typeof payload === 'string' && payload ? payload : 'Erro ' + response.status;
        throw new Error(message);
    }
    return payload;
}

function setStructure(structure) {
    state.structure = structure;
    el.activeLabel.textContent = structure.toUpperCase();
    el.segButtons.forEach(btn => {
        const active = btn.dataset.structure === structure;
        btn.classList.toggle('is-active', active);
        btn.setAttribute('aria-selected', active ? 'true' : 'false');
    });
    loadUrls();
}

async function loadUrls() {
    try {
        const urls = await apiFetch('/' + state.structure + '/urls');
        renderUrls(Array.isArray(urls) ? urls : []);
    } catch (error) {
        showToast(error.message, 'error');
        renderUrls([]);
    }
}

function renderUrls(urls) {
    el.urlsBody.innerHTML = '';
    if (!urls.length) {
        el.urlsEmpty.classList.remove('hidden');
        return;
    }
    el.urlsEmpty.classList.add('hidden');
    const sorted = urls.slice().sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    for (const item of sorted) {
        const shortPath = '/' + state.structure + '/urls/' + item.code;
        const row = document.createElement('tr');
        row.innerHTML =
            '<td class="code-cell">' + escapeHtml(item.code) + '</td>' +
            '<td class="url-cell"><a href="' + escapeHtml(item.originalUrl) + '" target="_blank" rel="noopener" title="' + escapeHtml(item.originalUrl) + '">' + escapeHtml(item.originalUrl) + '</a></td>' +
            '<td>' + escapeHtml(formatDate(item.createdAt)) + '</td>' +
            '<td class="num">' + Number(item.accessCount).toLocaleString('pt-BR') + '</td>' +
            '<td class="actions-col"><div class="row-actions">' +
            '<button class="icon-btn" data-action="open" data-path="' + shortPath + '">Abrir</button>' +
            '<button class="icon-btn icon-btn--danger" data-action="delete" data-code="' + escapeHtml(item.code) + '">Excluir</button>' +
            '</div></td>';
        el.urlsBody.appendChild(row);
    }
}

async function createUrl(event) {
    event.preventDefault();
    const originalUrl = el.originalUrl.value.trim();
    if (!originalUrl) return;
    el.createBtn.disabled = true;
    try {
        const result = await apiFetch('/' + state.structure + '/urls', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ originalUrl }) });
        el.createResult.classList.remove('hidden');
        el.createResult.innerHTML =
            '<span class="chip">' + escapeHtml(result.code) + '</span>' +
            '<a class="result__link" href="' + escapeHtml(result.shortUrl) + '" target="_blank" rel="noopener">' + escapeHtml(result.shortUrl) + '</a>' +
            '<span class="result__meta">&rarr; ' + escapeHtml(result.originalUrl) + '</span>';
        el.originalUrl.value = '';
        showToast('URL encurtada com sucesso.', 'success');
        loadUrls();
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        el.createBtn.disabled = false;
    }
}

async function openUrl(path) {
    try {
        const data = await apiFetch(path + '?raw=true');
        window.open(data.originalUrl, '_blank', 'noopener');
        loadUrls();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

async function deleteUrl(code) {
    try {
        await apiFetch('/' + state.structure + '/urls/' + code, { method: 'DELETE' });
        showToast('URL removida.', 'success');
        loadUrls();
    } catch (error) {
        showToast(error.message, 'error');
    }
}

function onTableClick(event) {
    const button = event.target.closest('button[data-action]');
    if (!button) return;
    if (button.dataset.action === 'open') openUrl(button.dataset.path);
    if (button.dataset.action === 'delete') deleteUrl(button.dataset.code);
}

async function runBenchmark(event) {
    event.preventDefault();
    const operations = parseInt(el.benchmarkOps.value, 10);
    if (!operations || operations <= 0) {
        showToast('Informe um número de operações válido.', 'error');
        return;
    }
    el.benchmarkBtn.disabled = true;
    el.benchmarkBtn.textContent = 'Executando...';
    try {
        const [avl, hash] = await Promise.all([
            apiFetch('/avl/benchmark?operations=' + operations),
            apiFetch('/hash/benchmark?operations=' + operations)
        ]);
        el.benchmarkResult.classList.remove('hidden');
        el.avlTotal.textContent = formatMs(avl.totalMs);
        el.hashTotal.textContent = formatMs(hash.totalMs);
        drawChart(avl, hash);
        showToast('Benchmark concluído para ' + operations.toLocaleString('pt-BR') + ' operações.', 'success');
    } catch (error) {
        showToast(error.message, 'error');
    } finally {
        el.benchmarkBtn.disabled = false;
        el.benchmarkBtn.textContent = 'Executar benchmark';
    }
}

function drawChart(avl, hash) {
    const canvas = el.chart;
    const ctx = canvas.getContext('2d');
    const width = canvas.width;
    const height = canvas.height;
    ctx.clearRect(0, 0, width, height);

    const groups = [
        { label: 'Put', avl: avl.putMs, hash: hash.putMs },
        { label: 'Get', avl: avl.getMs, hash: hash.getMs },
        { label: 'Delete', avl: avl.deleteMs, hash: hash.deleteMs }
    ];
    const colors = { avl: '#4f46e5', hash: '#06b6d4' };
    const padding = { top: 30, right: 24, bottom: 46, left: 58 };
    const plotWidth = width - padding.left - padding.right;
    const plotHeight = height - padding.top - padding.bottom;

    const maxValue = Math.max(...groups.flatMap(g => [g.avl, g.hash]), 0.0001);
    const niceMax = niceCeil(maxValue);

    ctx.strokeStyle = '#e2e6ef';
    ctx.fillStyle = '#6b7488';
    ctx.lineWidth = 1;
    ctx.font = '12px Inter, sans-serif';
    ctx.textAlign = 'right';
    ctx.textBaseline = 'middle';
    const ticks = 4;
    for (let i = 0; i <= ticks; i++) {
        const value = (niceMax / ticks) * i;
        const y = padding.top + plotHeight - (value / niceMax) * plotHeight;
        ctx.beginPath();
        ctx.moveTo(padding.left, y);
        ctx.lineTo(width - padding.right, y);
        ctx.stroke();
        ctx.fillText(value.toFixed(1), padding.left - 8, y);
    }

    const groupWidth = plotWidth / groups.length;
    const barWidth = Math.min(46, groupWidth / 3);
    const gap = 10;

    groups.forEach((group, index) => {
        const groupCenter = padding.left + groupWidth * index + groupWidth / 2;
        const avlX = groupCenter - barWidth - gap / 2;
        const hashX = groupCenter + gap / 2;
        drawBar(ctx, avlX, group.avl, niceMax, barWidth, padding, plotHeight, colors.avl);
        drawBar(ctx, hashX, group.hash, niceMax, barWidth, padding, plotHeight, colors.hash);

        ctx.fillStyle = '#1b2333';
        ctx.font = '600 13px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'top';
        ctx.fillText(group.label, groupCenter, padding.top + plotHeight + 10);
    });

    ctx.fillStyle = '#6b7488';
    ctx.font = '11px Inter, sans-serif';
    ctx.textAlign = 'left';
    ctx.textBaseline = 'top';
    ctx.fillText('Tempo (ms) — menor é melhor', padding.left, 8);
}

function drawBar(ctx, x, value, maxValue, barWidth, padding, plotHeight, color) {
    const barHeight = (value / maxValue) * plotHeight;
    const y = padding.top + plotHeight - barHeight;
    const radius = Math.min(6, barWidth / 2);
    ctx.fillStyle = color;
    roundedRectTop(ctx, x, y, barWidth, barHeight, radius);
    ctx.fill();

    ctx.fillStyle = color;
    ctx.font = '600 11px Inter, sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'bottom';
    ctx.fillText(value.toFixed(2), x + barWidth / 2, y - 4);
}

function roundedRectTop(ctx, x, y, w, h, r) {
    const radius = Math.min(r, h);
    ctx.beginPath();
    ctx.moveTo(x, y + h);
    ctx.lineTo(x, y + radius);
    ctx.quadraticCurveTo(x, y, x + radius, y);
    ctx.lineTo(x + w - radius, y);
    ctx.quadraticCurveTo(x + w, y, x + w, y + radius);
    ctx.lineTo(x + w, y + h);
    ctx.closePath();
}

function niceCeil(value) {
    const magnitude = Math.pow(10, Math.floor(Math.log10(value)));
    const normalized = value / magnitude;
    let nice;
    if (normalized <= 1) nice = 1;
    else if (normalized <= 2) nice = 2;
    else if (normalized <= 5) nice = 5;
    else nice = 10;
    return nice * magnitude;
}

el.segButtons.forEach(btn => btn.addEventListener('click', () => setStructure(btn.dataset.structure)));
el.createForm.addEventListener('submit', createUrl);
el.refreshBtn.addEventListener('click', loadUrls);
el.urlsBody.addEventListener('click', onTableClick);
el.benchmarkForm.addEventListener('submit', runBenchmark);

setStructure('avl');
