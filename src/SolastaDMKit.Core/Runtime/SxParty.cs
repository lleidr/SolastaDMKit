using System.Collections.Generic;
using System.Linq;
using TA;

namespace SolastaDMKit.Core.Runtime;

public static class SxParty
{
    public static IEnumerable<GameLocationCharacter> Members()
    {
        var service = ServiceRepository.GetService<IGameLocationCharacterService>();
        return service?.PartyCharacters ?? Enumerable.Empty<GameLocationCharacter>();
    }

    public static IEnumerable<GameLocationCharacter> Guests()
    {
        var service = ServiceRepository.GetService<IGameLocationCharacterService>();
        return service?.GuestCharacters ?? Enumerable.Empty<GameLocationCharacter>();
    }

    public static IEnumerable<GameLocationCharacter> MembersAndGuests()
    {
        return Members().Concat(Guests());
    }

    public static int Count => Members().Count();

    public static int TeleportAllTo(
        int3 position,
        LocationDefinitions.Orientation orientation = LocationDefinitions.Orientation.North)
    {
        var service = ServiceRepository.GetService<IGameLocationPositioningService>();
        if (service == null)
        {
            return 0;
        }

        var count = 0;
        foreach (var member in Members())
        {
            service.TeleportCharacter(member, position, orientation);
            count++;
        }

        return count;
    }
}
