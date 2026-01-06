using System.Collections.Generic;

namespace Freecost.Core.Services;

public interface IValidationService
{
    ValidationResult ValidateRecipe(Models.Recipe recipe);
    ValidationResult ValidateIngredient(Models.Ingredient ingredient);
    ValidationResult ValidateEntree(Models.Entree entree);
}

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public List<ValidationInfo> Infos { get; set; } = new();

    public void AddError(string field, string message)
    {
        Errors.Add(new ValidationError { Field = field, Message = message });
    }

    public void AddWarning(string field, string message)
    {
        Warnings.Add(new ValidationWarning { Field = field, Message = message });
    }

    public void AddInfo(string field, string message)
    {
        Infos.Add(new ValidationInfo { Field = field, Message = message });
    }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ValidationWarning
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ValidationInfo
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
