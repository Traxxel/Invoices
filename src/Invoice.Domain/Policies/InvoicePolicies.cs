using Invoice.Domain.BusinessRules;
using Invoice.Domain.Entities;

namespace Invoice.Domain.Policies;

public class InvoicePolicies
{
    public static bool CanImportInvoice(Entities.Invoice invoice, IEnumerable<Entities.Invoice> existingInvoices)
    {
        // Check for duplicates
        if (InvoiceBusinessRules.IsDuplicateInvoice(invoice, existingInvoices))
            return false;

        // Check if invoice is ready for processing
        if (!InvoiceBusinessRules.IsReadyForProcessing(invoice))
            return false;

        return true;
    }

    public static bool CanUpdateInvoice(Entities.Invoice invoice, Entities.Invoice updatedInvoice)
    {
        // Cannot update to invalid state
        if (!InvoiceBusinessRules.IsReadyForProcessing(updatedInvoice))
            return false;

        // Cannot change invoice number to existing one
        if (updatedInvoice.InvoiceNumber != invoice.InvoiceNumber)
        {
            // This would need to be checked against existing invoices
            // Implementation depends on repository access
        }

        return true;
    }

    public static bool CanDeleteInvoice(Entities.Invoice invoice)
    {
        // Cannot delete if it's the only invoice from this issuer
        // Cannot delete if it's referenced by other entities
        // Implementation depends on business requirements

        return true; // Simplified for now
    }

    public static bool RequiresManualReview(Entities.Invoice invoice)
    {
        return InvoiceBusinessRules.IsLowConfidenceExtraction(invoice) ||
               InvoiceBusinessRules.IsSuspiciousAmount(invoice) ||
               !InvoiceBusinessRules.IsValidVatCalculation(invoice) ||
               !InvoiceBusinessRules.IsCompleteAddress(invoice);
    }

    public static bool CanAutoProcess(Entities.Invoice invoice)
    {
        return InvoiceBusinessRules.IsHighConfidenceExtraction(invoice) &&
               InvoiceBusinessRules.IsValidVatCalculation(invoice) &&
               InvoiceBusinessRules.IsCompleteAddress(invoice) &&
               InvoiceBusinessRules.IsCompleteFinancials(invoice) &&
               !InvoiceBusinessRules.IsSuspiciousAmount(invoice);
    }
}

