"""Main entry point for the Peel console client."""

import sys
from typing import NoReturn

from config import AppConfig
from menus import (
    MainMenuAction,
    OffersMenuAction,
    SettingsMenuAction,
    show_main_menu,
    show_offers_menu,
    show_settings_menu,
    show_search_offers_stub,
    show_configuration,
)
from ui import console, print_success


def handle_offers_menu(config: AppConfig) -> None:
    """Handle offers submenu navigation."""
    while True:
        action = show_offers_menu()

        if action == OffersMenuAction.SEARCH:
            show_search_offers_stub()
        elif action == OffersMenuAction.BACK:
            break


def handle_settings_menu(config: AppConfig) -> None:
    """Handle settings submenu navigation."""
    while True:
        action = show_settings_menu()

        if action == SettingsMenuAction.SHOW:
            show_configuration(config)
        elif action == SettingsMenuAction.BACK:
            break


def run() -> NoReturn:
    """Run the main application loop."""
    config = AppConfig.default()

    try:
        while True:
            action = show_main_menu()

            if action == MainMenuAction.OFFERS:
                handle_offers_menu(config)
            elif action == MainMenuAction.SETTINGS:
                handle_settings_menu(config)
            elif action == MainMenuAction.EXIT:
                console.print()
                print_success("Goodbye!")
                sys.exit(0)

    except KeyboardInterrupt:
        console.print("\n")
        print_success("Interrupted - Goodbye!")
        sys.exit(0)
    except Exception as e:
        console.print(f"\n[error]Unexpected error: {e}[/error]")
        sys.exit(1)


if __name__ == "__main__":
    run()
