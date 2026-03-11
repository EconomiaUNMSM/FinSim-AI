# FinSim-AI | Quantum Market Simulator 🚀

[English](README.md) | [Español](README.es.md) | [Português](README.pt.md) | [中文](README.zh.md)

FinSim-AI es un simulador avanzado de dinámicas de mercado financiero impulsado por múltiples agentes de Inteligencia Artificial con incentivos y mandatos conflictivos. A diferencia de las simulaciones lineales, este motor explora el comportamiento emergente del mercado a través de una arquitectura multi-agente aislada.


![Demo de FinSim-AI](assets/video_muestra_2.gif)

---

## 📊 Dinámicas de Simulación
Después de ejecutar una simulación ante un evento macroeconómico o un shock ambiental, el sistema genera un flujo causal de reacciones:

![Output de Simulación](assets/app_final.png)

### El Output:
- **Reacciones de Agentes**: Cada agente (Retail, Hedge Fund, Institucional) emite un juicio subjetivo y un "Risk Score".
- **Order Flow**: El agente Market Maker (Citadel-MM) procesa las órdenes agregadas para determinar la liquidez real.
- **CIO Report**: Un quinto agente sintetiza todo en una narrativa estratégica, calculando probabilidades (Bull/Bear/Neutral) y un índice de convicción del mercado.

---

## 🧠 Metodología: Aislamiento Cognitivo
El sustento de FinSim-AI reside en que cada agente opera en un entorno de aislamiento cognitivo durante su turno de decisión.

![Metodología](assets/app_metodologia.png)

### Conceptos Clave:
1. **Incentivos Opuestos**: Cada agente tiene una cartera y un mandato específico (ej. Vanguard busca seguridad, Aura busca asimetría).
2. **Memoria Compartida**: Al final de cada turno, el reporte del CIO se inyecta en la memoria de todos los agentes, creando una "consciencia de mercado" para el siguiente ciclo.
3. **Causalidad Secuencial**: Las reacciones no son aleatorias; se basan en el estado actual de Inflación, Tasas y Liquidez Global.

---

## ⚙️ Configuración y Motor de IA
FinSim-AI es agnóstico al modelo. Puedes configurar diversos cerebros para tus agentes.

![Configuración](assets/app_settings.png)

### Parámetros Ajustables:
- **Proveedores**: Soporte para OpenRouter, OpenAI y Google Gemini.
- **Modelos**: Desde modelos ligeros (flash) hasta modelos razonadores potentes (o1, gpt-4o).
- **Parámetros del Motor**:
    - **Max Tokens**: Controla la profundidad de la narrativa de los agentes.
    - **Temperature**: Ajusta el determinismo vs. creatividad en las reacciones.
- **Estado Inicial**: Define el punto de partida macroeconómico (Inflación, Tasas y Liquidez).

---

## 🛠️ Ejecución y Desarrollo
Si deseas ejecutar el proyecto desde el código fuente o contribuir al desarrollo, sigue estos pasos:

### Prerrequisitos
- Tener instalado el **.NET SDK** (v8.0 o superior).

### Pasos
1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/EconomiaUNMSM/FinSim-AI.git
   cd FinSim-AI
   ```
2. **Instalar dependencias**:
   ```bash
   dotnet restore
   ```
3. **Ejecutar la aplicación**:
   ```bash
   dotnet run
   ```
   *La aplicación se abrirá automáticamente en tu navegador predeterminado (usualmente en http://localhost:5000).*

---

## 🎯 Utilidad, Casos de Uso y Limitaciones

### Utilidad
FinSim-AI sirve para explorar escenarios "What If" (¿Qué pasaría si...?) en entornos financieros complejos, permitiendo entender cómo la psicología de diferentes participantes interactúa ante noticias macroeconómicas.

### Casos de Uso
- **Educación Financiera**: Comprender la relación entre tasas de interés y apetito de riesgo.
- **Análisis de Narrativa**: Observar cómo una noticia puede ser interpretada de formas opuestas por distintos sectores.
- **Wargaming Financiero**: Simular ataques especulativos o crisis de liquidez.

### Limitaciones
- **Naturaleza Sintética**: Las reacciones dependen de la calidad del modelo de lenguaje utilizado.
- **Falta de Ejecución Real**: Es un simulador cualitativo, no un motor de trading cuantitativo. No procesa datos de mercado en tiempo real.

---

## ⚠️ Disclaimer (Descargo de Responsabilidad)
**ESTA ES UNA HERRAMIENTA EXCLUSIVAMENTE EDUCATIVA Y DE ENTRETENIMIENTO.**
FinSim-AI no constituye, ni debe ser interpretado como:
- Asesoría financiera profesional.
- Recomendaciones de inversión o trading.
- Proyecciones reales de comportamiento de mercado.

Las inversiones financieras conllevan riesgos significativos. Siempre consulte con un asesor financiero certificado antes de tomar cualquier decisión de inversión real. El autor no se hace responsable de las pérdidas incurridas por el uso de esta herramienta.

---
*Desarrollado por EconomiaUNMSM.*
