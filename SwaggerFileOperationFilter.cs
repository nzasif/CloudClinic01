using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Schema;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile));

        foreach (var fileParam in fileParams)
        {
            var parameter = operation.Parameters.FirstOrDefault(p => p.Name == fileParam.Name);
            if (parameter != null) operation.Parameters.Remove(parameter);

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object", // Fixed: Use string "object" instead of JsonSchemaType.Object
                            Properties = { [fileParam.Name] = new OpenApiSchema { Type = "string", Format = "binary" } },
                            Required = new HashSet<string> { fileParam.Name }
                        }
                    }
                }
            };
        }
    }
}