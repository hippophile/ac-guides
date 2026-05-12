# Agentic Coding Standards for Banking AI Systems

This guide defines the **AICoE Agentic Standard** — a set of principles and practices that make building AI systems with coding agents (Claude Code, Cursor, Aider, GitHub Copilot) more efficient, more auditable, and significantly higher quality in a regulated banking environment.

---

## 1. Core Coding Principles

These are the non-negotiables. Every AI-assisted codebase in the bank should follow them.

### Modular, Small Functions
**Rule:** Keep functions focused on doing exactly one thing.

**Why it matters:** AI coding agents have a limited "context window" — the amount of code they can hold in memory at once. Small, focused functions let the model reason about a single unit of logic without losing track of what's happening elsewhere. In a banking context, this also makes compliance reviews and security audits far easier.

---

### Strong Typing (TypeScript, C# types, Python Type Hints)
**Rule:** Always define the types for inputs and outputs explicitly.

**Why it matters:** Types act as a contract the AI cannot violate. When a function signature says `CalculateCreditScore(CustomerId: string, Income: decimal): RiskScore`, the agent immediately understands what goes in and what comes out — no guessing, no hallucinations about data shapes. For regulatory systems, this is essential.

---

### Semantic Naming
**Rule:** Name variables and functions as if explaining them to a new colleague — not `x`, `tmp`, or `calc1`.

**Why it matters:** AI models rely on the meaning embedded in names. `CustomerLoanRejectionReason` tells the model everything about intent; `res` tells it nothing. Good names also make audit trails self-documenting.

---

### Docstrings with Intent (The "Why", not just the "What")
**Rule:** Every function should have a docstring that explains *why* it exists, not just what it does.

**Why it matters:** When an AI agent reads code to perform a refactor, it reads comments as instructions. If you only document *what* the code does, the agent may refactor it correctly but destroy the business intent. In banking, the intent often carries regulatory meaning.

**Example:**
```csharp
/// <summary>
/// Applies the EBA-compliant income threshold check for retail loan eligibility.
/// Per Regulation (EU) 2023/2631, rejections must be explainable and documented.
/// </summary>
public LoanDecision EvaluateLoanEligibility(CustomerProfile customer) { ... }
```

---

### Standardized Error Handling
**Rule:** Use consistent, descriptive error types across the codebase. Never swallow exceptions silently.

**Why it matters:** Clear, structured errors allow the agent to read a stack trace and self-correct without human intervention. In production banking systems, they also feed into your incident management and audit logs.

---

## 2. Workflow Practices

### The Single Responsibility Rule
**Rule:** Keep files under 200–300 lines, each focused on one domain concept.

**Why it matters:** A focused file means the AI agent can hold the entire file in its context window. When a file grows beyond ~300 lines, the agent starts "forgetting" the beginning as it reaches the end, which introduces subtle bugs.

---

### Always Keep an Architecture Map
**Rule:** Maintain an up-to-date `architecture.md` or `README.md` that explains how the system's components interact.

**Why it matters:** This acts as a navigation map for the agent. Instead of reading every file to understand the system, it reads the architecture doc first and knows exactly which files are relevant to the current task — saving tokens, time, and money.

---

### Pure Functions and Isolated Side Effects
**Rule:** Avoid hidden global state mutations. Functions should take explicit inputs and produce explicit outputs.

**Why it matters:** Side effects (writing to a database, mutating a shared object, calling an external API) are the primary source of AI-introduced bugs. Pure, predictable functions allow the agent to write unit tests that actually work and give regulators clear data flow to audit.

---

### The Test-First Loop
**Rule:** Before starting any agent session, ensure your test suite (`dotnet test`, `pytest`, `npm test`) is fully operational.

**Why it matters:** This enables the feedback loop: *Write Code → Run Tests → See Errors → Fix*. Without working tests, the agent has no ground truth to validate against. It works blind, and so do you.

---

## 3. Cost & Quality Cheat Sheet

| Strategy | Impact | What to Do |
| :--- | :--- | :--- |
| **Session Resets** | −50% Token Cost | Restart the agent session after each merged task to clear accumulated chat history |
| **Small Files** | −30% Token Cost | Keep files focused and short so the agent reads less noise |
| **Strict Type Schemas** | +40% Output Quality | Use Zod (TS), Pydantic (Python), or record types (C#) to define all data shapes |
| **Failing Tests First** | +50% Fix Speed | Never ask the agent to fix a bug without a failing test to guide it |
| **Architecture Doc** | +35% Precision | A good architecture.md dramatically reduces irrelevant file reads |

---

## 4. Environment Setup

### Use Project-Level Config Files
**Rule:** Use tool-specific config files (`.claudecode/config`, `.aider.conf.yml`, `.cursorrules`) to define coding standards once.

**Why it matters:** You stop repeating instructions in every session. The agent knows your tech stack, your libraries, and your constraints from the first token. Saves cost, ensures consistency.

---

### Always Use a Sandbox / Mock Environment
**Rule:** Give the agent mock data or a sandbox database, never production access during development.

**Why it matters:** Agents waste tokens debugging infrastructure issues (connection strings, permissions, firewall rules) instead of the actual code. In banking, sandbox environments are also a compliance requirement.

---

### Enable Verbose Logging
**Rule:** Your application should support a `--verbose` or `DEBUG` mode that outputs detailed execution traces.

**Why it matters:** Logs are the agent's debugging eyes. Without them, it makes assumptions about what went wrong. With them, it reasons directly from evidence.

---

## 5. AI-Specific Risks in Banking Systems

These are issues unique to using AI coding agents in a regulated environment:

| Risk | What Happens | How to Mitigate |
| :--- | :--- | :--- |
| **Hallucinated Dependencies** | Agent adds a library that doesn't exist or has vulnerabilities | Run `dotnet audit` / `npm audit` after every agent session |
| **Logic Drift** | Agent slowly changes business logic across sessions | Commit small, atomic changes and review every `git diff` before merging |
| **Deprecated APIs** | Agent uses outdated library versions from its training data | Always specify exact versions: "We use .NET 8, EF Core 8.0.4" |
| **Silent Overwrites** | Agent removes existing compliance comments | Always use a diff review tool before committing |
| **Context Overload** | Agent "forgets" early instructions in a long session | Reset sessions frequently; use config files for persistent rules |

---

## 6. Post-Generation Verification Checklist

Before any AI-generated code touches a staging or production system:

- [ ] Run `git diff` — check every changed line, including files you didn't ask the agent to touch
- [ ] Run linting (`dotnet format`, `eslint --fix`, `black`) to normalize style
- [ ] Run the full test suite — not just the tests for the feature you changed
- [ ] Run a security scan (`snyk`, `npm audit`, `dotnet list package --vulnerable`)
- [ ] Confirm no compliance comments or docstrings were silently removed
- [ ] Reset the agent session before starting the next task

---

## 7. Using OpenAPI / OpenSpec at Full Power

For banking APIs, the OpenAPI spec is not documentation — it is a **regulatory contract**.

### Contract-First Development
**Rule:** Define the API spec first, then use it to auto-generate validation schemas (Zod, Pydantic, C# records).

**Why it matters:** This creates a strict contract the agent cannot violate. It eliminates hallucinated data formats, which in financial systems can mean incorrect monetary values or missing regulatory fields.

### Break Specs into Domain Files
**Rule:** Split large specs into domain-specific files (`credit-scoring.yaml`, `fraud-detection.yaml`, `kyc.yaml`).

**Why it matters:** The agent loads only what it needs for the current task, keeping the context window small and focused.

### Annotate Everything
**Rule:** Fill in `description` and `example` fields for every endpoint and property.

**Why it matters:** The agent uses these as anchors to understand intent without reading all the source code. For regulators, these annotations also serve as inline documentation.

### Self-Healing via Contract Tests
**Rule:** Ask the agent to write tests that validate API responses against the OpenAPI spec definition.

**Why it matters:** When the agent's output drifts from the spec, the test catches it automatically — the agent can then self-correct without human intervention.

---

> **Banking Reminder:** Every AI-assisted system deployed at the bank must be explainable, auditable, and compliant with EU AI Act and EBA Guidelines. "The AI generated it" is never an acceptable answer to a regulator. Your standards, tests, and documentation are your evidence.