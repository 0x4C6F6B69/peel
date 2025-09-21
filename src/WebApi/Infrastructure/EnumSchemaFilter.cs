using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema == null || context?.Type == null)
            return;

        var type = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!type.IsEnum)
            return;

        var names = Enum.GetNames(type);
        var arr = new OpenApiArray();
        foreach (var name in names) {
            arr.Add(new OpenApiString(name));
        }

        // Add or replace the x-enumNames extension
        schema.Extensions["x-enumNames"] = arr;
    }
}
