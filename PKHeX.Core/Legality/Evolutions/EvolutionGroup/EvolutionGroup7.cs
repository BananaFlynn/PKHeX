using System;

namespace PKHeX.Core;

public sealed class EvolutionGroup7 : IEvolutionGroup
{
    public static readonly EvolutionGroup7 Instance = new();
    private static readonly EvolutionTree Tree = EvolutionTree.Evolves7;
    private const int Generation = 7;
    private static PersonalTable7 Personal => PersonalTable.USUM;

    public IEvolutionGroup? GetNext(PKM pk, EvolutionOrigin enc) => pk.Format > Generation ? EvolutionGroup8.Instance : null;
    public IEvolutionGroup? GetPrevious(PKM pk, EvolutionOrigin enc)
    {
        if (enc.Generation <= 2)
            return EvolutionGroup2.Instance;
        if (enc.Generation < Generation)
            return EvolutionGroup6.Instance;
        return null;
    }

    public bool Append(PKM pk, EvolutionHistory history, ref ReadOnlySpan<EvoCriteria> chain, EvolutionOrigin enc)
    {
        // Get the first evolution in the chain that can be present in this group
        var any = GetFirstEvolution(chain, out var evo);
        if (!any)
            return false;

        // Get the evolution tree from this group and get the new chain from it.
        var criteria = enc with { LevelMax = evo.LevelMax, LevelMin = (byte)pk.Met_Level };
        var local = GetInitialChain(pk, criteria, evo.Species, evo.Form);

        // Revise the tree
        var revised = Prune(local);

        // Set the tree to the history field
        history.Gen7 = revised;

        // Retain a reference to the current chain for future appending as we step backwards.
        chain = revised;

        return revised.Length != 0;
    }

    public EvoCriteria[] GetInitialChain(PKM pk, EvolutionOrigin enc, ushort species, byte form)
    {
        Span<EvoCriteria> result = stackalloc EvoCriteria[EvolutionTree.MaxEvolutions];
        var count = Tree.GetExplicitLineage(result, species, form, pk, enc.LevelMin, enc.LevelMax, enc.Species, enc.SkipChecks);
        return result[..count].ToArray();
    }

    private static EvoCriteria[] Prune(EvoCriteria[] chain) => chain;

    private static bool GetFirstEvolution(ReadOnlySpan<EvoCriteria> chain, out EvoCriteria result)
    {
        var pt = Personal;
        foreach (var evo in chain)
        {
            // If the evo can't exist in the game, it must be a future evolution.
            if (!pt.IsPresentInGame(evo.Species, evo.Form))
                continue;

            result = evo;
            return true;
        }

        result = default;
        return false;
    }
}
