using Application.DTOs;
using Application.DTOs.Products;
using Application.Interfaces;
using Domain.Entities;
using FluentValidation;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            return null;
        }

        return MapToResponse(product);
    }

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (products, totalCount) =
            await _productRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)pageSize);

        return new PagedResult<ProductResponse>
        {
            Items = products
                .Select(MapToResponse)
                .ToList(),

            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var validationResult =
            await _createValidator.ValidateAsync(
                request,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var product = new Product
        {
            ProductName = request.ProductName.Trim(),
            CreatedBy = createdBy,
            CreatedOn = DateTime.UtcNow
        };

        await _productRepository.AddAsync(
            product,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(product);
    }

    public async Task UpdateAsync(
        int id,
        UpdateProductRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var validationResult =
            await _updateValidator.ValidateAsync(
                request,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product with ID {id} was not found.");
        }

        product.ProductName = request.ProductName.Trim();
        product.ModifiedBy = modifiedBy;
        product.ModifiedOn = DateTime.UtcNow;

        _productRepository.Update(product);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product with ID {id} was not found.");
        }

        _productRepository.Delete(product);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            ProductName = product.ProductName,
            CreatedBy = product.CreatedBy,
            CreatedOn = product.CreatedOn,
            ModifiedBy = product.ModifiedBy,
            ModifiedOn = product.ModifiedOn
        };
    }
}