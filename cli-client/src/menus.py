"""Menu navigation and interactive prompts."""

from enum import Enum
from InquirerPy import inquirer
from InquirerPy.base.control import Choice

from ui import console, print_header, print_info, print_panel
from config import AppConfig
from theme import MENU_POINTER


class MainMenuAction(str, Enum):
    """Main menu action choices."""

    OFFERS = "offers"
    SETTINGS = "settings"
    EXIT = "exit"


class OffersMenuAction(str, Enum):
    """Offers submenu action choices."""

    SEARCH = "search"
    BACK = "back"


class SettingsMenuAction(str, Enum):
    """Settings submenu action choices."""

    SHOW = "show"
    BACK = "back"


def show_main_menu() -> MainMenuAction:
    """Display main menu and return selected action."""
    print_header("Peel - Peach Bitcoin Offers Manager")

    choices = [
        Choice(value=MainMenuAction.OFFERS, name="Offers"),
        Choice(value=MainMenuAction.SETTINGS, name="Settings"),
        Choice(value=MainMenuAction.EXIT, name="Exit"),
    ]

    action = inquirer.select(
        message="Select an option:",
        choices=choices,
        pointer=MENU_POINTER,
        default=MainMenuAction.OFFERS,
    ).execute()

    return MainMenuAction(action)


def show_offers_menu() -> OffersMenuAction:
    """Display offers submenu and return selected action."""
    print_header("Offers")

    choices = [
        Choice(value=OffersMenuAction.SEARCH, name="Search Offers"),
        Choice(value=OffersMenuAction.BACK, name="← Back"),
    ]

    action = inquirer.select(
        message="Select an option:",
        choices=choices,
        pointer=MENU_POINTER,
        default=OffersMenuAction.SEARCH,
    ).execute()

    return OffersMenuAction(action)


def show_settings_menu() -> SettingsMenuAction:
    """Display settings submenu and return selected action."""
    print_header("Settings")

    choices = [
        Choice(value=SettingsMenuAction.SHOW, name="Show Configuration"),
        Choice(value=SettingsMenuAction.BACK, name="← Back"),
    ]

    action = inquirer.select(
        message="Select an option:",
        choices=choices,
        pointer=MENU_POINTER,
        default=SettingsMenuAction.SHOW,
    ).execute()

    return SettingsMenuAction(action)


def show_search_offers_stub() -> None:
    """Stub for search offers functionality."""
    console.print()
    print_info("Search offers functionality - coming soon")
    print_panel(
        "[muted]This will allow you to search and filter Bitcoin offers\n"
        "with various criteria including amount, spread, and payment methods.[/muted]",
        title="Search Offers"
    )
    console.print()
    inquirer.confirm(
        message="Press Enter to continue",
        default=True,
    ).execute()


def show_configuration(config: AppConfig) -> None:
    """Display current configuration."""
    console.print()
    config_text = f"""[cyan]Environment:[/cyan] {config.environment}
[cyan]API Base URL:[/cyan] {config.api.base_url}
[cyan]API Timeout:[/cyan] {config.api.timeout}s
[cyan]Default Fiat:[/cyan] {config.default_fiat}"""

    print_panel(config_text, title="Configuration")
    console.print()
    inquirer.confirm(
        message="Press Enter to continue",
        default=True,
    ).execute()
