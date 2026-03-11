# FinSim-AI | Quantum Market Simulator 🚀

[English](README.md) | [Español](README.es.md) | [Português](README.pt.md) | [中文](README.zh.md)

FinSim-AI é um simulador avançado de dinâmicas do mercado financeiro impulsionado por múltiplos agentes de Inteligência Artificial com incentivos e mandatos conflitantes. Ao contrário das simulações lineares, este motor explora o comportamento emergente do mercado através de uma arquitetura multiagente isolada.


![Demo do FinSim-AI](assets/video_muestra_2.gif)

---

## 📊 Dinâmicas de Simulação
Após a execução de uma simulação diante de um evento macroeconômico ou um choque ambiental, o sistema gera um fluxo causal de reações:

![Output da Simulação](assets/app_final.png)

### O Output:
- **Reações dos Agentes**: Cada agente (Varejo, Fundo de Cobertura, Institucional) emite um julgamento subjetivo e uma "Pontuação de Risco" (Risk Score).
- **Fluxo de Ordens**: O agente Formador de Mercado (Citadel-MM) processa as ordens agregadas para determinar a liquidez real.
- **Relatório do CIO**: Um quinto agente sintetiza tudo em uma narrativa estratégica, calculando probabilidades (Alta/Baixa/Neutra) e um índice de convicção do mercado.

---

## 🧠 Metodologia: Isolamento Cognitivo
A base do FinSim-AI reside no fato de que cada agente opera em um ambiente de isolamento cognitivo durante seu turno de decisão.

![Metodologia](assets/app_metodologia.png)

### Conceitos-Chave:
1. **Incentivos Opostos**: Cada agente possui um portfólio e um mandato específico (ex: a Vanguard busca segurança, a Aura busca assimetria).
2. **Memória Compartilhada**: No final de cada turno, o relatório do CIO é injetado na memória de todos os agentes, criando uma "consciência de mercado" para o próximo ciclo.
3. **Causalidade Sequencial**: As reações não são aleatórias; elas se baseiam no estado atual da Inflação, das Taxas de Juros e da Liquidez Global.

---

## ⚙️ Configuração e Motor de IA
O FinSim-AI é agnóstico em relação ao modelo. Você pode configurar vários cérebros para seus agentes.

![Configuração](assets/app_settings.png)

### Parâmetros Ajustáveis:
- **Provedores**: Suporte para OpenRouter, OpenAI e Google Gemini.
- **Modelos**: Desde modelos leves (flash) até modelos de raciocínio poderosos (o1, gpt-4o).
- **Parâmetros do Motor**:
    - **Max Tokens**: Controla a profundidade da narrativa dos agentes.
    - **Temperature**: Ajusta o determinismo vs. a criatividade nas reações.
- **Estado Inicial**: Define o ponto de partida macroeconômico (Inflação, Taxas de Juros e Liquidez).

---

## 🛠️ Execução e Desenvolvimento
Se você deseja executar o projeto a partir do código-fonte ou contribuir para o desenvolvimento, siga estas etapas:

### Pré-requisitos
- Ter o **.NET SDK** (v8.0 ou superior) instalado.

### Etapas
1. **Clonar o repositório**:
   ```bash
   git clone https://github.com/EconomiaUNMSM/FinSim-AI.git
   cd FinSim-AI
   ```
2. **Instalar dependências**:
   ```bash
   dotnet restore
   ```
3. **Executar o aplicativo**:
   ```bash
   dotnet run
   ```
   *O aplicativo será aberto automaticamente no seu navegador padrão (geralmente em http://localhost:5000).*

---

## 🎯 Utilidade, Casos de Uso e Limitações

### Utilidade
O FinSim-AI serve para explorar cenários "What If" (O que aconteceria se...?) em ambientes financeiros complexos, permitindo entender como a psicologia dos diferentes participantes interage diante das notícias macroeconômicas.

### Casos de Uso
- **Educação Financeira**: Compreender a relação entre as taxas de juros e o apetite por risco.
- **Análise de Narrativa**: Observar como uma notícia pode ser interpretada de formas opostas por diferentes setores.
- **Wargaming Financeiro**: Simular ataques especulativos ou crises de liquidez.

### Limitações
- **Natureza Sintética**: As reações dependem da qualidade do modelo de linguagem utilizado.
- **Falta de Execução Real**: É um simulador qualitativo, não um motor de negociação (trading) quantitativo. Não processa dados de mercado em tempo real.

---

## ⚠️ Disclaimer (Aviso de Responsabilidade)
**ESTA É UMA FERRAMENTA EXCLUSIVAMENTE EDUCACIONAL E DE ENTRETENIMENTO.**
O FinSim-AI não constitui, nem deve ser interpretado como:
- Orientação financeira profissional.
- Recomendações de investimento ou negociação (trading).
- Projeções reais de comportamento do mercado.

Os investimentos financeiros envolvem riscos significativos. Consulte sempre um consultor financeiro certificado antes de tomar qualquer decisão de investimento real. O autor não se responsabiliza pelas perdas decorrentes do uso desta ferramenta.

---
*Desenvolvido por EconomiaUNMSM.*
