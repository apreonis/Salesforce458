using InventoryManagement.Data;
using InventoryManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text;

namespace InventoryManagement.Services;

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _context;

    public ExportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportInventoryToCsvAsync(Guid inventoryId)
    {
        var items = await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .Include(i => i.CreatedBy)
            .ToListAsync();

        var fields = await _context.CustomFieldDefinitions
            .Where(f => f.InventoryId == inventoryId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.Append("CustomId,CreatedBy,CreatedAt");
        foreach (var field in fields)
        {
            sb.Append($",{EscapeCsv(field.Name)}");
        }
        sb.AppendLine();

        foreach (var item in items)
        {
            sb.Append($"{EscapeCsv(item.CustomId)},{EscapeCsv(item.CreatedBy?.DisplayName)},{item.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            foreach (var field in fields)
            {
                var value = GetFieldValue(item, field);
                sb.Append($",{EscapeCsv(value?.ToString() ?? "")}");
            }
            sb.AppendLine();
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportInventoryToExcelAsync(Guid inventoryId)
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Items");

        var items = await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .Include(i => i.CreatedBy)
            .ToListAsync();

        var fields = await _context.CustomFieldDefinitions
            .Where(f => f.InventoryId == inventoryId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        int col = 1;
        worksheet.Cells[1, col++].Value = "Custom ID";
        worksheet.Cells[1, col++].Value = "Created By";
        worksheet.Cells[1, col++].Value = "Created At";
        foreach (var field in fields)
        {
            worksheet.Cells[1, col++].Value = field.Name;
        }

        int row = 2;
        foreach (var item in items)
        {
            col = 1;
            worksheet.Cells[row, col++].Value = item.CustomId;
            worksheet.Cells[row, col++].Value = item.CreatedBy?.DisplayName;
            worksheet.Cells[row, col++].Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            foreach (var field in fields)
            {
                worksheet.Cells[row, col++].Value = GetFieldValue(item, field);
            }
            row++;
        }

        return package.GetAsByteArray();
    }

    private object GetFieldValue(Item item, CustomFieldDefinition field)
    {
        return field.FieldType switch
        {
            CustomFieldType.SingleLineText => field.FieldIndex switch
            {
                1 => item.Text1,
                2 => item.Text2,
                3 => item.Text3,
                _ => null
            },
            CustomFieldType.MultiLineText => field.FieldIndex switch
            {
                1 => item.MultiText1,
                2 => item.MultiText2,
                3 => item.MultiText3,
                _ => null
            },
            CustomFieldType.Number => field.FieldIndex switch
            {
                1 => item.Number1,
                2 => item.Number2,
                3 => item.Number3,
                _ => null
            },
            CustomFieldType.Boolean => field.FieldIndex switch
            {
                1 => item.Boolean1,
                2 => item.Boolean2,
                3 => item.Boolean3,
                _ => null
            },
            CustomFieldType.Document => field.FieldIndex switch
            {
                1 => item.Document1,
                2 => item.Document2,
                3 => item.Document3,
                _ => null
            },
            _ => null
        };
    }

    private string EscapeCsv(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            return $"\"{input.Replace("\"", "\"\"")}\"";
        return input;
    }
}