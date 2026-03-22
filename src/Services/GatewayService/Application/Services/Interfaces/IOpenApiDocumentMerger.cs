namespace GatewayService.Application.Services.Interfaces;

public interface IOpenApiDocumentMerger
{
    string Merge(params (string Name, string Json)[] documents);
}