// ============================================
// FINSIM-AI: Frontend Logic
// ============================================

// --- SignalR Connection ---
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Lightweight Markdown parser: **bold**, *italic*, `code`, ### headers, lists, ---
 */
function parseMarkdown(text) {
    if (!text) return '';
    let html = escapeHtml(text);

    // Headers (### → h4, ## → h3, # → h2)
    html = html.replace(/^### (.+)$/gm, '<h4 class="md-h4">$1</h4>');
    html = html.replace(/^## (.+)$/gm, '<h3 class="md-h3">$1</h3>');
    html = html.replace(/^# (.+)$/gm, '<h2 class="md-h2">$1</h2>');

    // Bold + Italic combined ***text***
    html = html.replace(/\*\*\*(.+?)\*\*\*/g, '<strong><em>$1</em></strong>');
    // Bold **text**
    html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
    // Italic *text*
    html = html.replace(/(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)/g, '<em>$1</em>');

    // Inline code `code`
    html = html.replace(/`([^`]+)`/g, '<code class="md-code">$1</code>');

    // Horizontal rule ---
    html = html.replace(/^---$/gm, '<hr class="md-hr">');

    // Unordered lists - item
    html = html.replace(/^- (.+)$/gm, '<li class="md-li">$1</li>');
    html = html.replace(/(<li[^>]*>.*<\/li>\n?)+/g, '<ul class="md-ul">$&</ul>');

    // Ordered lists 1. item
    html = html.replace(/^\d+\. (.+)$/gm, '<li class="md-li md-li--ordered">$1</li>');

    // Paragraphs (double newline)
    html = html.replace(/\n\n/g, '</p><p class="md-p">');
    html = '<p class="md-p">' + html + '</p>';

    // Clean up empty paragraphs
    html = html.replace(/<p class="md-p">\s*<\/p>/g, '');
    // Fix headers/lists wrapped in paragraphs
    html = html.replace(/<p class="md-p">(<h[234]|<ul|<hr|<li)/g, '$1');
    html = html.replace(/(<\/h[234]>|<\/ul>|<hr[^>]*>)<\/p>/g, '$1');

    return html;
}

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/simulationHub")
    .withAutomaticReconnect()
    .build();

// --- State ---
let isRunning = false;

// --- DOM Elements ---
const elements = {
    // Tabs
    tabs: {
        simulator: document.getElementById('tab-simulator'),
        methodology: document.getElementById('tab-methodology'),
        settings: document.getElementById('tab-settings'),
    },
    // Ticker
    tickerInflation: document.getElementById('tickerInflation'),
    tickerRates: document.getElementById('tickerRates'),
    tickerLiquidity: document.getElementById('tickerLiquidity'),
    tickerSentiment: document.getElementById('tickerSentiment'),
    // Sliders
    sliderInflation: document.getElementById('sliderInflation'),
    sliderRates: document.getElementById('sliderRates'),
    sliderLiquidity: document.getElementById('sliderLiquidity'),
    valInflation: document.getElementById('valInflation'),
    valRates: document.getElementById('valRates'),
    valLiquidity: document.getElementById('valLiquidity'),
    // Inputs
    eventInput: document.getElementById('eventInput'),
    btnExecute: document.getElementById('btnExecute'),
    // Feed
    agentFeed: document.getElementById('agentFeed'),
    // CIO
    cioPanel: document.getElementById('cioPanel'),
    cioProbs: document.getElementById('cioProbs'),
    cioJson: document.getElementById('cioJson'),
    probBull: document.getElementById('probBull'),
    probNeutral: document.getElementById('probNeutral'),
    probBear: document.getElementById('probBear'),
    convictionScore: document.getElementById('convictionScore'),
    jsonBlock: document.getElementById('jsonBlock'),
    // Risk gauge
    riskGaugeMarker: document.getElementById('riskGaugeMarker'),
    riskGaugeScore: document.getElementById('riskGaugeScore'),
};

// --- Agent Metadata ---
const agentMeta = {
    'Reddit-Retail': { icon: '📱', shortName: 'RT', label: 'Retail' },
    'Aura-Hedge': { icon: '🎯', shortName: 'HF', label: 'Hedge Fund' },
    'Vanguard-IG': { icon: '🏛', shortName: 'IG', label: 'Institucional' },
    'Citadel-MM': { icon: '⚙', shortName: 'MM', label: 'Market Maker' },
};

// --- Tab Switching ---
function switchTab(tabName) {
    // Hide all tabs
    Object.values(elements.tabs).forEach(tab => tab.classList.add('hidden'));
    // Show target
    const target = elements.tabs[tabName];
    if (target) target.classList.remove('hidden');
    // Update nav
    document.querySelectorAll('.nav__tab').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.tab === tabName);
    });
}

// --- Sliders ---
elements.sliderInflation.addEventListener('input', (e) => {
    elements.valInflation.textContent = `${e.target.value}%`;
});
elements.sliderRates.addEventListener('input', (e) => {
    elements.valRates.textContent = `${e.target.value}%`;
});
elements.sliderLiquidity.addEventListener('input', (e) => {
    elements.valLiquidity.textContent = e.target.value;
});

// --- World State Update ---
async function updateWorldState() {
    const inf = parseFloat(elements.sliderInflation.value);
    const rates = parseFloat(elements.sliderRates.value);
    const liq = parseFloat(elements.sliderLiquidity.value);
    await connection.invoke("UpdateWorldState", inf, rates, liq);
}

// --- Reset Simulation ---
async function resetSimulation() {
    if (isRunning) return;
    try {
        await connection.invoke("ResetSimulation");
    } catch (err) {
        console.error("Error en reset:", err);
        alert('❌ Error al resetear: ' + err.message);
    }
}

// --- Run Simulation ---
async function runSimulation() {
    const eventText = elements.eventInput.value.trim();
    if (!eventText || isRunning) return;

    isRunning = true;
    elements.btnExecute.disabled = true;
    elements.btnExecute.innerHTML = '<span class="btn__icon">⏳</span> Simulando...';

    // Clear previous feed
    elements.agentFeed.innerHTML = '';
    elements.cioPanel.innerHTML = '<div class="cio__empty"><p>Esperando respuestas de agentes...</p></div>';
    elements.cioProbs.classList.add('hidden');
    elements.cioJson.classList.add('hidden');

    try {
        await connection.invoke("RunSimulation", eventText);
    } catch (err) {
        console.error("Error en simulación:", err);
        addErrorToFeed("Error de conexión: " + err.message);
    }

    isRunning = false;
    elements.btnExecute.disabled = false;
    elements.btnExecute.innerHTML = '<span class="btn__icon">⚡</span> Ejecutar Simulación';
    elements.eventInput.value = '';
}

// --- SignalR Handlers ---

connection.on("ReceiveWorldState", (data) => {
    elements.tickerInflation.textContent = `${data.inflation.toFixed(2)}%`;
    elements.tickerRates.textContent = `${data.interestRates.toFixed(2)}%`;
    elements.tickerLiquidity.textContent = data.globalLiquidity.toFixed(1);

    // Sync sliders
    elements.sliderInflation.value = data.inflation;
    elements.valInflation.textContent = `${data.inflation}%`;
    elements.sliderRates.value = data.interestRates;
    elements.valRates.textContent = `${data.interestRates}%`;
    elements.sliderLiquidity.value = data.globalLiquidity;
    elements.valLiquidity.textContent = data.globalLiquidity;
});

connection.on("AgentThinking", (agentName) => {
    const meta = agentMeta[agentName] || { icon: '?', shortName: '??', label: agentName };
    const card = createAgentCard(agentName, meta, null, true);
    elements.agentFeed.appendChild(card);
    elements.agentFeed.scrollTop = elements.agentFeed.scrollHeight;
});

connection.on("ReceiveAgentResponse", (data) => {
    // Find the thinking card and replace it
    const thinkingCard = elements.agentFeed.querySelector(`[data-agent="${data.name}"].agent-card--thinking`);
    if (thinkingCard) thinkingCard.remove();

    const meta = agentMeta[data.name] || { icon: '?', shortName: '??', label: data.name };
    const card = createAgentCard(data.name, meta, data, false);
    elements.agentFeed.appendChild(card);
    elements.agentFeed.scrollTop = elements.agentFeed.scrollHeight;
});

connection.on("CIOThinking", () => {
    elements.cioPanel.innerHTML = `
        <div class="cio__empty" style="animation: pulse 1.5s infinite;">
            <p>🧠 CIO sintetizando reacciones del mercado...</p>
        </div>`;
});

connection.on("ReceiveCIOReport", (data) => {
    // Render narrative with Markdown formatting
    elements.cioPanel.innerHTML = `<div class="cio__narrative">${parseMarkdown(data.narrative)}</div>`;

    // Parse JSON data if available
    if (data.jsonData) {
        try {
            const json = JSON.parse(data.jsonData);

            // Probabilities
            if (json.projections?.short_term) {
                const st = json.projections.short_term;
                elements.probBull.style.width = `${st.bullish || 33}%`;
                elements.probBull.querySelector('span').textContent = `Bull ${st.bullish || 0}%`;
                elements.probNeutral.style.width = `${st.neutral || 34}%`;
                elements.probNeutral.querySelector('span').textContent = `Neutral ${st.neutral || 0}%`;
                elements.probBear.style.width = `${st.bearish || 33}%`;
                elements.probBear.querySelector('span').textContent = `Bear ${st.bearish || 0}%`;
                elements.cioProbs.classList.remove('hidden');
            }

            // Conviction score
            if (json.conviction_score !== undefined) {
                elements.convictionScore.textContent = json.conviction_score;
                const score = parseFloat(json.conviction_score);
                if (score > 0) elements.convictionScore.style.color = 'var(--accent-bull)';
                else if (score < 0) elements.convictionScore.style.color = 'var(--accent-bear)';
                else elements.convictionScore.style.color = 'var(--accent-neutral)';
            }

            // JSON raw
            elements.jsonBlock.textContent = JSON.stringify(json, null, 2);
            elements.cioJson.classList.remove('hidden');
        } catch (e) {
            console.warn("No se pudo parsear JSON del CIO:", e);
        }
    }
});

connection.on("ReceiveMarketSentiment", (data) => {
    // Update ticker
    elements.tickerSentiment.textContent = data.sentiment;

    // Color sentiment badge
    const sentEl = elements.tickerSentiment;
    if (data.riskIndex > 0.1) {
        sentEl.style.background = 'rgba(0, 255, 136, 0.1)';
        sentEl.style.borderColor = 'rgba(0, 255, 136, 0.3)';
        sentEl.style.color = 'var(--accent-bull)';
    } else if (data.riskIndex < -0.1) {
        sentEl.style.background = 'rgba(255, 77, 106, 0.1)';
        sentEl.style.borderColor = 'rgba(255, 77, 106, 0.3)';
        sentEl.style.color = 'var(--accent-bear)';
    } else {
        sentEl.style.background = 'rgba(148, 163, 184, 0.1)';
        sentEl.style.borderColor = 'rgba(148, 163, 184, 0.2)';
        sentEl.style.color = 'var(--accent-neutral)';
    }

    // Update gauge
    const gaugePercent = ((data.riskIndex + 1) / 2) * 100; // -1..1 -> 0..100
    elements.riskGaugeMarker.style.left = `${gaugePercent}%`;
    elements.riskGaugeScore.textContent = data.riskIndex.toFixed(2);

    // Color score
    if (data.riskIndex > 0.1) elements.riskGaugeScore.style.color = 'var(--accent-bull)';
    else if (data.riskIndex < -0.1) elements.riskGaugeScore.style.color = 'var(--accent-bear)';
    else elements.riskGaugeScore.style.color = 'var(--text-primary)';
});

connection.on("ReceiveAgentProfiles", (profiles) => {
    console.log("Agent profiles loaded:", profiles);
});

connection.on("SimulationReset", () => {
    // Limpiar feed
    elements.agentFeed.innerHTML = `
        <div class="feed__empty">
            <div class="feed__empty-icon">🏛</div>
            <p>Simulación reseteada. Todos los agentes tienen memoria limpia.</p>
            <p class="feed__hint">Ingresa un nuevo evento para comenzar un escenario fresco.</p>
        </div>`;

    // Limpiar CIO
    elements.cioPanel.innerHTML = '<div class="cio__empty"><p>El Analista CIO sintetizará las reacciones después de la simulación.</p></div>';
    elements.cioProbs.classList.add('hidden');
    elements.cioJson.classList.add('hidden');

    // Reset gauge
    elements.riskGaugeMarker.style.left = '50%';
    elements.riskGaugeScore.textContent = '0.00';
    elements.riskGaugeScore.style.color = 'var(--text-primary)';

    console.log('✅ Simulación reseteada');
});

// --- UI Helpers ---

function createAgentCard(name, meta, data, isThinking) {
    const card = document.createElement('div');
    card.className = `agent-card ${isThinking ? 'agent-card--thinking' : ''}`;
    card.dataset.agent = name;

    const color = data?.color || '#666';
    const riskScore = data?.riskScore ?? 0;
    const riskColor = riskScore > 0 ? 'var(--accent-bull)' : riskScore < 0 ? 'var(--accent-bear)' : 'var(--accent-neutral)';

    let ordersHtml = '';
    if (data?.orders?.length) {
        ordersHtml = '<div class="agent-card__orders">' +
            data.orders.map(order => {
                const lc = order.toLowerCase();
                const cls = lc.includes('buy') ? 'order-tag--buy' : lc.includes('sell') ? 'order-tag--sell' : 'order-tag--other';
                return `<span class="order-tag ${cls}">${escapeHtml(order)}</span>`;
            }).join('') +
            '</div>';
    }

    card.innerHTML = `
        <div class="agent-card__header">
            <div class="agent-card__avatar" style="background: ${color}20; color: ${color}; border: 1px solid ${color}40;">
                ${meta.icon}
            </div>
            <span class="agent-card__name" style="color: ${color};">${name}</span>
            ${!isThinking ? `<span class="agent-card__risk" style="background: ${riskColor}15; color: ${riskColor}; border: 1px solid ${riskColor}30;">${data?.riskTag || `RISK: ${riskScore.toFixed(1)}`}</span>` : ''}
        </div>
        <div class="agent-card__body">${isThinking ? 'Analizando evento macroeconómico...' : parseMarkdown(data?.response || '')}</div>
        ${ordersHtml}
    `;

    return card;
}

function addErrorToFeed(message) {
    const el = document.createElement('div');
    el.className = 'agent-card';
    el.style.borderColor = 'var(--accent-bear)';
    el.innerHTML = `<div class="agent-card__body" style="color: var(--accent-bear);">❌ ${escapeHtml(message)}</div>`;
    elements.agentFeed.appendChild(el);
}

// --- Settings ---

// Track si las keys están enmascaradas (vienen del backend) o el usuario las editó
let settingsKeysEdited = {
    openRouterKey: false,
    openAIKey: false,
    googleKey: false,
};

// Marcar campos como editados cuando el usuario los toca
['settingOpenRouterKey', 'settingOpenAIKey', 'settingGoogleKey'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.addEventListener('input', () => {
        const keyName = id.replace('setting', '').replace('Key', 'Key');
        // Map: settingOpenRouterKey -> openRouterKey, etc.
        if (id === 'settingOpenRouterKey') settingsKeysEdited.openRouterKey = true;
        if (id === 'settingOpenAIKey') settingsKeysEdited.openAIKey = true;
        if (id === 'settingGoogleKey') settingsKeysEdited.googleKey = true;
    });
});

// Almacén temporal de keys reales (las que vienen sin máscara al guardar)
let _realKeys = { openRouterKey: '', openAIKey: '', googleKey: '' };

async function saveSettings() {
    const provider = document.getElementById('settingProvider').value;
    const openRouterKey = settingsKeysEdited.openRouterKey
        ? document.getElementById('settingOpenRouterKey').value
        : _realKeys.openRouterKey || document.getElementById('settingOpenRouterKey').value;
    const openRouterModel = document.getElementById('settingOpenRouterModel').value;
    const openAIKey = settingsKeysEdited.openAIKey
        ? document.getElementById('settingOpenAIKey').value
        : _realKeys.openAIKey || document.getElementById('settingOpenAIKey').value;
    const openAIModel = document.getElementById('settingOpenAIModel').value;
    const googleKey = settingsKeysEdited.googleKey
        ? document.getElementById('settingGoogleKey').value
        : _realKeys.googleKey || document.getElementById('settingGoogleKey').value;
    const googleModel = document.getElementById('settingGoogleModel').value;
    const maxTokens = parseInt(document.getElementById('settingMaxTokens').value) || 800;
    const temperature = parseFloat(document.getElementById('settingTemperature').value) || 0.2;

    try {
        await connection.invoke("SaveSettings",
            provider, openRouterKey, openRouterModel,
            openAIKey, openAIModel, googleKey, googleModel,
            maxTokens, temperature);
    } catch (err) {
        alert('❌ Error al guardar: ' + err.message);
    }
}

// Handler: confirmación del backend
connection.on("SettingsSaved", (data) => {
    if (data.success) {
        alert('✅ ' + data.message);
        // Reset edit flags
        settingsKeysEdited = { openRouterKey: false, openAIKey: false, googleKey: false };
    } else {
        alert('❌ ' + data.message);
    }
});

// Handler: recibir settings actuales (keys enmascaradas)
connection.on("ReceiveSettings", (data) => {
    if (data.provider) document.getElementById('settingProvider').value = data.provider;
    if (data.openRouterKey) document.getElementById('settingOpenRouterKey').value = data.openRouterKey;
    if (data.openRouterModel) document.getElementById('settingOpenRouterModel').value = data.openRouterModel;
    if (data.openAIKey) document.getElementById('settingOpenAIKey').value = data.openAIKey;
    if (data.openAIModel) document.getElementById('settingOpenAIModel').value = data.openAIModel;
    if (data.googleKey) document.getElementById('settingGoogleKey').value = data.googleKey;
    if (data.googleModel) document.getElementById('settingGoogleModel').value = data.googleModel;
    if (data.maxTokens) document.getElementById('settingMaxTokens').value = data.maxTokens;
    if (data.temperature !== undefined) document.getElementById('settingTemperature').value = data.temperature;

    // Indicador visual de que hay .env
    const hint = document.querySelector('.settings__hint');
    if (!data.hasEnvFile && hint) {
        hint.innerHTML = '⚠️ No se encontró archivo <code>.env</code>. Ingresa tus credenciales y guarda para crear uno.';
        hint.style.color = 'var(--accent-bear)';
    }
});

async function applyInitialState() {
    const inf = parseFloat(document.getElementById('settingInflation').value) || 3.1;
    const rates = parseFloat(document.getElementById('settingRates').value) || 5.25;
    const liq = parseFloat(document.getElementById('settingLiquidity').value) || 100;

    try {
        await connection.invoke("UpdateWorldState", inf, rates, liq);
        alert('✅ WorldState actualizado.');
    } catch (err) {
        alert('❌ Error: ' + err.message);
    }
}

// --- Event: Enter to execute ---
elements.eventInput.addEventListener('keydown', (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        runSimulation();
    }
});

// --- Connection Start ---
async function startConnection() {
    try {
        await connection.start();
        console.log("✅ SignalR conectado");
        await connection.invoke("GetInitialState");
    } catch (err) {
        console.error("❌ SignalR error:", err);
        setTimeout(startConnection, 3000);
    }
}

connection.onclose(async () => {
    console.warn("⚠ SignalR desconectado, reconectando...");
    await startConnection();
});

// --- Init ---
startConnection();
