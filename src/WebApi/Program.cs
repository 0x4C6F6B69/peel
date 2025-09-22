using System.Text.Json;
using System.Text.Json.Serialization;
using PeachClient;
using Peel;
using Peel.Configuration;
using Peel.Infrastructure;
using Peel.Services;
using Peel.Web.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.Converters.Add(new OfferSearchCriteriaConverter());
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.UseAllOfToExtendReferenceSchemas();
    //c.UseInlineDefinitionsForEnums();
    c.SchemaFilter<EnumSchemaFilter>();
    c.SchemaFilter<DecimalSchemaFilter>();
});

builder.Services.AddHealthChecks()
    .AddCheck<PeachHealthCheck>("peach");

builder.Services.AddSingleton<Mapper>();

builder.Services.AddOptions<SystemConfig>()
    .BindConfiguration(SystemConfig.SectionName);

builder.Services.AddOptions<PeachApiClientSettings>()
    .BindConfiguration(PeachApiClientSettings.SectionName);
builder.Services.AddOptions<BinanceConfig>()
    .BindConfiguration(BinanceConfig.SectionName);

builder.Services.AddSingleton<PeachApiClient>()
    .AddSingleton<OfferReader>();
builder.Services.AddSingleton<BinanceClient>()
    .AddSingleton<MarketAnalyzer>();

builder.Services.AddSingleton<OffersHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthHandler()
   .MapMarketHandler();

var offersHandler = app.Services.GetRequiredService<OffersHandler>();
offersHandler.Map(app);

app.Run();
