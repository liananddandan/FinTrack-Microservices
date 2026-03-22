using System.Text.Json;
using System.Text.Json.Nodes;
using GatewayService.Application.Services.Interfaces;

namespace GatewayService.Application.Services;

public class OpenApiDocumentMerger : IOpenApiDocumentMerger
{
    public string Merge(params (string Name, string Json)[] documents)
    {
        if (documents.Length == 0)
        {
            throw new InvalidOperationException("No OpenAPI documents were provided.");
        }

        JsonObject? mergedRoot = null;
        JsonObject mergedPaths = new();
        JsonObject mergedSchemas = new();
        JsonObject mergedResponses = new();
        JsonObject mergedParameters = new();
        JsonObject mergedExamples = new();
        JsonObject mergedRequestBodies = new();
        JsonObject mergedHeaders = new();
        JsonObject mergedSecuritySchemes = new();
        JsonObject mergedLinks = new();
        JsonObject mergedCallbacks = new();

        foreach (var (name, json) in documents)
        {
            var root = JsonNode.Parse(json)?.AsObject()
                       ?? throw new InvalidOperationException($"Failed to parse OpenAPI document: {name}");

            if (mergedRoot is null)
            {
                mergedRoot = new JsonObject
                {
                    ["openapi"] = root["openapi"]?.DeepClone() ?? "3.0.1",
                    ["info"] = new JsonObject
                    {
                        ["title"] = "Transaction & Workflow Platform API Docs",
                        ["version"] = "v1"
                    },
                    ["paths"] = mergedPaths,
                    ["components"] = new JsonObject
                    {
                        ["schemas"] = mergedSchemas,
                        ["responses"] = mergedResponses,
                        ["parameters"] = mergedParameters,
                        ["examples"] = mergedExamples,
                        ["requestBodies"] = mergedRequestBodies,
                        ["headers"] = mergedHeaders,
                        ["securitySchemes"] = mergedSecuritySchemes,
                        ["links"] = mergedLinks,
                        ["callbacks"] = mergedCallbacks
                    }
                };
            }

            MergePathSection(root["paths"]?.AsObject(), mergedPaths, name);

            var components = root["components"]?.AsObject();
            if (components is not null)
            {
                MergeNamedSection(components["schemas"]?.AsObject(), mergedSchemas, $"{name} / components.schemas");
                MergeNamedSection(components["responses"]?.AsObject(), mergedResponses, $"{name} / components.responses");
                MergeNamedSection(components["parameters"]?.AsObject(), mergedParameters, $"{name} / components.parameters");
                MergeNamedSection(components["examples"]?.AsObject(), mergedExamples, $"{name} / components.examples");
                MergeNamedSection(components["requestBodies"]?.AsObject(), mergedRequestBodies, $"{name} / components.requestBodies");
                MergeNamedSection(components["headers"]?.AsObject(), mergedHeaders, $"{name} / components.headers");
                MergeNamedSection(components["securitySchemes"]?.AsObject(), mergedSecuritySchemes, $"{name} / components.securitySchemes");
                MergeNamedSection(components["links"]?.AsObject(), mergedLinks, $"{name} / components.links");
                MergeNamedSection(components["callbacks"]?.AsObject(), mergedCallbacks, $"{name} / components.callbacks");
            }
        }

        if (mergedRoot is null)
        {
            throw new InvalidOperationException("Merged OpenAPI root document could not be created.");
        }

        return mergedRoot.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static void MergePathSection(JsonObject? sourcePaths, JsonObject targetPaths, string sourceName)
    {
        if (sourcePaths is null)
        {
            return;
        }

        foreach (var kvp in sourcePaths)
        {
            var pathKey = kvp.Key;
            if (pathKey.Contains("/internal"))
            {
                continue;
            }
            var pathValue = kvp.Value?.DeepClone();

            if (pathValue is null)
            {
                continue;
            }

            if (targetPaths.ContainsKey(pathKey))
            {
                throw new InvalidOperationException(
                    $"Duplicate OpenAPI path detected: '{pathKey}' from '{sourceName}'.");
            }

            targetPaths[pathKey] = pathValue;
        }
    }

    private static void MergeNamedSection(JsonObject? sourceSection, JsonObject targetSection, string sectionName)
    {
        if (sourceSection is null)
        {
            return;
        }

        foreach (var kvp in sourceSection)
        {
            var key = kvp.Key;
            var value = kvp.Value?.DeepClone();

            if (value is null)
            {
                continue;
            }

            if (targetSection.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Duplicate OpenAPI component detected: '{key}' in '{sectionName}'.");
            }

            targetSection[key] = value;
        }
    }
}