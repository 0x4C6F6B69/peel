"""Configuration management for the console client."""

from dataclasses import dataclass
from typing import Literal


@dataclass
class ApiConfig:
    """API configuration settings."""

    base_url: str = "http://localhost:5000"
    timeout: int = 30


@dataclass
class AppConfig:
    """Application configuration."""

    api: ApiConfig
    environment: Literal["dev", "prod"] = "dev"
    default_fiat: str = "EUR"

    @classmethod
    def default(cls) -> "AppConfig":
        """Create default configuration."""
        return cls(
            api=ApiConfig(),
            environment="dev",
            default_fiat="EUR"
        )
