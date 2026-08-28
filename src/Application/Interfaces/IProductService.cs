using Application.DTOs;
using Application.DTOs.Products;

namespace Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ProductResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        int id,
        UpdateProductRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}