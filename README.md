# FinSim-AI | Quantum Market Simulator 🚀

[English](README.md) | [Español](README.es.md) | [Português](README.pt.md) | [中文](README.zh.md)

FinSim-AI is an advanced simulator of financial market dynamics powered by multiple Artificial Intelligence agents with conflicting incentives and mandates. Unlike linear simulations, this engine explores the emergent behavior of the market through an isolated multi-agent architecture.


![FinSim-AI Demo](assets/video_muestra_2.gif)

---

## 📊 Simulation Dynamics
After running a simulation given a macroeconomic event or an environmental shock, the system generates a causal flow of reactions:

![Simulation Output](assets/app_final.png)

### The Output:
- **Agent Reactions**: Each agent (Retail, Hedge Fund, Institutional) issues a subjective judgment and a "Risk Score".
- **Order Flow**: The Market Maker agent (Citadel-MM) processes the aggregated orders to determine actual liquidity.
- **CIO Report**: A fifth agent synthesizes everything into a strategic narrative, calculating probabilities (Bull/Bear/Neutral) and a market conviction index.

---

## 🧠 Methodology: Cognitive Isolation
The foundation of FinSim-AI lies in each agent operating in an environment of cognitive isolation during its decision turn.

![Methodology](assets/app_metodologia.png)

### Key Concepts:
1. **Opposing Incentives**: Each agent has a specific portfolio and mandate (e.g., Vanguard seeks safety, Aura seeks asymmetry).
2. **Shared Memory**: At the end of each turn, the CIO report is injected into the memory of all agents, creating a "market consciousness" for the next cycle.
3. **Sequential Causality**: Reactions are not random; they are based on the current state of Inflation, Rates, and Global Liquidity.

---

## ⚙️ Configuration and AI Engine
FinSim-AI is model-agnostic. You can configure various brains for your agents.

![Configuration](assets/app_settings.png)

### Adjustable Parameters:
- **Providers**: Support for OpenRouter, OpenAI, and Google Gemini.
- **Models**: From lightweight models (flash) to powerful reasoning models (o1, gpt-4o).
- **Engine Parameters**:
    - **Max Tokens**: Controls the depth of the agents' narrative.
    - **Temperature**: Adjusts determinism vs. creativity in the reactions.
- **Initial State**: Defines the macroeconomic starting point (Inflation, Rates, and Liquidity).

---

## 🛠️ Execution and Development
If you want to run the project from source or contribute to the development, follow these steps:

### Prerequisites
- Have the **.NET SDK** (v8.0 or higher) installed.

### Steps
1. **Clone the repository**:
   ```bash
   git clone https://github.com/EconomiaUNMSM/FinSim-AI.git
   cd FinSim-AI
   ```
2. **Install dependencies**:
   ```bash
   dotnet restore
   ```
3. **Run the application**:
   ```bash
   dotnet run
   ```
   *The application will automatically open in your default browser (usually at http://localhost:5000).*

---

## 🎯 Utility, Use Cases, and Limitations

### Utility
FinSim-AI serves to explore "What If" scenarios in complex financial environments, allowing one to understand how the psychology of different participants interacts with macroeconomic news.

### Use Cases
- **Financial Education**: Understand the relationship between interest rates and risk appetite.
- **Narrative Analysis**: Observe how a piece of news can be interpreted in opposing ways by different sectors.
- **Financial Wargaming**: Simulate speculative attacks or liquidity crises.

### Limitations
- **Synthetic Nature**: Reactions depend on the quality of the language model used.
- **Lack of Real Execution**: It is a qualitative simulator, not a quantitative trading engine. It does not process real-time market data.

---

## ⚠️ Disclaimer
**THIS IS STRICTLY AN EDUCATIONAL AND ENTERTAINMENT TOOL.**
FinSim-AI does not constitute, nor should it be interpreted as:
- Professional financial advice.
- Investment or trading recommendations.
- Real projections of market behavior.

Financial investments carry significant risks. Always consult with a certified financial advisor before making any real investment decision. The author is not responsible for losses incurred by the use of this tool.

---
*Developed by EconomiaUNMSM.*
