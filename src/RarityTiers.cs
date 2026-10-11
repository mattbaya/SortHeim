using System;
using System.Collections.Generic;
using UnityEngine;

namespace SortHeim
{
    /// <summary>
    /// "Rarity" = the biome an item belongs to (0 Meadows .. 7 Deep North).
    /// Raw materials come from a hand-made table; anything crafted, smelted, cooked or
    /// fermented inherits the highest tier among its ingredients.
    /// </summary>
    internal static class RarityTiers
    {
        public const int Unknown = -1;

        // Prefab names, verified against the game's asset bundles.
        private static readonly string[] BaseTable =
        {
            /* 0 Meadows */      "Wood Stone Flint Resin LeatherScraps DeerHide DeerMeat RawMeat NeckTail Feathers Raspberry Mushroom Dandelion Honey QueenBee TrophyBoar TrophyDeer TrophyNeck TrophyEikthyr HardAntler FishRaw BeechSeeds Coins",
            /* 1 Black Forest */ "CopperOre TinOre RoundLog FineWood Blueberries Carrot CarrotSeeds MushroomYellow GreydwarfEye SurtlingCore AncientSeed TrophyGreydwarf TrophyGreydwarfBrute TrophyGreydwarfShaman TrophySkeleton TrophySkeletonHildir TrophyTheElder TrophyForestTroll TrollHide BoneFragments Thistle FirCone PineCone CryptKey Amber AmberPearl Ruby TrophyBjorn BjornHide BjornPaw BjornMeat",
            /* 2 Swamp */        "IronScrap IronOre Ooze Bloodbag Guck Entrails Chain WitheredBone ElderBark Turnip TurnipSeeds TrophyDraugr TrophyDraugrElite TrophyDraugrFem TrophyBlob TrophyLeech TrophySkeletonPoison TrophyWraith TrophyBonemass TrophyAbomination TrophySurtling Wishbone SerpentMeat SerpentScale TrophySerpent Root SilverNecklace",
            /* 3 Mountain */     "SilverOre WolfPelt WolfFang WolfMeat FreezeGland Obsidian Crystal TrophyWolf TrophyFenring TrophyCultist TrophyCultist_Hildir TrophyHatchling TrophySGolem TrophyUlv TrophyDragonQueen DragonEgg DragonTear Onion OnionSeeds WolfHairBundle",
            /* 4 Plains */       "BlackMetalScrap Barley Flax Needle LoxMeat LoxPelt Tar Cloudberry TrophyDeathsquito TrophyGoblin TrophyGoblinBrute TrophyGoblinShaman TrophyGoblinBruteBrosBrute TrophyGoblinBruteBrosShaman TrophyBjornUndead TrophyLox TrophyGrowth TrophyGoblinKing YagluthDrop GoblinTotem ChickenMeat ChickenEgg",
            /* 5 Mistlands */    "YggdrasilWood BlackMarble BlackCore Sap Softtissue Carapace Mandible BugMeat GiantBloodSack HareMeat ScaleHide Bilebag RoyalJelly MushroomJotunPuffs MushroomMagecap TrophyHare TrophySeeker TrophySeekerBrute TrophyTick TrophyGjall TrophyDvergr TrophySeekerQueen QueenDrop DvergrNeedle DvergrKeyFragment Wisp",
            /* 6 Ashlands */     "Grausten Blackwood FlametalOreNew CharredBone CelestialFeather MorgenSinew MorgenHeart AskHide AskBladder AsksvinMeat VoltureMeat VoltureEgg BoneMawSerpentMeat BonemawSerpentTooth ProustitePowder SulfurStone MoltenCore Fiddleheadfern MushroomSmokePuff Vineberry VineberrySeeds TrophyAsksvin TrophyCharredArcher TrophyCharredMelee TrophyCharredMage TrophyFallenValkyrie TrophyMorgen TrophyVolture TrophyBonemawSerpent Trophy_BoneMawSerpent TrophyFader FaderDrop",
        };

        // Crafting station floor, for recipes whose ingredients are all unknown.
        private static readonly Dictionary<string, int> StationTiers = new Dictionary<string, int>
        {
            { "$piece_workbench", 0 },
            { "$piece_forge", 1 },
            { "$piece_cauldron", 1 },
            { "$piece_stonecutter", 1 },
            { "$piece_artisanstation", 4 },
            { "$piece_blackforge", 5 },
            { "$piece_magetable", 5 },
        };

        private class Source
        {
            public readonly List<string> Ingredients = new List<string>();
            public int StationTier;
        }

        private static Dictionary<string, int> _base;
        private static Dictionary<string, List<Source>> _sources;
        private static readonly Dictionary<string, int> Cache = new Dictionary<string, int>();
        private static ObjectDB _builtFor;

        public static void Invalidate()
        {
            _builtFor = null;
            Cache.Clear();
        }

        public static int Get(ItemDrop.ItemData item)
        {
            var prefab = item.m_dropPrefab;
            if (prefab == null) return Unknown;
            EnsureBuilt();
            return Resolve(prefab.name, new HashSet<string>());
        }

        private static void EnsureBuilt()
        {
            var db = ObjectDB.instance;
            if (db != null && db == _builtFor) return;

            Cache.Clear();
            _base = new Dictionary<string, int>();
            for (int tier = 0; tier < BaseTable.Length; tier++)
                foreach (var name in BaseTable[tier].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    _base[name] = tier;
            ApplyOverrides(_base);

            _sources = new Dictionary<string, List<Source>>();
            if (db != null) AddRecipes(db);
            if (ZNetScene.instance != null) AddConversions(ZNetScene.instance);

            // Only treat the table as complete once the game's databases exist.
            _builtFor = db != null && ZNetScene.instance != null ? db : null;
        }

        private static void ApplyOverrides(Dictionary<string, int> table)
        {
            foreach (var pair in SortHeimPlugin.TierOverrides.Value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = pair.Split('=');
                if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int tier))
                    table[parts[0].Trim()] = tier;
                else
                    SortHeimPlugin.Log.LogWarning($"Ignoring malformed TierOverrides entry '{pair.Trim()}'");
            }
        }

        private static void AddRecipes(ObjectDB db)
        {
            foreach (var recipe in db.m_recipes)
            {
                if (recipe == null || recipe.m_item == null || recipe.m_resources == null) continue;

                var source = new Source();
                if (recipe.m_craftingStation != null)
                    StationTiers.TryGetValue(recipe.m_craftingStation.m_name, out source.StationTier);
                foreach (var req in recipe.m_resources)
                    if (req != null && req.m_resItem != null)
                        source.Ingredients.Add(req.m_resItem.name);

                AddSource(recipe.m_item.name, source);
            }
        }

        private static void AddConversions(ZNetScene scene)
        {
            foreach (var prefab in scene.m_prefabs)
            {
                if (prefab == null) continue;

                var smelter = prefab.GetComponent<Smelter>();
                if (smelter != null)
                    foreach (var c in smelter.m_conversion) AddConversion(c?.m_from, c?.m_to);

                var cooking = prefab.GetComponent<CookingStation>();
                if (cooking != null)
                    foreach (var c in cooking.m_conversion) AddConversion(c?.m_from, c?.m_to);

                var fermenter = prefab.GetComponent<Fermenter>();
                if (fermenter != null)
                    foreach (var c in fermenter.m_conversion) AddConversion(c?.m_from, c?.m_to);
            }
        }

        private static void AddConversion(ItemDrop from, ItemDrop to)
        {
            if (from == null || to == null) return;
            var source = new Source();
            source.Ingredients.Add(from.name);
            AddSource(to.name, source);
        }

        private static void AddSource(string product, Source source)
        {
            if (!_sources.TryGetValue(product, out var list))
                _sources[product] = list = new List<Source>();
            list.Add(source);
        }

        private static int Resolve(string name, HashSet<string> visiting)
        {
            if (_base.TryGetValue(name, out int tier)) return tier;
            if (Cache.TryGetValue(name, out tier)) return tier;
            if (!_sources.TryGetValue(name, out var sources)) return Unknown;
            if (!visiting.Add(name)) return Unknown; // recipe cycle

            // An item's tier is the cheapest way to make it: min over recipes of (max over ingredients).
            int best = int.MaxValue;
            foreach (var source in sources)
            {
                int t = source.StationTier;
                foreach (var ingredient in source.Ingredients)
                    t = Math.Max(t, Resolve(ingredient, visiting));
                best = Math.Min(best, t);
            }

            visiting.Remove(name);
            tier = best == int.MaxValue ? Unknown : best;
            Cache[name] = tier;
            return tier;
        }
    }
}
