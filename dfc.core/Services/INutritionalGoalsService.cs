using Dfc.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dfc.Core.Services;

public interface INutritionalGoalsService
{
    Task<NutritionalGoals> GetGoalsAsync();
    Task SaveGoalsAsync(NutritionalGoals goals);
    List<NutritionalGoalComparison> CompareRecipeToGoals(Recipe recipe, NutritionalGoals goals);
}
