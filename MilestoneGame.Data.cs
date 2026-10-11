using Microsoft.Xna.Framework;

namespace KingsLastCastle;

// Enthält alle festen Spieldaten: Level, Wege, Tower-Spots, Gegnerreihenfolgen
// und Schwierigkeitswerte. Zusätzlich werden diese Daten beim Start validiert.
public sealed partial class MilestoneGame
{
    // Erstellt alle drei Level vollständig und gibt sie als gemeinsame Level-Liste zurück.
    private static List<LevelDefinition> CreateLevels()
    {
        // Die Bäume werden ringförmig am Inselrand verteilt und in beiden Leveln verwendet.
        var trees = new[]
        {
            new Vector3(-11.4f,0f,5.5f), new Vector3(-9.6f,0f,8.1f), new Vector3(-6.4f,0f,10.1f),
            new Vector3(-2.4f,0f,11.0f), new Vector3(2.1f,0f,10.9f), new Vector3(6.2f,0f,9.7f),
            new Vector3(9.7f,0f,7.2f), new Vector3(11.4f,0f,3.9f), new Vector3(11.6f,0f,-3.4f),
            new Vector3(9.2f,0f,-7.1f), new Vector3(5.8f,0f,-9.7f), new Vector3(1.6f,0f,-10.9f),
            new Vector3(-2.9f,0f,-10.8f), new Vector3(-7.0f,0f,-9.3f), new Vector3(-10.2f,0f,-6.3f),
            new Vector3(-11.8f,0f,-2.7f)
        };

        // Level 1 besitzt einen kurzen direkten Weg und einen deutlich längeren Umweg.
        var shortPath = new List<Vector3>
        {
            new(-12.20f,0f,0.00f), // spawn
            new(-9.30f,0f,0.00f),  // shared route split
            new(-5.80f,0f,0.00f),
            new(-2.20f,0f,0.00f),
            new(1.40f,0f,0.00f),
            new(5.10f,0f,0.00f),
            new(8.60f,0f,0.00f),
            new(9.85f,0f,0.00f)  // castle
        };

        // Der Long Path beginnt und endet gleich, führt dazwischen aber um die Inselmitte.
        var longPath = new List<Vector3>
        {
            new(-12.20f,0f,0.00f), // spawn
            new(-9.30f,0f,0.00f),  // shared route split
            new(-9.30f,0f,5.35f),
            new(-2.70f,0f,5.35f),
            new(2.05f,0f,5.35f),
            new(2.05f,0f,-5.15f),
            new(7.85f,0f,-5.15f),
            new(7.85f,0f,0.00f),
            new(9.85f,0f,0.00f)  // castle
        };

        // Fluggegner verwenden eine eigene Spur vom gleichen Spawn in Richtung Schloss.
        var flyingPath = new List<Vector3>
        {
            new(-12.20f,0f,0.00f),
            new(-8.00f,0f,1.65f),
            new(-3.10f,0f,1.10f),
            new(2.20f,0f,1.65f),
            new(7.60f,0f,0.80f),
            new(9.85f,0f,0.00f)
        };

        // Feste Bauplätze für Level 1; nur an diesen Positionen können Tower entstehen.
        var originalTowerSpots = new List<Vector3>
        {
            new(-11.05f,0f,2.35f), new(-7.35f,0f,-2.10f), new(-5.30f,0f,2.25f),
            new(-2.25f,0f,-2.15f), new(0.75f,0f,2.15f), new(3.55f,0f,-2.05f),
            new(5.50f,0f,2.15f), new(9.60f,0f,-2.10f), new(9.75f,0f,2.00f),
            new(-6.80f,0f,6.75f), new(-1.55f,0f,6.80f), new(3.55f,0f,6.75f),
            new(4.90f,0f,-6.70f)
        };

        // Level 2 nutzt dieselbe Pfadkontrolle, aber mit einer gebogenen Short-Path-Variante.
        var crystalShortPath = new List<Vector3>
        {
            new(-12.20f,0f,-1.80f),
            new(-9.10f,0f,-1.80f),
            new(-5.70f,0f,-1.10f),
            new(-1.80f,0f,-0.15f),
            new(2.00f,0f,0.80f),
            new(5.85f,0f,1.70f),
            new(8.95f,0f,2.55f),
            new(9.85f,0f,3.05f)
        };

        // Der lange Weg von Crystal Ridge bildet einen größeren Zickzack-Umweg.
        var crystalLongPath = new List<Vector3>
        {
            new(-12.20f,0f,-1.80f),
            new(-9.10f,0f,-1.80f),
            new(-9.10f,0f,5.55f),
            new(-4.80f,0f,5.95f),
            new(-0.70f,0f,4.15f),
            new(2.15f,0f,6.10f),
            new(5.95f,0f,4.75f),
            new(5.95f,0f,-4.75f),
            new(9.15f,0f,-4.75f),
            new(9.15f,0f,2.55f),
            new(9.85f,0f,3.05f)
        };

        // Eigene Flugroute für die Lage des Schlosses in Level 2.
        var crystalFlyingPath = new List<Vector3>
        {
            new(-12.20f,0f,-1.80f),
            new(-8.20f,0f,0.55f),
            new(-2.25f,0f,1.60f),
            new(3.80f,0f,2.45f),
            new(8.70f,0f,2.90f),
            new(9.85f,0f,3.05f)
        };

        // Ausbalancierte Bauplätze entlang der möglichen Routen von Crystal Ridge.
        var crystalTowerSpots = new List<Vector3>
        {
            new(-10.55f,0f,0.95f), new(-7.40f,0f,-3.95f), new(-6.50f,0f,2.95f),
            new(-3.35f,0f,-2.30f), new(-2.80f,0f,6.85f), new(0.25f,0f,2.35f),
            new(2.70f,0f,-2.45f), new(4.10f,0f,6.85f), new(4.70f,0f,2.75f),
            new(6.85f,0f,-6.45f), new(10.80f,0f,-2.10f), new(9.70f,0f,4.80f)
        };

        // Level 3 verbindet eine schnelle Zickzack-Route mit einem weiten äußeren Verteidigungsbogen.
        var bastionShortPath = new List<Vector3>
        {
            new(-12.20f,0f,2.20f), new(-9.20f,0f,2.20f), new(-5.80f,0f,1.20f),
            new(-2.20f,0f,2.40f), new(1.50f,0f,1.10f), new(5.00f,0f,2.20f),
            new(8.60f,0f,1.20f), new(9.85f,0f,1.20f)
        };

        var bastionLongPath = new List<Vector3>
        {
            new(-12.20f,0f,2.20f), new(-9.20f,0f,2.20f), new(-9.20f,0f,-5.70f),
            new(-4.50f,0f,-6.00f), new(-1.00f,0f,-4.20f), new(2.50f,0f,-6.10f),
            new(6.30f,0f,-4.80f), new(6.30f,0f,5.30f), new(9.20f,0f,5.30f),
            new(9.20f,0f,1.20f), new(9.85f,0f,1.20f)
        };

        var bastionFlyingPath = new List<Vector3>
        {
            new(-12.20f,0f,2.20f), new(-8.00f,0f,3.70f), new(-3.00f,0f,2.60f),
            new(2.20f,0f,3.40f), new(7.40f,0f,2.20f), new(9.85f,0f,1.20f)
        };

        var bastionTowerSpots = new List<Vector3>
        {
            new(-10.90f,0f,4.55f), new(-7.10f,0f,-0.70f), new(-6.20f,0f,3.95f),
            new(-3.55f,0f,-0.30f), new(-2.15f,0f,4.70f), new(0.70f,0f,-1.70f),
            new(2.65f,0f,4.15f), new(4.70f,0f,-0.25f), new(8.00f,0f,3.65f),
            new(8.80f,0f,-2.50f), new(-5.65f,0f,-7.55f), new(1.85f,0f,-7.55f),
            new(8.10f,0f,-6.55f)
        };

        // Hier werden Geometrie, Welle und Farben zu vollständigen Leveldefinitionen verbunden.
        return new List<LevelDefinition>
        {
            new()
            {
                Name = "Green Island",
                CastlePosition = new Vector3(12.05f,0f,0.00f),
                ShortPath = shortPath,
                LongPath = longPath,
                FlyingPath = flyingPath,
                TowerSpots = new List<Vector3>(originalTowerSpots),
                ShortTrapPosition = new Vector3(1.40f,0f,0.00f),
                Waves = new[]
                {
                    new[]{ EnemyType.Standard,EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,
                           EnemyType.Heavy,EnemyType.Standard,EnemyType.Flying,EnemyType.Standard },
                    new[]{ EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,
                           EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy,EnemyType.Standard,
                           EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy },
                    new[]{ EnemyType.Standard,EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,
                           EnemyType.Heavy,EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,
                           EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,EnemyType.Heavy,
                           EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy,EnemyType.Standard }
                },
                Trees = trees
            },
            new()
            {
                Name = "Crystal Ridge",
                CastlePosition = new Vector3(12.05f,0f,3.05f),
                ShortPath = crystalShortPath,
                LongPath = crystalLongPath,
                FlyingPath = crystalFlyingPath,
                TowerSpots = new List<Vector3>(crystalTowerSpots),
                ShortTrapPosition = new Vector3(2.00f,0f,0.80f),
                Waves = new[]
                {
                    new[]{ EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,
                           EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,EnemyType.Standard },
                    new[]{ EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,EnemyType.Standard,
                           EnemyType.Flying,EnemyType.Heavy,EnemyType.Standard,EnemyType.Heavy,
                           EnemyType.Flying,EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy },
                    new[]{ EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,
                           EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,
                           EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,EnemyType.Heavy,
                           EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy,EnemyType.Flying,
                           EnemyType.Standard,EnemyType.Heavy }
                },
                Trees = trees,
                SkyTop = new Color(82, 139, 190),
                SkyBottom = new Color(220, 237, 244),
                IslandSide = new Color(83, 155, 126),
                IslandTop = new Color(126, 201, 158),
                PathBase = new Color(78, 76, 86),
                PathTop = new Color(168, 154, 171),
                Accent = new Color(92, 176, 189)
            },
            new()
            {
                Name = "Sunset Bastion",
                CastlePosition = new Vector3(12.05f,0f,1.20f),
                ShortPath = bastionShortPath,
                LongPath = bastionLongPath,
                FlyingPath = bastionFlyingPath,
                TowerSpots = new List<Vector3>(bastionTowerSpots),
                ShortTrapPosition = new Vector3(1.50f,0f,1.10f),
                Waves = new[]
                {
                    new[]{ EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,
                           EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy,EnemyType.Standard,
                           EnemyType.Flying,EnemyType.Standard },
                    new[]{ EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,EnemyType.Standard,
                           EnemyType.Flying,EnemyType.Heavy,EnemyType.Standard,EnemyType.Heavy,
                           EnemyType.Flying,EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy,
                           EnemyType.Standard,EnemyType.Flying },
                    new[]{ EnemyType.Heavy,EnemyType.Standard,EnemyType.Flying,EnemyType.Standard,
                           EnemyType.Heavy,EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,
                           EnemyType.Flying,EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,
                           EnemyType.Standard,EnemyType.Heavy,EnemyType.Flying,EnemyType.Heavy,
                           EnemyType.Standard,EnemyType.Flying,EnemyType.Heavy,EnemyType.Standard }
                },
                Trees = trees,
                SkyTop = new Color(178, 102, 92),
                SkyBottom = new Color(244, 205, 159),
                IslandSide = new Color(107, 137, 91),
                IslandTop = new Color(151, 184, 104),
                PathBase = new Color(104, 69, 56),
                PathTop = new Color(205, 144, 92),
                Accent = new Color(228, 184, 82)
            }
        };
    }

    // Definiert die Startressourcen und Gegner-Multiplikatoren für Easy und Hard.
    private static DifficultySettings[] CreateDifficulties() => new[]
    {
        new DifficultySettings { Name = "Easy", StartGold = 195, CastleHp = 18, EnemySpeedMul = 1.00f, EnemyHealthMul = 1.00f, RewardMul = 1.00f },
        new DifficultySettings { Name = "Hard", StartGold = 135, CastleHp = 10, EnemySpeedMul = 1.22f, EnemyHealthMul = 1.35f, RewardMul = 0.85f }
    };

    // Prüft beim Programmstart wichtige Regeln, damit fehlerhafte Leveldaten sofort auffallen.
    private void ValidateGameDefinitions()
    {
        if (_levels.Count < 3) throw new InvalidOperationException("The final build requires at least three levels.");
        if (_difficulties.Length != 2) throw new InvalidOperationException("Easy and Hard difficulty must both exist.");

        foreach (var level in _levels)
        {
            if (string.IsNullOrWhiteSpace(level.Name)) throw new InvalidOperationException("Every level needs a name.");
            if (level.ShortPath.Count < 2 || level.LongPath.Count < 2 || level.FlyingPath.Count < 2)
                throw new InvalidOperationException($"{level.Name}: every route needs at least two waypoints.");
            if (Vector3.Distance(level.ShortPath[0], level.LongPath[0]) > 0.01f ||
                Vector3.Distance(level.ShortPath[^1], level.LongPath[^1]) > 0.01f)
                throw new InvalidOperationException($"{level.Name}: short and long route must share spawn and castle.");
            if (PathLength(level.LongPath) <= PathLength(level.ShortPath) + 0.25f)
                throw new InvalidOperationException($"{level.Name}: long route must be meaningfully longer than short route.");
            if (level.TowerSpots.Count < 8 || level.TowerSpots.Distinct().Count() != level.TowerSpots.Count)
                throw new InvalidOperationException($"{level.Name}: tower spots are missing or duplicated.");
            if (level.Waves.Length != 3 || level.Waves.Any(wave => wave.Length == 0))
                throw new InvalidOperationException($"{level.Name}: exactly three non-empty waves are required.");
            if (DistanceToPathXZ(level.ShortTrapPosition, level.ShortPath) > 0.35f)
                throw new InvalidOperationException($"{level.Name}: Royal Trap must be placed on the short path.");
            foreach (var spot in level.TowerSpots)
            {
                float clearance = Math.Min(DistanceToPathXZ(spot, level.ShortPath), DistanceToPathXZ(spot, level.LongPath));
                if (clearance < 1.05f)
                    throw new InvalidOperationException($"{level.Name}: tower spot {spot} is only {clearance:0.00} from a ground path.");
            }
        }

        foreach (var difficulty in _difficulties)
        {
            if (difficulty.StartGold <= 0 || difficulty.CastleHp <= 0 || difficulty.EnemySpeedMul <= 0f || difficulty.EnemyHealthMul <= 0f)
                throw new InvalidOperationException($"{difficulty.Name}: difficulty values must be positive.");
        }

        // Vier Testpositionen stellen sicher, dass die Kamera wirklich um das Ziel kreist.
        float expectedRadius = CameraBaseOffset.Length();
        var cameraSamples = new[] { 0f, MathHelper.PiOver2, MathHelper.Pi, -MathHelper.PiOver2 };
        var positions = cameraSamples.Select(CameraPositionForYaw).ToArray();
        if (positions.Any(position => Math.Abs(Vector3.Distance(position, CameraTarget) - expectedRadius) > 0.01f) ||
            Vector3.Distance(positions[0], positions[2]) < 20f || Vector3.Distance(positions[1], positions[3]) < 20f)
            throw new InvalidOperationException("The 360-degree camera orbit is invalid.");
    }

    // Kleinster XZ-Abstand eines Punkts zu allen Segmenten eines Pfads.
    private static float DistanceToPathXZ(Vector3 point, IReadOnlyList<Vector3> path)
    {
        float best = float.MaxValue;
        var p = new Vector2(point.X, point.Z);
        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = new Vector2(path[i].X, path[i].Z);
            var b = new Vector2(path[i + 1].X, path[i + 1].Z);
            var ab = b - a;
            float t = ab.LengthSquared() < 0.0001f ? 0f : Math.Clamp(Vector2.Dot(p - a, ab) / ab.LengthSquared(), 0f, 1f);
            best = Math.Min(best, Vector2.Distance(p, a + ab * t));
        }
        return best;
    }

}

