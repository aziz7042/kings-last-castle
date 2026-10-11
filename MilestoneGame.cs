using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using KingsLastCastle.Rendering;

namespace KingsLastCastle;

// Zentrale Spielklasse: Sie verbindet Game Loop, Eingabe, Spielzustand,
// Gegnerbewegung, Tower-Kampf und den Wechsel zwischen Menue und Spielrunde.
// Weitere Teile derselben Klasse liegen in den Dateien Data, Rendering, Audio und Types.
public sealed partial class MilestoneGame : Game
{
    // Team-Aufteilung:
    // Aziz: Spielablauf, Game-State, Menue, Wellenstart, Gold/HP, HUD, Start/Restart sowie Sieg-/Niederlage-Logik.
    // Jihed: Gegner-System, Spawn-Logik, Pfadbewegung, Gegnerabstand, Level-/Pfad-Daten und prozedurale Spielwelt.
    // Rayan: Tower-System, Bauplaetze, Tower-Kampf, Balancing, Audio, Schuss-Effekte sowie Kamera-/3D-Modell-Darstellung.
    // Integration, Testing und Feinanpassungen wurden gemeinsam durchgefuehrt.

    // Grundlegende MonoGame-Ressourcen fuer Fenster, 2D-Oberflaeche und 3D-Hilfsformen.
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;
    private Texture2D? _pixel;
    private PrimitiveRenderer? _renderer;

    // Geladene 3D-Modelle fuer Schloss, Tower und Gegner.
    private Model? _castleModel;
    private Model? _arrowTowerModel;
    private Model? _cannonTowerModel;
    private Model? _magicTowerModel;
    private Model? _standardEnemyModel;
    private Model? _heavyEnemyModel;
    private Model? _flyingEnemyModel;

    // Vorschaubilder fuer die Tower-Auswahl im HUD.
    private Texture2D? _arrowTowerPreview;
    private Texture2D? _cannonTowerPreview;
    private Texture2D? _crystalTowerPreview;

    // Audio-Ressourcen fuer Tower, Schloss-Schaden und Hintergrundmusik.
    private SoundEffect? _arrowSound;
    private SoundEffect? _cannonSound;
    private SoundEffect? _crystalSound;
    private SoundEffect? _castleDamageSound;
    private SoundEffect? _victorySound;
    private SoundEffect? _defeatSound;
    private SoundEffect? _royalTrapSound;
    private Song? _backgroundMusic;

    // Aktive Spielobjekte sowie alle verfuegbaren Level- und Schwierigkeitsdaten.
    private readonly List<Enemy> _enemies = new();
    private readonly List<Tower> _towers = new();
    private readonly List<Shot> _shots = new();
    private readonly List<LevelDefinition> _levels;
    private readonly DifficultySettings[] _difficulties;

    // Zentrale Konstanten fuer Spawn-Abstand, Welt-Hoehe und Kamera.
    private const float SpawnDelay = 1.15f;
    private const float EnemySpacing = 1.25f;
    private const float GroundY = 0f;
    private const int MenuItemCount = 4;
    private const float CastleArrivalStopDistance = 0.0001f;
    private const float RoyalTrapRecharge = 11f;
    private const float RoyalTrapRadius = 1.75f;
    private const int RoyalTrapDamage = 38;
    private static readonly Vector3 CameraTarget = new(0.0f, -0.70f, 0.0f);
    private static readonly Vector3 CameraBaseOffset = new(0.0f, 18.40f, 33.20f);

    // Daten des aktuell ausgewaehlten Levels.
    private EnemyType[][] _waves = Array.Empty<EnemyType[]>();
    private List<Vector3> _path = new();
    private List<Vector3> _towerSpots = new();
    private Vector3[] _trees = Array.Empty<Vector3>();
    private Vector3 _castlePosition;
    private Vector3 _cameraPosition;
    private Matrix _view;
    private Matrix _projection;

    // Veraenderliche Werte der aktuellen Spielrunde.
    private int _gold;
    private int _castleHp;
    private int _currentWave;
    private int _nextEnemy;
    private int _selectedSpot;
    private int _selectedLevel;
    private int _selectedMenuItem;
    private int _totalKills;
    private int _waveResolved;
    private int _waveTotal;
    private float _spawnTimer;
    private float _cameraYaw;
    private float _sceneTime;
    private float _routePulse;
    private float _royalTrapCooldown;
    private float _royalTrapFlash;
    private float _screenShakeTime;
    private float _screenShakeStrength;
    private int _royalTrapHits;
    private int _unlockedLevelCount = 1;
    private bool _placementArmed = true;
    private bool _showCredits;
    private bool _newLevelUnlocked;
    private GamePhase _phaseBeforePause = GamePhase.Preparation;
    // Aktuelle Auswahl, Spielphase sowie vorherige Eingabezustaende fuer einzelne Klicks/Tastendruecke.
    private TowerType _selectedTower = TowerType.Arrow;
    private Difficulty _selectedDifficulty = Difficulty.Easy;
    private PathMode _selectedPathMode = PathMode.Short;
    private GamePhase _phase = GamePhase.MainMenu;
    private KeyboardState _oldKeyboard;
    private MouseState _oldMouse;
    private string _message = "Use the ROUTE button to switch between the short and long path.";

    // Richtet das Spielfenster ein, erstellt Leveldaten und setzt die erste Runde zurueck.
    public MilestoneGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            GraphicsProfile = GraphicsProfile.HiDef,
            PreferMultiSampling = true,
            SynchronizeWithVerticalRetrace = true
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "King's Last Castle - Final Build";

        _levels = CreateLevels();
        _difficulties = CreateDifficulties();
        LoadProgress();
        ValidateGameDefinitions();
        ApplySelectedSetup(resetRun: true);
    }

    // Laedt alle Modelle, UI-Texturen, Schriftarten und Audiodateien nach dem Erstellen des Fensters.
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("Default");
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _renderer = new PrimitiveRenderer(GraphicsDevice);

        _castleModel = Content.Load<Model>("Models/kings_castle");
        _arrowTowerModel = Content.Load<Model>("Models/arrow_Tower");
        _cannonTowerModel = Content.Load<Model>("Models/cannon_Tower");
        _magicTowerModel = Content.Load<Model>("Models/magic_Tower");
        _standardEnemyModel = Content.Load<Model>("Models/standard_enemy");
        _heavyEnemyModel = Content.Load<Model>("Models/heavy_enemy");
        _flyingEnemyModel = Content.Load<Model>("Models/flying_enemy");

        _arrowTowerPreview = LoadUiTexture("tower_arrow.png");
        _cannonTowerPreview = LoadUiTexture("tower_cannon.png");
        _crystalTowerPreview = LoadUiTexture("tower_crystal.png");

        CreateAudio();
        StartMusic();
    }

    // Haupt-Update des Spiels: liest Eingaben und aktualisiert Kamera, Spielrunde und Schuesse.
    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _sceneTime += dt;

        UpdateCamera(keyboard, dt);

        if (_phase == GamePhase.MainMenu)
        {
            HandleMenuInput(keyboard, mouse);
        }
        else
        {
            HandleGameInput(keyboard, mouse);
            if (_phase == GamePhase.WaveActive)
            {
                UpdateGameplay(dt);
            }
            if (_phase != GamePhase.Paused) UpdateShots(dt);
        }

        _routePulse = Math.Max(0f, _routePulse - dt * 1.6f);
        if (_phase != GamePhase.Paused)
        {
            _royalTrapFlash = Math.Max(0f, _royalTrapFlash - dt * 2.4f);
            _screenShakeTime = Math.Max(0f, _screenShakeTime - dt);
            if (_screenShakeTime <= 0f) _screenShakeStrength = 0f;
        }

        _oldKeyboard = keyboard;
        _oldMouse = mouse;
        base.Update(gameTime);
    }

    // Zeichnet zuerst die 3D-Spielwelt und danach je nach Phase Menue oder HUD.
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(124, 195, 232));
        DrawSky();

        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;

        _renderer!.Begin(_view, _projection);
        DrawIsland();
        DrawPathsForCurrentMode();
        DrawRoyalTrap();
        DrawCastleBase();
        DrawTowerPads();
        DrawTowerRangePreview();
        DrawEnemies();
        DrawTowers();
        DrawShots();
        DrawModel(_castleModel, _castlePosition + new Vector3(0f, GroundY, 0f), 0.105f, 0f, MathHelper.ToRadians(270f), 0f, true);

        if (_phase == GamePhase.MainMenu) DrawMenu();
        else DrawHud();

        base.Draw(gameTime);
    }

    // Uebernimmt Level- und Pfaddaten der aktuellen Auswahl in die aktive Spielrunde.
    private void ApplySelectedSetup(bool resetRun)
    {
        var level = _levels[_selectedLevel];
        if (resetRun) _selectedPathMode = PathMode.Short;
        _waves = level.Waves;
        _path = ActiveGroundPath(level);
        _towerSpots = new List<Vector3>(level.TowerSpots);
        _trees = level.Trees;
        _castlePosition = level.CastlePosition;
        if (resetRun) ResetRunValues();
    }

    // Setzt Gegner, Tower, Gold, Schloss-HP und Fortschritt auf die Startwerte zurueck.
    private void ResetRunValues()
    {
        var diff = CurDiff();
        _enemies.Clear();
        _towers.Clear();
        _shots.Clear();
        _routePulse = 0f;
        _royalTrapCooldown = 0f;
        _royalTrapFlash = 0f;
        _screenShakeTime = 0f;
        _screenShakeStrength = 0f;
        _gold = diff.StartGold;
        _castleHp = diff.CastleHp;
        _currentWave = 0;
        _nextEnemy = 0;
        _selectedSpot = 0;
        _spawnTimer = 0f;
        _selectedTower = TowerType.Arrow;
        _placementArmed = true;
        _showCredits = false;
        _newLevelUnlocked = false;
        _royalTrapHits = 0;
        _totalKills = 0;
        _waveResolved = 0;
        _waveTotal = _waves.Length > 0 ? _waves[0].Length : 0;
        _phase = GamePhase.MainMenu;
        _message = "Use the ROUTE button to switch between the short and long path.";
    }

    // Liefert die aktuell ausgewaehlten Level- und Schwierigkeitsdaten.
    private LevelDefinition CurLevel() => _levels[_selectedLevel];
    private DifficultySettings CurDiff() => _difficulties[(int)_selectedDifficulty];

    // Erstellt eine eigene Liste des momentan geoeffneten kurzen oder langen Bodenpfads.
    private List<Vector3> ActiveGroundPath(LevelDefinition level)
        => new(_selectedPathMode == PathMode.Short ? level.ShortPath : level.LongPath);

    // Waehlt fuer Fluggegner den Flugpfad und fuer Bodengegner den aktiven Bodenpfad.
    private List<Vector3> RouteForEnemy(EnemyType type)
    {
        var level = CurLevel();
        if (type == EnemyType.Flying && level.FlyingPath.Count > 1) return new List<Vector3>(level.FlyingPath);
        return ActiveGroundPath(level);
    }

    // Aktualisiert eine aktive Welle und entscheidet danach ueber Sieg, Niederlage oder Vorbereitung.
    private void UpdateGameplay(float dt)
    {
        UpdateEnemies(dt);
        UpdateRoyalTrap(dt);
        UpdateTowers(dt);

        if (_castleHp <= 0)
        {
            _phase = GamePhase.Defeat;
            _message = "The castle has fallen. Use the buttons to restart or return to menu.";
            PlayEndSound(victory: false);
            return;
        }

        if (_nextEnemy >= _waves[_currentWave].Length && _enemies.Count == 0)
        {
            if (_currentWave == _waves.Length - 1)
            {
                _phase = GamePhase.Victory;
                _message = "All waves defeated! Use the buttons to restart or return to menu.";
                _newLevelUnlocked = UnlockNextLevel();
                PlayEndSound(victory: true);
            }
            else
            {
                _currentWave++;
                _nextEnemy = 0;
                _spawnTimer = 0f;
                _waveResolved = 0;
                _waveTotal = _waves[_currentWave].Length;
                _phase = GamePhase.Preparation;
                _royalTrapCooldown = 0f;
                _message = $"Wave {_currentWave} cleared. Build towers, then start next wave.";
            }
        }
    }

    // Verarbeitet alle erlaubten Eingaben innerhalb der Spielrunde: Tower, Route und Buttons.
    private void HandleGameInput(KeyboardState kb, MouseState mouse)
    {
        if (_phase == GamePhase.Paused)
        {
            if (Pressed(kb, Keys.Escape) || (LeftClicked(mouse) && ResumePauseButtonRect().Contains(mouse.X, mouse.Y)))
            {
                ResumeGame();
            }
            else if (LeftClicked(mouse) && PauseMenuButtonRect().Contains(mouse.X, mouse.Y))
            {
                _phase = GamePhase.MainMenu;
                _showCredits = false;
                ResumeMusic();
            }
            return;
        }

        if ((_phase == GamePhase.Preparation || _phase == GamePhase.WaveActive) &&
            (Pressed(kb, Keys.Escape) || (LeftClicked(mouse) && PauseButtonRect().Contains(mouse.X, mouse.Y))))
        {
            PauseGame();
            return;
        }

        if (_phase == GamePhase.Victory || _phase == GamePhase.Defeat)
        {
            if (LeftClicked(mouse) && RestartOverlayButtonRect().Contains(mouse.X, mouse.Y))
            {
                RestartCurrentRun();
            }
            else if (LeftClicked(mouse) && MenuOverlayButtonRect().Contains(mouse.X, mouse.Y))
            {
                _phase = GamePhase.MainMenu;
                _showCredits = false;
            }
            return;
        }

        if (Pressed(kb, Keys.D1)) SelectTower(TowerType.Arrow);
        if (Pressed(kb, Keys.D2)) SelectTower(TowerType.Cannon);
        if (Pressed(kb, Keys.D3)) SelectTower(TowerType.Crystal);

        if (_phase == GamePhase.Preparation && RightClicked(mouse))
        {
            _placementArmed = false;
            _selectedSpot = -1;
            _message = "Tower placement cancelled. Select a tower to continue.";
            return;
        }

        if (_phase == GamePhase.Preparation &&
            ((LeftClicked(mouse) && StartButtonRect().Contains(mouse.X, mouse.Y)) || Pressed(kb, Keys.Space)))
        {
            StartWave();
            return;
        }

        if (CanTogglePathControl() && LeftClicked(mouse) && RouteButtonRect().Contains(mouse.X, mouse.Y))
        {
            TogglePathMode();
            return;
        }

        var hoverTower = TowerUnderMouse(mouse);
        if (hoverTower.HasValue && LeftClicked(mouse))
        {
            SelectTower(hoverTower.Value);
            return;
        }

        if (_phase == GamePhase.Preparation && _placementArmed)
        {
            int spot = AnySpotUnderMouse(mouse.X, mouse.Y);
            if (spot >= 0) _selectedSpot = spot;
            if (spot >= 0 && LeftClicked(mouse)) BuildSelectedTower();
        }
    }

    // Erkennt, welcher Menuepunkt mit Maus oder Touchpad ausgewaehlt und angeklickt wurde.
    private void HandleMenuInput(KeyboardState kb, MouseState mouse)
    {
        if (_showCredits)
        {
            if (Pressed(kb, Keys.Escape) || (LeftClicked(mouse) && CreditsBackButtonRect().Contains(mouse.X, mouse.Y)))
                _showCredits = false;
            return;
        }

        for (int i = 0; i < MenuItemCount; i++)
        {
            if (MenuButton(i).Contains(mouse.X, mouse.Y))
            {
                _selectedMenuItem = i;
                if (LeftClicked(mouse)) ActivateMenuItem();
            }
        }
    }

    // Fuehrt die Aktion des markierten Menuepunkts aus: Start, Schwierigkeit oder Level.
    private void ActivateMenuItem()
    {
        if (_selectedMenuItem == 0)
        {
            ApplySelectedSetup(resetRun: true);
            _phase = GamePhase.Preparation;
            _message = $"{CurLevel().Name} - build towers, then start wave 1.";
            StartMusic();
            return;
        }

        if (_selectedMenuItem == 1) ToggleDifficulty();
        if (_selectedMenuItem == 2) ToggleLevel();
        if (_selectedMenuItem == 3) _showCredits = true;
    }

    // Wechselt zwischen Easy und Hard und uebernimmt die neuen Startwerte.
    private void ToggleDifficulty()
    {
        _selectedDifficulty = _selectedDifficulty == Difficulty.Easy ? Difficulty.Hard : Difficulty.Easy;
        ApplySelectedSetup(resetRun: false);
        _message = $"{CurLevel().Name} | {CurDiff().Name}";
    }

    // Wechselt zyklisch zum naechsten Level und laedt dessen Pfade und Bauplaetze.
    private void ToggleLevel()
    {
        _selectedLevel = (_selectedLevel + 1) % Math.Max(1, _unlockedLevelCount);
        ApplySelectedSetup(resetRun: false);
        _message = $"{CurLevel().Name} | {CurDiff().Name} | {_unlockedLevelCount}/{_levels.Count} levels unlocked";
    }

    // Oeffnet abwechselnd Short Path oder Long Path und informiert den Spieler im HUD.
    private void TogglePathMode()
    {
        if (!CanTogglePathControl()) return;
        _selectedPathMode = _selectedPathMode == PathMode.Short ? PathMode.Long : PathMode.Short;
        _path = ActiveGroundPath(CurLevel());
        int rerouted = RerouteGroundEnemiesBeforeSwitch();
        _routePulse = 1f;
        _message = _selectedPathMode == PathMode.Short
            ? $"SHORT PATH active - Royal Trap {RoyalTrapStatusText()}. {rerouted} rerouted."
            : $"LONG PATH active - more tower time. {rerouted} rerouted.";
    }

    // Leitet nur Bodengegner vor der gemeinsamen Weggabelung auf den neuen Pfad um.
    // Gegner hinter der Weggabelung behalten ihre Route, damit sie nicht sichtbar teleportieren.
    private int RerouteGroundEnemiesBeforeSwitch()
    {
        var level = CurLevel();
        float sharedPrefix = SharedGroundPrefixLength(level) + 0.15f;
        int changed = 0;
        foreach (var enemy in _enemies)
        {
            if (enemy.Type == EnemyType.Flying) continue;
            if (enemy.DistanceOnPath > sharedPrefix) continue;
            enemy.Route = ActiveGroundPath(level);
            enemy.RouteLength = PathLength(enemy.Route);
            enemy.GroundRouteMode = _selectedPathMode;
            changed++;
        }
        return changed;
    }

    // Die Pfadkontrolle ist waehrend Vorbereitung und aktiver Welle verfuegbar.
    private bool CanTogglePathControl() => _phase == GamePhase.Preparation || _phase == GamePhase.WaveActive;

    // Dreht die Kamera mit A/D um die Insel und berechnet View- und Projektionsmatrix neu.
    private void UpdateCamera(KeyboardState kb, float dt)
    {
        const float rotationSpeed = 1.55f;
        if (_phase != GamePhase.MainMenu && _phase != GamePhase.Paused)
        {
            if (kb.IsKeyDown(Keys.A)) _cameraYaw -= rotationSpeed * dt;
            if (kb.IsKeyDown(Keys.D)) _cameraYaw += rotationSpeed * dt;
        }
        _cameraYaw = MathHelper.WrapAngle(_cameraYaw);

        _cameraPosition = CameraPositionForYaw(_cameraYaw);
        var shake = Vector3.Zero;
        if (_screenShakeTime > 0f)
        {
            float fade = Math.Clamp(_screenShakeTime / 0.34f, 0f, 1f);
            shake = new Vector3(
                MathF.Sin(_sceneTime * 73f) * _screenShakeStrength * fade,
                MathF.Cos(_sceneTime * 91f) * _screenShakeStrength * 0.45f * fade,
                MathF.Sin(_sceneTime * 57f) * _screenShakeStrength * 0.35f * fade);
        }
        _view = Matrix.CreateLookAt(_cameraPosition + shake, CameraTarget + shake * 0.20f, Vector3.Up);
        float aspect = GraphicsDevice.Viewport.AspectRatio;
        _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(39f), Math.Max(0.1f, aspect), 0.1f, 100f);
    }

    // Berechnet eine Kameraposition auf der Kreisbahn fuer den gegebenen Drehwinkel.
    private static Vector3 CameraPositionForYaw(float yaw)
        => CameraTarget + Vector3.Transform(CameraBaseOffset, Matrix.CreateRotationY(yaw));

    // Startet das aktuelle Level mit derselben Auswahl noch einmal von vorne.
    private void RestartCurrentRun()
    {
        ApplySelectedSetup(resetRun: true);
        _phase = GamePhase.Preparation;
        _message = $"{CurLevel().Name} - build towers, then start wave 1.";
        StartMusic();
    }

    // Aktiviert einen Tower-Typ und schaltet die Platzierung nach einem Abbruch wieder ein.
    private void SelectTower(TowerType type)
    {
        _selectedTower = type;
        _placementArmed = true;
        if (_selectedSpot < 0) SelectNextFreeSpot();
        _message = $"{TowerDisplayName(_selectedTower)} selected.";
    }

    // Friert die aktuelle Runde ein und merkt sich, ob Vorbereitung oder Welle aktiv war.
    private void PauseGame()
    {
        _phaseBeforePause = _phase;
        _phase = GamePhase.Paused;
        PauseMusic();
    }

    // Kehrt exakt in die Phase zurück, aus der das Pause-Menü geöffnet wurde.
    private void ResumeGame()
    {
        _phase = _phaseBeforePause;
        ResumeMusic();
    }

    // Lädt die höchste freigeschaltete Levelanzahl aus dem lokalen Benutzerprofil.
    private void LoadProgress()
    {
        try
        {
            string path = ProgressFilePath();
            if (File.Exists(path) && int.TryParse(File.ReadAllText(path), out int unlocked))
                _unlockedLevelCount = Math.Clamp(unlocked, 1, _levels.Count);
        }
        catch
        {
            _unlockedLevelCount = 1;
        }
    }

    // Speichert ausschließlich die Level-Freischaltung; Spielstände innerhalb einer Runde werden nicht persistiert.
    private void SaveProgress()
    {
        try
        {
            string path = ProgressFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, _unlockedLevelCount.ToString());
        }
        catch
        {
            // Ein nicht beschreibbares Benutzerprofil darf das Spiel nicht blockieren.
        }
    }

    private static string ProgressFilePath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KingsLastCastle", "progress.txt");

    // Schaltet nach einem Sieg das unmittelbar folgende Level frei.
    private bool UnlockNextLevel()
    {
        int requiredCount = Math.Min(_levels.Count, _selectedLevel + 2);
        if (requiredCount <= _unlockedLevelCount) return false;
        _unlockedLevelCount = requiredCount;
        SaveProgress();
        return true;
    }

    // Beginnt die aktuelle Welle, sobald mindestens ein Tower gebaut wurde.
    private void StartWave()
    {
        if (_towers.Count == 0)
        {
            _message = "Build at least one tower first.";
            return;
        }

        _phase = GamePhase.WaveActive;
        _waveResolved = 0;
        _waveTotal = _waves[_currentWave].Length;
        _message = $"Wave {_currentWave + 1} started.";
        StartMusic();
    }

    // Erstellt einen Gegner mit typ- und schwierigkeitsabhaengigen Kampfwerten.
    private void SpawnEnemy(EnemyType type)
    {
        var diff = CurDiff();
        var route = RouteForEnemy(type);
        var data = type switch
        {
            EnemyType.Heavy => (speed: 0.92f, scale: 0.0075f, hp: 125, reward: 32, dmg: 4),
            EnemyType.Flying => (speed: 1.65f, scale: 0.0060f, hp: 48, reward: 24, dmg: 2),
            _ => (speed: 1.15f, scale: 0.0060f, hp: 62, reward: 18, dmg: 1)
        };

        float waveHealthMultiplier = 1f + _currentWave * 0.25f;
        float waveSpeedMultiplier = 1f + _currentWave * 0.04f;
        int hp = Math.Max(1, (int)(data.hp * diff.EnemyHealthMul * waveHealthMultiplier));
        var enemy = new Enemy
        {
            Type = type,
            DistanceOnPath = 0f,
            Position = route[0],
            RotationY = 0f,
            TimeAlive = 0f,
            Scale = data.scale,
            Route = route,
            GroundRouteMode = type == EnemyType.Flying ? null : _selectedPathMode,
            RouteLength = PathLength(route),
            Speed = data.speed * diff.EnemySpeedMul * waveSpeedMultiplier,
            Health = hp,
            MaxHealth = hp,
            Reward = Math.Max(1, (int)(data.reward * diff.RewardMul)),
            CastleDamage = data.dmg
        };
        _enemies.Add(enemy);
    }

    // Spawnt Gegner mit Abstand, bewegt sie entlang ihrer Route und verarbeitet Schloss-Schaden.
    private void UpdateEnemies(float dt)
    {
        _spawnTimer -= dt;
        if (_nextEnemy < _waves[_currentWave].Length && _spawnTimer <= 0f && CanSpawnNextEnemy())
        {
            SpawnEnemy(_waves[_currentWave][_nextEnemy]);
            _nextEnemy++;
            _spawnTimer = SpawnDelay;
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            enemy.TimeAlive += dt;
            float nextDistance = enemy.DistanceOnPath + enemy.Speed * dt;
            var blocker = FindNearestEnemyAhead(i);
            if (blocker != null) nextDistance = Math.Min(nextDistance, blocker.DistanceOnPath - EnemySpacing);
            enemy.DistanceOnPath = Math.Max(0f, nextDistance);
            enemy.Position = PositionAtDistance(enemy.Route, enemy.DistanceOnPath, out enemy.RotationY);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];

            float arrivalDistance = Math.Max(0f, enemy.RouteLength - CastleArrivalStopDistance);

            if (enemy.DistanceOnPath >= arrivalDistance)
            {
                _castleHp = Math.Max(0, _castleHp - enemy.CastleDamage);
                _castleDamageSound?.Play(0.34f, -0.12f, 0f);
                TriggerScreenShake(enemy.Type == EnemyType.Heavy ? 0.34f : 0.22f);
                _waveResolved++;
                _enemies.RemoveAt(i);
            }
        }
    }

    // Lädt die Falle auf dem Short Path auf und löst sie bei einer Gegnergruppe automatisch aus.
    private void UpdateRoyalTrap(float dt)
    {
        _royalTrapCooldown = Math.Max(0f, _royalTrapCooldown - dt);
        if (_royalTrapCooldown > 0f) return;

        var trapPosition = CurLevel().ShortTrapPosition;
        var targets = _enemies
            .Where(enemy => enemy.Type != EnemyType.Flying &&
                            enemy.GroundRouteMode == PathMode.Short &&
                            Vector3.Distance(enemy.Position, trapPosition) <= RoyalTrapRadius)
            .ToList();

        bool groupIsCentered = targets.Count >= 2 && targets.Any(enemy => Vector3.Distance(enemy.Position, trapPosition) <= 0.48f);
        if (!groupIsCentered) return;

        int damage = RoyalTrapDamage + _currentWave * 8;

        foreach (var enemy in targets) DamageEnemy(enemy, damage);

        _royalTrapHits += targets.Count;
        _royalTrapCooldown = RoyalTrapRecharge;
        _royalTrapFlash = 1f;
        TriggerScreenShake(0.18f);
        PlayRoyalTrapSound();
        _message = $"ROYAL TRAP hit {targets.Count} ground enemies - recharging.";
    }

    // Liefert einen kurzen Status für HUD und Routennachrichten.
    private string RoyalTrapStatusText()
        => _royalTrapCooldown <= 0f ? "READY" : $"CHARGING {Math.Ceiling(_royalTrapCooldown)}s";

    // Startet einen kurzen Kamerastoß, ohne die aktuelle Kameraposition dauerhaft zu verändern.
    private void TriggerScreenShake(float strength)
    {
        _screenShakeTime = 0.34f;
        _screenShakeStrength = Math.Max(_screenShakeStrength, strength);
    }

    // Verhindert, dass der naechste Gegner zu nah am vorherigen Gegner derselben Spur erscheint.
    private bool CanSpawnNextEnemy()
    {
        if (_enemies.Count == 0 || _nextEnemy >= _waves[_currentWave].Length) return true;
        bool nextIsFlying = _waves[_currentWave][_nextEnemy] == EnemyType.Flying;
        var nearestSameLane = _enemies
            .Where(enemy => (enemy.Type == EnemyType.Flying) == nextIsFlying)
            .OrderBy(enemy => enemy.DistanceOnPath)
            .FirstOrDefault();
        return nearestSameLane == null || nearestSameLane.DistanceOnPath >= EnemySpacing;
    }

    // Findet den naechsten Gegner vor dem aktuellen Gegner auf derselben Boden- oder Flugspur.
    private Enemy? FindNearestEnemyAhead(int enemyIndex)
    {
        var enemy = _enemies[enemyIndex];
        Enemy? nearest = null;
        float sharedPrefix = SharedGroundPrefixLength(CurLevel());
        for (int i = 0; i < _enemies.Count; i++)
        {
            if (i == enemyIndex) continue;
            var candidate = _enemies[i];
            bool bothOnSharedPrefix = enemy.Type != EnemyType.Flying && candidate.Type != EnemyType.Flying &&
                enemy.DistanceOnPath <= sharedPrefix && candidate.DistanceOnPath <= sharedPrefix;
            bool sameLane = enemy.Type == EnemyType.Flying
                ? candidate.Type == EnemyType.Flying
                : candidate.Type != EnemyType.Flying &&
                  (candidate.GroundRouteMode == enemy.GroundRouteMode || bothOnSharedPrefix);
            if (!sameLane || candidate.DistanceOnPath <= enemy.DistanceOnPath) continue;
            if (nearest == null || candidate.DistanceOnPath < nearest.DistanceOnPath) nearest = candidate;
        }
        return nearest;
    }

    // Misst den gemeinsamen Anfang von Short Path und Long Path bis zur ersten Abzweigung.
    private static float SharedGroundPrefixLength(LevelDefinition level)
    {
        float length = 0f;
        int commonPoints = Math.Min(level.ShortPath.Count, level.LongPath.Count);
        for (int i = 1; i < commonPoints; i++)
        {
            if (Vector3.Distance(level.ShortPath[i], level.LongPath[i]) > 0.01f) break;
            length += Vector3.Distance(level.ShortPath[i - 1], level.ShortPath[i]);
        }
        return length;
    }

    // Aktualisiert Tower-Cooldowns und laesst schussbereite Tower automatisch angreifen.
    private void UpdateTowers(float dt)
    {
        foreach (var tower in _towers)
        {
            tower.Timer -= dt;
            if (tower.Timer > 0f) continue;

            var target = FindTarget(tower);
            if (target == null) continue;

            FireTower(tower, target);
            tower.Timer = tower.Cooldown;
        }
    }

    // Sucht ein gueltiges Ziel in Reichweite und beachtet die Anti-Air-Regeln der Tower.
    private Enemy? FindTarget(Tower tower)
    {
        IEnumerable<Enemy> candidates = _enemies;
        if (tower.Type == TowerType.Cannon) candidates = candidates.Where(e => e.Type != EnemyType.Flying);

        var inRange = candidates
            .Where(e => Vector3.Distance(e.Position, tower.Position) <= tower.Range);

        if (tower.Type == TowerType.Crystal)
        {
            return inRange
                .OrderByDescending(e => e.Type == EnemyType.Flying ? 1 : 0)
                .ThenByDescending(e => e.DistanceOnPath)
                .FirstOrDefault();
        }

        return inRange
            .OrderByDescending(e => e.DistanceOnPath)
            .FirstOrDefault();
    }

    // Erzeugt den sichtbaren Schuss. Arrow und Cannon verursachen Schaden erst beim Einschlag.
    // Crystal trifft wegen seines direkten Laserstrahls sofort.
    private void FireTower(Tower tower, Enemy target)
    {
        var start = tower.Position + new Vector3(0f, GroundY + 1.15f, 0f);
        var end = target.Position + new Vector3(0f, GroundY + (target.Type == EnemyType.Flying ? 1.85f : 0.9f), 0f);
        var color = TowerColor(tower.Type);

        _shots.Add(new Shot
        {
            Start = start,
            End = end,
            Position = start,
            Age = 0f,
            Life = tower.Type == TowerType.Cannon ? 0.34f : tower.Type == TowerType.Arrow ? 0.28f : 0.16f,
            Color = color,
            IsLaser = tower.Type == TowerType.Crystal,
            Type = tower.Type,
            Target = target,
            Damage = tower.Damage,
            SplashRadius = tower.Type == TowerType.Cannon ? 1.15f : 0f,
            DamageApplied = tower.Type == TowerType.Crystal
        });

        PlayTowerSound(tower.Type);
        if (tower.Type == TowerType.Crystal) DamageEnemy(target, tower.Damage);
    }

    // Zieht Lebenspunkte ab und vergibt bei einem Kill Gold sowie Fortschritt.
    private void DamageEnemy(Enemy enemy, int damage)
    {
        enemy.Health -= damage;
        if (enemy.Health > 0)
        {
            return;
        }

        _gold += enemy.Reward;
        _totalKills++;
        _waveResolved++;
        _enemies.Remove(enemy);
    }

    // Bewegt sichtbare Projektile zwischen Start und Ziel; Cannon und Arrow erhalten eine Flugkurve.
    private void UpdateShots(float dt)
    {
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            var shot = _shots[i];
            shot.Age += dt;

            if (!shot.DamageApplied && shot.Target != null && _enemies.Contains(shot.Target))
            {
                shot.End = shot.Target.Position + new Vector3(
                    0f,
                    GroundY + (shot.Target.Type == EnemyType.Flying ? 1.85f : 0.9f),
                    0f);
            }

            float life = Math.Max(0.001f, shot.Life);
            float t = Math.Clamp(shot.Age / life, 0f, 1f);

            var pos = Vector3.Lerp(shot.Start, shot.End, t);

            if (!shot.IsLaser)
            {
                float arc = shot.Type == TowerType.Cannon ? 0.45f : 0.18f;
                pos += Vector3.Up * (MathF.Sin(t * MathHelper.Pi) * arc);
            }

            shot.Position = pos;

            if (shot.Age >= life)
            {
                ApplyShotImpact(shot);
                _shots.RemoveAt(i);
            }
        }
    }

    // Wendet den gespeicherten Projektilschaden genau beim sichtbaren Einschlag an.
    private void ApplyShotImpact(Shot shot)
    {
        if (shot.DamageApplied) return;
        shot.DamageApplied = true;

        if (shot.Type == TowerType.Cannon)
        {
            foreach (var enemy in _enemies
                .Where(enemy => enemy.Type != EnemyType.Flying &&
                                Vector2.Distance(new Vector2(enemy.Position.X, enemy.Position.Z), new Vector2(shot.End.X, shot.End.Z)) <= shot.SplashRadius)
                .ToList())
            {
                DamageEnemy(enemy, shot.Damage);
            }
            return;
        }

        if (shot.Target != null && _enemies.Contains(shot.Target)) DamageEnemy(shot.Target, shot.Damage);
    }

    // Baut den ausgewaehlten Tower auf dem markierten freien Spot, wenn genug Gold vorhanden ist.
    private void BuildSelectedTower()
    {
        if (!_placementArmed || _selectedSpot < 0 || _selectedSpot >= _towerSpots.Count) return;
        if (SpotOccupied(_selectedSpot))
        {
            _message = "This spot is already occupied.";
            return;
        }

        var stats = TowerStats(_selectedTower);
        if (_gold < stats.Cost)
        {
            _message = $"Not enough gold. Needed: {stats.Cost}g";
            return;
        }

        var pos = _towerSpots[_selectedSpot];
        _towers.Add(new Tower
        {
            Type = _selectedTower,
            SpotIndex = _selectedSpot,
            Position = pos,
            Range = stats.Range,
            Cooldown = stats.Cooldown,
            Timer = 0f,
            Damage = stats.Damage
        });
        _gold -= stats.Cost;
        _message = $"{TowerDisplayName(_selectedTower)} built.";
        SelectNextFreeSpot();
    }

    // Prueft, ob auf einem Tower-Spot bereits ein Tower steht.
    private bool SpotOccupied(int spot) => _towers.Any(t => t.SpotIndex == spot);

    // Markiert nach dem Bauen automatisch den naechsten noch freien Tower-Spot.
    private void SelectNextFreeSpot()
    {
        int start = Math.Max(-1, _selectedSpot);
        for (int offset = 1; offset <= _towerSpots.Count; offset++)
        {
            int candidate = (start + offset) % _towerSpots.Count;
            if (!SpotOccupied(candidate))
            {
                _selectedSpot = candidate;
                return;
            }
        }
    }

    // Liefert Kosten und Kampfwerte des angeforderten Tower-Typs.
    private static (int Cost, int Damage, float Range, float Cooldown) TowerStats(TowerType type) => type switch
    {
        TowerType.Cannon => (75, 32, 3.8f, 1.45f),
        TowerType.Crystal => (70, 24, 5.0f, 1.05f),
        _ => (50, 12, 4.4f, 0.72f)
    };

    // Addiert die Laenge aller Segmente eines Wegpunkt-Pfads.
    private static float PathLength(IReadOnlyList<Vector3> path)
    {
        float length = 0f;
        for (int i = 0; i < path.Count - 1; i++) length += Vector3.Distance(path[i], path[i + 1]);
        return length;
    }

    // Bestimmt Position und Blickrichtung eines Gegners anhand seiner Distanz auf dem Pfad.
    private static Vector3 PositionAtDistance(IReadOnlyList<Vector3> path, float distance, out float rotationY)
    {
        float remaining = distance;
        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];
            float segment = Vector3.Distance(a, b);
            if (remaining <= segment)
            {
                float t = segment <= 0.001f ? 0f : remaining / segment;
                var direction = b - a;
                rotationY = MathF.Atan2(direction.X, direction.Z);
                return Vector3.Lerp(a, b, t);
            }
            remaining -= segment;
        }

        var lastDirection = path[^1] - path[^2];
        rotationY = MathF.Atan2(lastDirection.X, lastDirection.Z);
        return path[^1];
    }

    // Erkennt einen neuen Tastendruck statt eines dauerhaft gehaltenen Tastenzustands.
    private bool Pressed(KeyboardState keyboard, Keys key) => keyboard.IsKeyDown(key) && !_oldKeyboard.IsKeyDown(key);

    // Erkennt genau den Moment, in dem die linke Maustaste neu gedrueckt wurde.
    private bool LeftClicked(MouseState mouse) => mouse.LeftButton == ButtonState.Pressed && _oldMouse.LeftButton == ButtonState.Released;

    // Erkennt genau den Moment, in dem die rechte Maustaste neu gedrueckt wurde.
    private bool RightClicked(MouseState mouse) => mouse.RightButton == ButtonState.Pressed && _oldMouse.RightButton == ButtonState.Released;

    // Beendet Audio und gibt alle manuell erstellten MonoGame-Ressourcen sauber frei.
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopMusic();
            _backgroundMusic?.Dispose();
            _arrowSound?.Dispose();
            _cannonSound?.Dispose();
            _crystalSound?.Dispose();
            _castleDamageSound?.Dispose();
            _victorySound?.Dispose();
            _defeatSound?.Dispose();
            _royalTrapSound?.Dispose();
            _arrowTowerPreview?.Dispose();
            _cannonTowerPreview?.Dispose();
            _crystalTowerPreview?.Dispose();
            _renderer?.Dispose();
            _spriteBatch?.Dispose();
            _pixel?.Dispose();
        }
        base.Dispose(disposing);
    }
}
