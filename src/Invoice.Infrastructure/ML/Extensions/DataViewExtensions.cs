using Microsoft.ML;
using Microsoft.ML.Data;
using Invoice.Infrastructure.ML.Models;

namespace Invoice.Infrastructure.ML.Extensions;

public static class DataViewExtensions
{
    public static long? GetRowCount(this IDataView dataView)
    {
        return dataView.GetRowCount();
    }

    public static bool HasColumn(this IDataView dataView, string columnName)
    {
        return dataView.Schema.Any(col => col.Name == columnName);
    }

    public static DataViewType? GetColumnType(this IDataView dataView, string columnName)
    {
        var column = dataView.Schema.FirstOrDefault(col => col.Name == columnName);
        return column.Type;
    }

    public static IEnumerable<string> GetColumnNames(this IDataView dataView)
    {
        return dataView.Schema.Select(col => col.Name);
    }

    public static DataViewSchema GetDataViewSchema(this IDataView dataView)
    {
        return dataView.Schema;
    }

    public static bool IsEmpty(this IDataView dataView)
    {
        var rowCount = dataView.GetRowCount();
        return !rowCount.HasValue || rowCount.Value == 0;
    }

    public static int GetColumnCount(this IDataView dataView)
    {
        return dataView.Schema.Count;
    }

    public static bool HasRequiredColumns(this IDataView dataView, params string[] requiredColumns)
    {
        return requiredColumns.All(col => dataView.HasColumn(col));
    }
}

