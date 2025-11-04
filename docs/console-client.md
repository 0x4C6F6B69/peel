### Peel API Console Client

#### Purpose
Provide a lightweight, interactive console UI client in Python 3 to consume the **Peel API** (RESTful backend).
Supports only **interactive mode** not **command-line invocation**.
Prioritizes usability, discoverability and future extensibility.

#### Dependencies
Explicitly versioned for reproducibility:
```
httpx == 0.28.1    # HTTP calls (sync/async), JSON handling
typer == 0.20.0    # CLI app structure, auto-help, commands
rich == 14.2.0     # Formatted output: tables, syntax highlighting, colors
inquirerpy ==0.3.4 # Interactive prompts: menus, forms, defaults
```

#### UI Structure & Interaction Model

##### Navigation Hierarchy
Almost flat, resource-centric design:
```
Main Context
├── Offers
│   └── Search
├── Settings
│   ├── Show config
└── Exit
```

##### Prompt Behavior (Interactive Flow)

Each selection uses `InquirerPy.list_prompt()` with these rules:

- **Default Value**: Always set where logical (e.g., `status=active`, `env=dev`).
Displayed as `[default: dev]`. Press `Enter` to accept.
- **Selection Keys**:
  - ↑ / ↓ : Move up/down list
  - Enter : Confirm choice
  - Esc   : Return to parent menu
  - Ctrl+C: Exit entire app
- **Tab Cycling**: Used to move to the next input field.

##### Output Rendering
All output uses `rich.print()` for enhanced readability:
- Lists → Borderless table with headers
- JSON/data dumps → Syntax-highlighted, indented
- Status → Color-coded banners:
  - Success: green `[✓] Created user`
  - Error: red `[✗] 404 Not Found`
  - Warning: yellow `[!] Rate limited`

#### Error Handling & Resilience
- Network failure → Retry once, then prompt: `[?] Retry? (Y/n)`
- 4xx errors → Parse `error.message` from JSON, display clearly
- Invalid input → Highlight field, re-prompt (don’t exit)
- Unknown commands → Suggest closest match (fuzzy matching via `difflib`)
