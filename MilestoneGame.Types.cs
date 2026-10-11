using Microsoft.Xna.Framework;

namespace KingsLastCastle;

// Enthält die kleinen internen Datentypen der Spielklasse.
// Diese Typen speichern nur Zustand und Werte; die eigentliche Logik liegt in MilestoneGame.cs.
public sealed partial class MilestoneGame
{
    // Verfügbare Gegnerarten mit unterschiedlichen Werten und Bewegungsregeln.
    private enum EnemyType { Standard, Heavy, Flying }

    // Verfügbare Tower-Arten mit unterschiedlichen Rollen im Kampf.
    private enum TowerType { Arrow, Cannon, Crystal }

    // Mögliche Hauptzustände vom Menü bis zum Ende einer Spielrunde.
    private enum GamePhase { MainMenu, Preparation, WaveActive, Paused, Victory, Defeat }

    // Auswahl der beiden Balancing-Stufen.
    private enum Difficulty { Easy, Hard }

    // Aktuell geöffnete Bodenroute für neu gespawnte Gegner.
    private enum PathMode { Short, Long }

    // Laufzeitdaten eines einzelnen Gegners.
    private sealed class Enemy
    {
        // Darstellung und aktuelle Position auf der Route.
        public EnemyType Type;
        public Vector3 Position;
        public float RotationY;
        public float DistanceOnPath;
        public float TimeAlive;
        public float Scale;
        // Zugewiesener Weg und zurückgelegter Fortschritt.
        public List<Vector3> Route = new();
        public PathMode? GroundRouteMode;
        public float RouteLength;
        // Kampf- und Belohnungswerte des Gegners.
        public float Speed;
        public int Health;
        public int MaxHealth;
        public int Reward;
        public int CastleDamage;
    }

    // Laufzeitdaten eines gebauten Towers.
    private sealed class Tower
    {
        // Tower-Typ, belegter Bauplatz und Position in der 3D-Welt.
        public TowerType Type;
        public int SpotIndex;
        public Vector3 Position;
        // Kampfwerte und verbleibender Schuss-Cooldown.
        public float Range;
        public float Cooldown;
        public float Timer;
        public int Damage;
    }

    // Kurzlebige visuelle Darstellung eines Projektils oder Laserstrahls.
    private sealed class Shot
    {
        // Flugstrecke und momentane Position des sichtbaren Schusses.
        public Vector3 Start;
        public Vector3 End;
        public Vector3 Position;
        // Zeitsteuerung, Farbe und Darstellungsart.
        public float Age;
        public float Life;
        public Color Color;
        public bool IsLaser;
        public TowerType Type;
        // Arrow und Cannon wenden ihren Schaden erst beim sichtbaren Einschlag an.
        public Enemy? Target;
        public int Damage;
        public float SplashRadius;
        public bool DamageApplied;
    }

    // Vollständige feste Beschreibung eines Levels.
    private sealed class LevelDefinition
    {
        // Allgemeine Identität und Zielposition des Levels.
        public string Name = "";
        public Vector3 CastlePosition;
        // Alle möglichen Gegnerpfade und erlaubten Tower-Bauplätze.
        public List<Vector3> ShortPath = new();
        public List<Vector3> LongPath = new();
        public List<Vector3> FlyingPath = new();
        public List<Vector3> TowerSpots = new();
        // Auf dieser Position liegt die wiederaufladbare Falle des Short Path.
        public Vector3 ShortTrapPosition;
        // Dekoration und Gegnerreihenfolge des Levels.
        public Vector3[] Trees = Array.Empty<Vector3>();
        public EnemyType[][] Waves = Array.Empty<EnemyType[]>();
        // Farbschema für Himmel, Insel, Wege und UI-Akzent.
        public Color SkyTop = new(82, 165, 224);
        public Color SkyBottom = new(213, 238, 244);
        public Color IslandSide = new(90, 169, 99);
        public Color IslandTop = new(118, 195, 116);
        public Color PathBase = new(96, 72, 48);
        public Color PathTop = new(190, 151, 95);
        public Color Accent = new(88, 155, 160);
    }

    // Zahlenwerte einer Schwierigkeitsstufe.
    private sealed class DifficultySettings
    {
        // Startressourcen und Multiplikatoren für Gegner und Belohnungen.
        public string Name = "";
        public int StartGold;
        public int CastleHp;
        public float EnemySpeedMul;
        public float EnemyHealthMul;
        public float RewardMul;
    }
}
