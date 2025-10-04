-- db version: 0.1.0
CREATE DATABASE peeldb;

-- types
CREATE TYPE offer_type_filter AS ENUM ('all', 'sell', 'buy');
CREATE TYPE currency_type AS ENUM ('sat', 'btc', 'fiat');

-- tables
CREATE TABLE search_history (
  search_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  offer_type offer_type_filter,
  amount_currency currency_type,
  amount_range int4range,
  max_spread REAL,
  min_reputation DOUBLE PRECISION,
  occurred_at TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

  CONSTRAINT currency_range_nullability CHECK (
    (amount_currency IS NULL AND amount_range IS NULL) OR
    (amount_currency IS NOT NULL AND amount_range IS NOT NULL)
  )
);
