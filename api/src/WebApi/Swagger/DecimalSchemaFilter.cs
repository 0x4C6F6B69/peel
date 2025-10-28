﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class DecimalSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(decimal) || context.Type == typeof(decimal?)) {
            schema.Type = "number";
            schema.Format = "decimal";
        }
    }
}
