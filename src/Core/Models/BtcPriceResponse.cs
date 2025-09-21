namespace Peel.Models;

public record class BtcPriceResponse(
    decimal BtcUnitPrice,
    string DefaultFiat
);
