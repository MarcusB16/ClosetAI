using System.ComponentModel.DataAnnotations;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Application.Dtos.Items.CreateItem;

[AttributeUsage(AttributeTargets.Property)]
public class GarmentTypeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        GarmentTypes.IsKnown(value as string)
            ? ValidationResult.Success
            : new ValidationResult($"Type must be one of: {string.Join(", ", GarmentTypes.All)}.");
}
