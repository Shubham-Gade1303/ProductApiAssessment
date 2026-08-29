using Application.DTOs.Products;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using FluentValidation;
using Moq;
using Xunit;

namespace Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IValidator<CreateProductRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateProductRequest>> _updateValidatorMock;

    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _createValidatorMock =
            new Mock<IValidator<CreateProductRequest>>();
        _updateValidatorMock =
            new Mock<IValidator<UpdateProductRequest>>();

        _service = new ProductService(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ProductExists_ReturnsProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Dell Laptop",
            CreatedBy = "admin",
            CreatedOn = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Dell Laptop", result.ProductName);
        Assert.Equal("admin", result.CreatedBy);
    }

    [Fact]
    public async Task GetByIdAsync_ProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                ProductName = "Laptop",
                CreatedBy = "admin",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                ProductName = "Keyboard",
                CreatedBy = "admin",
                CreatedOn = DateTime.UtcNow
            }
        };

        _productRepositoryMock
            .Setup(x => x.GetPagedAsync(
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 2));

        // Act
        var result = await _service.GetPagedAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            ProductName = "Dell Laptop"
        };

        _createValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        Product? capturedProduct = null;

        _productRepositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>(
                (product, _) => capturedProduct = product)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(
            request,
            "admin");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(capturedProduct);
        Assert.Equal("Dell Laptop", capturedProduct!.ProductName);
        Assert.Equal("admin", capturedProduct.CreatedBy);

        _productRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            ProductName = ""
        };

        var validationResult =
            new FluentValidation.Results.ValidationResult(
                new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        "ProductName",
                        "Product name is required.")
                });

        _createValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(
                request,
                "admin"));
    }

    [Fact]
    public async Task UpdateAsync_ProductExists_UpdatesProduct()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            ProductName = "Updated Laptop"
        };

        var product = new Product
        {
            Id = 1,
            ProductName = "Old Laptop",
            CreatedBy = "admin",
            CreatedOn = DateTime.UtcNow
        };

        _updateValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.UpdateAsync(
            1,
            request,
            "admin");

        // Assert
        Assert.Equal("Updated Laptop", product.ProductName);
        Assert.Equal("admin", product.ModifiedBy);
        Assert.NotNull(product.ModifiedOn);

        _productRepositoryMock.Verify(
            x => x.Update(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateProductRequest
        {
            ProductName = "Updated Laptop"
        };

        _updateValidatorMock
            .Setup(x => x.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(
                999,
                request,
                "admin"));
    }

    [Fact]
    public async Task DeleteAsync_ProductExists_DeletesProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Dell Laptop",
            CreatedBy = "admin",
            CreatedOn = DateTime.UtcNow
        };

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _productRepositoryMock.Verify(
            x => x.Delete(product),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(
                999,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.DeleteAsync(999));
    }
}