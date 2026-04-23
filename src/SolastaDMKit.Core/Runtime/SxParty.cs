using System.Collections.Generic;
using System.Linq;

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
}
