"""Theme and styling constants inspired by Claude Code."""

from rich.theme import Theme

# Claude Code-inspired color scheme
THEME = Theme({
    "info": "cyan",
    "success": "green",
    "warning": "yellow",
    "error": "red",
    "prompt": "bold cyan",
    "header": "bold white",
    "muted": "dim white",
    "highlight": "bold yellow",
})

# Status symbols
SYMBOLS = {
    "success": "✓",
    "error": "✗",
    "warning": "!",
    "info": "→",
    "bullet": "•",
}

# Menu styles
MENU_POINTER = "›"
MENU_STYLE = "cyan"
MENU_HIGHLIGHT_STYLE = "bold cyan"
