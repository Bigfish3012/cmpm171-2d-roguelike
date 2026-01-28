# AGENTS.md
Guidelines for AI assistants and automated tools working in this repository.

This project is a **Unity 2D roguelike** developed by a student team.  
Please follow the rules below strictly to avoid breaking Unity serialization,
Git history, or team workflow.

---

## 1. General Principles
- Prefer **small, minimal changes**
- Do **not refactor** unless explicitly asked
- Preserve existing gameplay behavior
- Ask before making architectural changes
- Clearly explain what was changed and the effect of the change
---

## 2. Unity-Specific Rules (Very Important)
- **NEVER rename serialized fields** in MonoBehaviours or ScriptableObjects
- **NEVER rename public fields** used in the Inspector
- **DO NOT touch `.meta` files** unless explicitly instructed
- **DO NOT modify** `/Library`, `/Temp`, `/Logs`, or `/Build`
- Avoid changing Script Execution Order
- Assume Unity version is fixed for the project

Breaking these rules can corrupt scenes and prefabs.

---

## 3. Project Structure
- `/Assets/Scripts` → Gameplay logic only
- `/Assets/Prefabs` → Prefabs only (do not inline-edit scene objects)
- `/Assets/Art`, `/Assets/Audio` → Assets only, no code
- `/Assets/Scenes` → Do not modify scenes unless requested

Respect existing folder organization.

---

## 4. Coding Style
- Language: **C# (Unity)**
- Keep methods short and readable
- Use clear, descriptive names
- Avoid unnecessary abstractions
- Prefer explicit logic over “clever” solutions

---

## 5. Gameplay & Systems
- This is a **top-down 2D roguelike**
- Movement is not platformer-based
- Floors are usually walkable (no collider)
- Walls and obstacles define collision
- Assume grid/room-based level structure unless told otherwise

---

## 6. Git & Collaboration Rules
- Do not rewrite Git history
- Do not squash commits unless instructed
- Avoid large diffs
- Respect branch boundaries
- Assume multiple teammates are working in parallel
    - Unity version: 6000.2.7f2 (do not upgrade or downgrade)


---

## 7. When in Doubt
If something is unclear:
- Ask before acting
- Explain tradeoffs
- Propose changes instead of applying them directly

Silence is worse than asking.

---

## 8. Scope Awareness
This is a **course project**, not a production game.
Favor clarity, stability, and correctness over optimization or polish.

## 9. Branches
- Do NOT work directly on the main branch
- Feature branches should merge into test-main first
- main should only receive changes that are tested and approved
