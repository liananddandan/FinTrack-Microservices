namespace GatewayService.Application.Doc.Abstractions;

public interface IOpenApiDocumentMerger
{
    string Merge(params (string Name, string Json)[] documents);
}