using Invoice.Application.DTOs;
using Invoice.Domain.Entities;

namespace Invoice.Application.Extensions;

public static class DtoExtensions
{
    public static InvoiceDto ToDto(this Domain.Entities.Invoice invoice)
    {
        return new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.InvoiceDate,
            invoice.IssuerName,
            invoice.IssuerStreet,
            invoice.IssuerPostalCode,
            invoice.IssuerCity,
            invoice.IssuerCountry,
            invoice.NetTotal,
            invoice.VatTotal,
            invoice.GrossTotal,
            invoice.SourceFilePath,
            invoice.ImportedAt,
            invoice.ExtractionConfidence,
            invoice.ModelVersion
        );
    }

    public static Domain.Entities.Invoice ToEntity(this InvoiceCreateDto dto)
    {
        return Domain.Entities.Invoice.Create(
            dto.InvoiceNumber,
            dto.InvoiceDate,
            dto.IssuerName,
            dto.NetTotal,
            dto.VatTotal,
            dto.GrossTotal,
            dto.SourceFilePath,
            dto.ExtractionConfidence,
            dto.ModelVersion
        );
    }

    public static List<InvoiceDto> ToDtoList(this List<Domain.Entities.Invoice> invoices)
    {
        return invoices.Select(i => i.ToDto()).ToList();
    }

    public static ApiResponse<T> ToApiResponse<T>(this T data, string message = "Success")
    {
        return new ApiResponse<T>(
            true,
            data,
            message,
            new List<string>(),
            DateTime.UtcNow
        );
    }

    public static ApiResponse<T> ToErrorResponse<T>(this string error, List<string>? errors = null)
    {
        return new ApiResponse<T>(
            false,
            default(T),
            error,
            errors ?? new List<string> { error },
            DateTime.UtcNow
        );
    }

    public static PagedResponse<T> ToPagedResponse<T>(this List<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PagedResponse<T>(
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            page < totalPages,
            page > 1
        );
    }
}

