using Dfc.Core.Models;

namespace Dfc.Core.Services;

public interface IValidationFeedbackService
{
    /// <summary>
    /// Validate ingredient with detailed feedback
    /// </summary>
    ValidationFeedback ValidateIngredient(Ingredient ingredient);

    /// <summary>
    /// Validate recipe with detailed feedback
    /// </summary>
    ValidationFeedback ValidateRecipe(Recipe recipe);

    /// <summary>
    /// Validate entree with detailed feedback
    /// </summary>
    ValidationFeedback ValidateEntree(Entree entree);

    /// <summary>
    /// Create user-friendly error message from exception
    /// </summary>
    string FormatException(System.Exception exception);
}
