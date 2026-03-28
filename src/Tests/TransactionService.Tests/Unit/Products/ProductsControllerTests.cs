using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.Products.Contracts;
using TransactionService.Api.Products.Controllers;
using TransactionService.Application.Products.Commands;
using TransactionService.Application.Products.Dtos;

namespace TransactionService.Tests.Unit.Products;

public class ProductsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductsController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var request = new CreateProductRequest(
            Guid.NewGuid(),
            "Flat White",
            "Coffee",
            5.5m,
            null,
            1);

        var response = ServiceResult<ProductDto>.Ok(
            new ProductDto
            {
                PublicId = Guid.NewGuid(),
                CategoryPublicId = request.CategoryPublicId,
                CategoryName = "Coffee",
                Name = "Flat White",
                Description = "Coffee",
                Price = 5.5m,
                DisplayOrder = 1,
                IsAvailable = true
            },
            ResultCodes.Product.CreateSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CreateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<ProductDto>>();

        var body = (ApiResponse<ProductDto>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Product.CreateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("Flat White");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenMediatorReturnsFailure()
    {
        var request = new CreateProductRequest(
            Guid.NewGuid(),
            "",
            null,
            0m,
            null,
            null);

        var response = ServiceResult<ProductDto>.Fail(
            ResultCodes.Product.CreateParameterError,
            "Product name is required.");

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CreateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().BeOfType<ApiResponse<ProductDto>>();

        var body = (ApiResponse<ProductDto>)badRequest.Value!;
        body.Code.Should().Be(ResultCodes.Product.CreateParameterError);
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var publicId = Guid.NewGuid();
        var request = new UpdateProductRequest(
            Guid.NewGuid(),
            "Latte",
            "Coffee",
            5.8m,
            null,
            2,
            true);

        var response = ServiceResult<ProductDto>.Ok(
            new ProductDto
            {
                PublicId = publicId,
                CategoryPublicId = request.CategoryPublicId,
                CategoryName = "Coffee",
                Name = "Latte",
                Description = "Coffee",
                Price = 5.8m,
                DisplayOrder = 2,
                IsAvailable = true
            },
            ResultCodes.Product.UpdateSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<UpdateProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Update(publicId, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<ProductDto>>();

        var body = (ApiResponse<ProductDto>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Product.UpdateSuccess);
        body.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("Latte");
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var publicId = Guid.NewGuid();

        var response = ServiceResult<bool>.Ok(
            true,
            ResultCodes.Product.DeleteSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<DeleteProductCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Delete(publicId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<bool>>();

        var body = (ApiResponse<bool>)ok.Value!;
        body.Code.Should().Be(ResultCodes.Product.DeleteSuccess);
        body.Data.Should().BeTrue();
    }
}