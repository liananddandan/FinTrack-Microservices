using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedKernel.Common.DTOs;
using SharedKernel.Common.Results;
using TransactionService.Api.ProductCategories.Contracts;
using TransactionService.Api.ProductCategories.Controllers;
using TransactionService.Application.DTOs;
using TransactionService.Application.ProductCategories.Commands;
using TransactionService.Application.ProductCategories.Queries;
using Xunit;

namespace TransactionService.Tests.Unit.ProductCategories;

public class ProductCategoriesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductCategoriesController _controller;

    public ProductCategoriesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductCategoriesController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetList_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var response = ServiceResult<List<ProductCategoryDto>>.Ok(
            new List<ProductCategoryDto>
            {
                new()
                {
                    PublicId = Guid.NewGuid(),
                    Name = "Coffee",
                    DisplayOrder = 1,
                    IsActive = true
                }
            },
            ResultCodes.ProductCategory.GetListSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<GetProductCategoriesQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.GetList(CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<List<ProductCategoryDto>>>();

        var body = (ApiResponse<List<ProductCategoryDto>>)ok.Value!;
        body.Code.Should().Be(ResultCodes.ProductCategory.GetListSuccess);
        body.Message.Should().Be(response.Message ?? string.Empty);
        body.Data.Should().NotBeNull();
        body.Data!.Should().HaveCount(1);
        body.Data[0].Name.Should().Be("Coffee");
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var request = new CreateProductCategoryRequest("Coffee", 1);

        var response = ServiceResult<ProductCategoryDto>.Ok(
            new ProductCategoryDto
            {
                PublicId = Guid.NewGuid(),
                Name = "Coffee",
                DisplayOrder = 1,
                IsActive = true
            },
            ResultCodes.ProductCategory.CreateSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CreateProductCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<ProductCategoryDto>>();

        var body = (ApiResponse<ProductCategoryDto>)ok.Value!;
        body.Code.Should().Be(ResultCodes.ProductCategory.CreateSuccess);
        body.Message.Should().Be(response.Message ?? string.Empty);
        body.Data.Should().NotBeNull();
        body.Data!.Name.Should().Be("Coffee");
        body.Data.DisplayOrder.Should().Be(1);
        body.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenMediatorReturnsFailure()
    {
        var request = new CreateProductCategoryRequest(string.Empty, 1);

        var response = ServiceResult<ProductCategoryDto>.Fail(
            ResultCodes.ProductCategory.CreateParameterError,
            "Category name is required.");

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<CreateProductCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(request, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().BeOfType<ApiResponse<ProductCategoryDto>>();

        var body = (ApiResponse<ProductCategoryDto>)badRequest.Value!;
        body.Code.Should().Be(ResultCodes.ProductCategory.CreateParameterError);
        body.Message.Should().Be("Category name is required.");
        body.Data.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var publicId = Guid.NewGuid();
        var request = new UpdateProductCategoryRequest("Coffee", 2, true);

        var response = ServiceResult<ProductCategoryDto>.Ok(
            new ProductCategoryDto
            {
                PublicId = publicId,
                Name = "Coffee",
                DisplayOrder = 2,
                IsActive = true
            },
            ResultCodes.ProductCategory.UpdateSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<UpdateProductCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Update(publicId, request, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<ProductCategoryDto>>();

        var body = (ApiResponse<ProductCategoryDto>)ok.Value!;
        body.Code.Should().Be(ResultCodes.ProductCategory.UpdateSuccess);
        body.Message.Should().Be(response.Message ?? string.Empty);
        body.Data.Should().NotBeNull();
        body.Data!.PublicId.Should().Be(publicId);
        body.Data.Name.Should().Be("Coffee");
        body.Data.DisplayOrder.Should().Be(2);
        body.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenMediatorReturnsSuccess()
    {
        var publicId = Guid.NewGuid();

        var response = ServiceResult<bool>.Ok(
            true,
            ResultCodes.ProductCategory.DeleteSuccess);

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<DeleteProductCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Delete(publicId, CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>();

        var ok = (OkObjectResult)result;
        ok.Value.Should().BeOfType<ApiResponse<bool>>();

        var body = (ApiResponse<bool>)ok.Value!;
        body.Code.Should().Be(ResultCodes.ProductCategory.DeleteSuccess);
        body.Message.Should().Be(response.Message ?? string.Empty);
        body.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenMediatorReturnsFailure()
    {
        var publicId = Guid.NewGuid();

        var response = ServiceResult<bool>.Fail(
            ResultCodes.ProductCategory.NotFound,
            "Category not found.");

        _mediatorMock.Setup(x => x.Send(
                It.IsAny<DeleteProductCategoryCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Delete(publicId, CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;
        badRequest.Value.Should().BeOfType<ApiResponse<bool>>();

        var body = (ApiResponse<bool>)badRequest.Value!;
        body.Code.Should().Be(ResultCodes.ProductCategory.NotFound);
        body.Message.Should().Be("Category not found.");
        body.Data.Should().BeFalse();
    }
}