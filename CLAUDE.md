# Peel - Architecture Overview

## Purpose
Web API providing a flexible layer for exploring Peach Bitcoin offers with enhanced filtering capabilities. Critical context: Peach API removed native filtering from search endpoints, forcing client-side implementation.

## Solution Structure

### Core Project (`src/Core`)
Business logic layer transforming PeachClient offers into `OfferSummary` types.

**Key Dependencies:**
- `PeachClient`: .NET client for Peach Bitcoin API
- `SharpX`: Functional programming utilities

**Core Components:**
#### Services (`src/Core/Services`)
- **OfferReader**: Main facade for offer retrieval
  - Paginates through Peach API (100/page, 150ms cooldown)
  - Implements client-side filtering (amount, spread, reputation) since Peach API lacks it
  - Supports both `Sell`/`Buy` filters via `OfferTypeFilter`
  - Returns `Result<List<OfferSummary>>` with errors collection
  - Advanced criteria: flexible payment method/fiat filtering post-fetch
- **MarketAnalyzer**: BTC volatility and pricing
  - Computes volatility from Binance candles (1m intervals)
  - Fetches BTC market price via PeachClient
- **BinanceClient**: Direct Binance API integration for market data

#### Models (`src/Core/Models`)
- **OfferSummary**: Primary domain model
- **OfferSummaryFlat**: Flattened for CSV export
- **OfferSearchCriteria**: Abstract base for filtering

#### Infrastructure (`src/Core/Infrastructure`)
- **Mapper**: PeachClient `Offer` → `OfferSummary` conversion
- **Converter**: Unit conversions

### WebApi Project (`src/WebApi`)
ASP.NET Core minimal API exposing endpoints.

**Key Handlers:**
#### OffersHandler (`src/WebApi/Handlers/OffersHandler.cs:18`)
- **POST** `/offers/summary`: Search offers with criteria
  - Query params: `format` (Default/Flat/Csv), `groupBy` (None/Spread/FiatPrice)
  - Returns `SummaryResponse<T>` with errors, BTC price, default fiat
  - CSV output via `CsvExtensions.ToCsvTextAsync`
- **GET** `/offers/summary/{id}` - Single offer by summary ID

#### MarketHandler (`src/WebApi/Handlers/MarketHandler.cs`)
- **GET** `/market/volatility/{hours}`: BTC volatility analysis
- **GET** `/market/price/BTC`: Current BTC market price

**Configuration:**
- `SystemConfig`: Default fiat currency
- `BinanceConfig`: Binance API settings
- `PeachApiClientSettings`: PeachClient configuration

## Critical Implementation Details

### Client-Side Filtering Workaround
`OfferReader.cs` implements all filtering logic because Peach API no longer supports filter parameters.
Filters applied:
- Amount range (converted to satoshis)
- Spread min/max (premium percentage)
- Minimum user reputation
- Payment methods (Advanced criteria only, post-mapping)

### Error Handling
`Result<T>` type accumulates non-fatal errors (e.g., failed pagination pages) while allowing partial success. Max N failures before abort.

### Implementation Policies
- Keep the code as clean and simple as possible, while adhering to the existing style
- Do not remove features arbitrarily unless explicitly requested
- Do not create unit tests unless explicitly requested
