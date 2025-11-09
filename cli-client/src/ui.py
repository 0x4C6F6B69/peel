"""UI utilities and common display functions."""

from rich.console import Console
from rich.panel import Panel
from rich.table import Table

from theme import THEME, SYMBOLS


console = Console(theme=THEME)


def print_header(text: str) -> None:
    """Print a header with styling."""
    console.print(f"\n[header]{text}[/header]")


def print_success(text: str) -> None:
    """Print a success message."""
    console.print(f"[success]{SYMBOLS['success']}[/success] {text}")


def print_error(text: str) -> None:
    """Print an error message."""
    console.print(f"[error]{SYMBOLS['error']}[/error] {text}")


def print_warning(text: str) -> None:
    """Print a warning message."""
    console.print(f"[warning]{SYMBOLS['warning']}[/warning] {text}")


def print_info(text: str) -> None:
    """Print an info message."""
    console.print(f"[info]{SYMBOLS['info']}[/info] {text}")


def print_panel(content: str, title: str = "") -> None:
    """Print content in a panel."""
    console.print(Panel(content, title=title, border_style="cyan"))


def create_table(title: str, columns: list[str]) -> Table:
    """Create a styled table."""
    table = Table(title=title, show_header=True, header_style="bold cyan", border_style="dim")
    for col in columns:
        table.add_column(col)
    return table


def clear_screen() -> None:
    """Clear the console screen."""
    console.clear()
