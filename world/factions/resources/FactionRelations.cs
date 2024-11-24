namespace SneakGame;

using Godot;
using Godot.Collections;

[GlobalClass]
public partial class FactionRelations : Resource
{

    [Export] public Array<FactionRelationLink> Links { get; set; } = new Array<FactionRelationLink>();

    public float GetRelationshipBetweenFactions(Faction factionA, Faction factionB)
    {
        foreach (var link in Links)
        {
            if (link.Faction1 == factionA && link.Faction2 == factionB)
            {
                return link.Affinity;
            }
        }

        return 0;
    }

}
