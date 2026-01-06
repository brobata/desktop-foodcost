using Freecost.Core.Enums;
using System;
using System.Collections.Generic;

namespace Freecost.Core.Helpers;

public static class AllergenMapper
{
    private static readonly Dictionary<AllergenType, Guid> _allergenTypeToId = new()
    {
        // FDA Big 9
        { AllergenType.Milk, Guid.Parse("11111111-1111-1111-1111-111111111111") },
        { AllergenType.Eggs, Guid.Parse("22222222-2222-2222-2222-222222222222") },
        { AllergenType.Fish, Guid.Parse("33333333-3333-3333-3333-333333333333") },
        { AllergenType.Shellfish, Guid.Parse("44444444-4444-4444-4444-444444444444") },
        { AllergenType.TreeNuts, Guid.Parse("55555555-5555-5555-5555-555555555555") },
        { AllergenType.Peanuts, Guid.Parse("66666666-6666-6666-6666-666666666666") },
        { AllergenType.Wheat, Guid.Parse("77777777-7777-7777-7777-777777777777") },
        { AllergenType.Soybeans, Guid.Parse("88888888-8888-8888-8888-888888888888") },
        { AllergenType.Sesame, Guid.Parse("99999999-9999-9999-9999-999999999999") },

        // Dietary
        { AllergenType.Vegan, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") },
        { AllergenType.Vegetarian, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb") },
        { AllergenType.Pescatarian, Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc") },
        { AllergenType.GlutenFree, Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd") },

        // Religious
        { AllergenType.Kosher, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee") },
        { AllergenType.Halal, Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff") },

        // Additional
        { AllergenType.ContainsAlcohol, Guid.Parse("12121212-1212-1212-1212-121212121212") },
        { AllergenType.Nightshades, Guid.Parse("13131313-1313-1313-1313-131313131313") },
        { AllergenType.Sulfites, Guid.Parse("14141414-1414-1414-1414-141414141414") },
        { AllergenType.AddedSugar, Guid.Parse("15151515-1515-1515-1515-151515151515") },
    };

    public static Guid GetAllergenId(AllergenType type)
    {
        return _allergenTypeToId[type];
    }
}
