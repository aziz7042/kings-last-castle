using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KingsLastCastle;

// Zeichnet die komplette sichtbare Ausgabe des Spiels: 3D-Welt, Modelle,
// Wege, Effekte, HUD und Hauptmenü. Hier liegen außerdem UI-Rechtecke
// und Hilfsmethoden, mit denen Mausklicks auf 3D-Positionen geprüft werden.
public sealed partial class MilestoneGame
{
    // Zeichnet den Himmel als vertikalen Farbverlauf passend zum aktuellen Level.
    private void DrawSky()
    {
        if (_spriteBatch == null || _pixel == null) return;
        var viewport = GraphicsDevice.Viewport;
        var level = CurLevel();
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
        for (int i = 0; i < 12; i++)
        {
            float t = i / 11f;
            var color = Color.Lerp(level.SkyTop, level.SkyBottom, t);
            _spriteBatch.Draw(_pixel, new Rectangle(0, i * viewport.Height / 12, viewport.Width, viewport.Height / 12 + 2), color);
        }
        _spriteBatch.End();
    }

    // Baut die schwebende Insel aus Grundformen, Dekorationen und Leveldetails auf.
    private void DrawIsland()
    {
        var level = CurLevel();
        DrawCloud(new Vector3(-8.5f, -5.4f, 5.8f), 1.2f);
        DrawCloud(new Vector3(7.2f, -5.1f, 4.6f), 1.0f);
        DrawCloud(new Vector3(0.8f, -6.0f, -7.5f), 1.4f);
        DrawFloatingStone(new Vector3(-7.8f, -2.5f, 4.3f), 0.7f);
        DrawFloatingStone(new Vector3(6.8f, -3.1f, -5.0f), 0.55f);
        DrawFloatingStone(new Vector3(1.0f, -4.0f, -8.0f), 0.45f);

        _renderer!.DrawCone(new Vector3(0f, -0.95f, 0f), 12.7f, new Vector3(0f, -7.9f, 0.8f), 72, new Color(73, 60, 53));
        _renderer.DrawCone(new Vector3(-4.5f, -0.85f, 2.8f), 5.0f, new Vector3(-4.9f, -6.6f, 1.9f), 36, new Color(88, 71, 58));
        _renderer.DrawCone(new Vector3(5.8f, -0.90f, -3.2f), 4.6f, new Vector3(5.2f, -6.4f, -2.0f), 36, new Color(87, 70, 58));
        _renderer.DrawCylinder(new Vector3(0f, -0.63f, 0f), 13.35f, 0.55f, 72, new Color(118, 91, 67));
        _renderer.DrawCylinder(new Vector3(0f, -0.23f, 0f), 13.85f, 0.34f, 72, level.IslandSide);
        _renderer.DrawCylinder(new Vector3(0f, -0.02f, 0f), 13.20f, 0.08f, 72, level.IslandTop);
        DrawWaterfall(new Vector3(-9.8f, -1.8f, 3.0f));

        foreach (var tree in _trees) DrawTree(tree, 0.7f);

        if (_selectedLevel == 1)
        {
            DrawCrystalCluster(new Vector3(-7.8f, 0f, 1.4f), 0.72f);
            DrawCrystalCluster(new Vector3(2.7f, 0f, 6.8f), 0.60f);
            DrawCrystalCluster(new Vector3(7.2f, 0f, -3.2f), 0.66f);
        }
        else if (_selectedLevel == 2)
        {
            DrawRuneStone(new Vector3(-7.6f, 0f, 7.2f));
            DrawRuneStone(new Vector3(-1.4f, 0f, 7.4f));
            DrawRuneStone(new Vector3(7.4f, 0f, -7.1f));
            DrawRuneStone(new Vector3(9.8f, 0f, 6.8f));
        }
        else
        {
            DrawRuneStone(new Vector3(-1.4f, 0f, 7.4f));
            DrawRuneStone(new Vector3(7.4f, 0f, -7.1f));
        }
    }

    // Zeichnet eine kleine Kristallgruppe als Dekoration von Crystal Ridge.
    private void DrawCrystalCluster(Vector3 p, float s)
    {
        var c1 = new Color(88, 225, 235);
        var c2 = new Color(132, 116, 228);
        _renderer!.DrawCone(p + new Vector3(0f, 0.12f * s, 0f), 0.18f * s, p + new Vector3(0f, 1.05f * s, 0f), 6, c1);
        _renderer.DrawCone(p + new Vector3(0.28f * s, 0.10f * s, 0.12f * s), 0.13f * s, p + new Vector3(0.38f * s, 0.72f * s, 0.10f * s), 6, c2);
        _renderer.DrawCone(p + new Vector3(-0.25f * s, 0.10f * s, -0.09f * s), 0.12f * s, p + new Vector3(-0.34f * s, 0.62f * s, -0.12f * s), 6, c1);
        _renderer.DrawRing(p, 0.38f * s, 0.48f * s, 24, new Color(110, 215, 228), 0.08f);
    }

    // Zeichnet einen dekorativen Runenstein für Green Island.
    private void DrawRuneStone(Vector3 p)
    {
        _renderer!.DrawCylinder(p + new Vector3(0f, 0.22f, 0f), 0.28f, 0.44f, 8, new Color(105, 100, 88));
        _renderer.DrawRing(p, 0.36f, 0.43f, 22, new Color(205, 172, 86), 0.08f);
    }

    // Erstellt eine Wolke aus mehreren flachen Zylindern unterhalb der Insel.
    private void DrawCloud(Vector3 p, float s)
    {
        var color = new Color(215, 236, 242);
        _renderer!.DrawCylinder(p, 0.95f * s, 0.12f * s, 18, color);
        _renderer.DrawCylinder(p + new Vector3(0.75f * s, 0.05f, 0.15f * s), 0.65f * s, 0.10f * s, 18, color);
        _renderer.DrawCylinder(p + new Vector3(-0.65f * s, 0.03f, -0.05f * s), 0.70f * s, 0.10f * s, 18, color);
    }

    // Zeichnet einen Wasserfall am Inselrand mit Wasserfläche und Schaum.
    private void DrawWaterfall(Vector3 p)
    {
        _renderer!.DrawCylinder(p, 0.16f, 3.2f, 12, new Color(138, 213, 230));
        _renderer.DrawCylinder(p + new Vector3(0.05f, -2.0f, 0.05f), 0.42f, 0.10f, 18, new Color(210, 236, 240));
    }

    // Zeichnet einen einfachen Baum aus Stamm und kegelförmiger Krone.
    private void DrawTree(Vector3 p, float s)
    {
        _renderer!.DrawCylinder(p + new Vector3(0f, 0.25f * s, 0f), 0.10f * s, 0.50f * s, 8, new Color(101, 70, 45));
        _renderer.DrawCone(p + new Vector3(0f, 0.48f * s, 0f), 0.50f * s, p + new Vector3(0f, 1.45f * s, 0f), 10, new Color(55, 137, 89));
        _renderer.DrawCone(p + new Vector3(0f, 0.88f * s, 0f), 0.38f * s, p + new Vector3(0f, 1.75f * s, 0f), 10, new Color(85, 166, 100));
    }

    // Zeichnet einen kleinen schwebenden Felsen unterhalb der Insel.
    private void DrawFloatingStone(Vector3 p, float s)
    {
        _renderer!.DrawCylinder(p, 0.55f * s, 0.20f * s, 7, new Color(106, 96, 87));
        _renderer.DrawCone(p, 0.62f * s, p + new Vector3(0f, -0.95f * s, 0f), 7, new Color(80, 71, 65));
    }

    // Zeichnet Short Path, Long Path und Flying Path; die aktive Bodenroute wird hervorgehoben.
    private void DrawPathsForCurrentMode()
    {
        var level = CurLevel();
        var active = _selectedPathMode == PathMode.Short ? level.ShortPath : level.LongPath;
        var inactive = _selectedPathMode == PathMode.Short ? level.LongPath : level.ShortPath;
        float pulse = 1f + 0.16f * _routePulse * MathF.Sin(_sceneTime * 18f);

        _renderer!.DrawPath(inactive, 0.96f, new Color(68, 75, 74), 0.035f);
        _renderer.DrawPath(inactive, 0.54f, new Color(102, 100, 92), 0.060f);

        _renderer.DrawPath(active, 1.25f * pulse, level.PathBase, 0.070f);
        _renderer.DrawPath(active, 0.88f * pulse, level.PathTop, 0.105f);
        _renderer.DrawPath(active, 0.18f, _selectedPathMode == PathMode.Long ? level.Accent : new Color(232, 173, 82), 0.145f);

        DrawPathDirectionArrows();
        DrawPathMarkers();
    }

    // Zeichnet die wiederaufladbare Royal Trap direkt auf dem Short Path.
    private void DrawRoyalTrap()
    {
        var p = CurLevel().ShortTrapPosition;
        bool ready = _royalTrapCooldown <= 0f;
        float pulse = ready ? 0.08f * (1f + MathF.Sin(_sceneTime * 6f)) : 0f;
        float flash = Math.Clamp(_royalTrapFlash, 0f, 1f);
        var baseColor = ready ? new Color(250, 205, 70) : new Color(92, 104, 106);
        var glowColor = Color.Lerp(baseColor, new Color(255, 108, 52), flash);

        _renderer!.DrawCylinder(p + new Vector3(0f, GroundY + 0.075f, 0f), 0.62f + pulse, 0.10f, 28, new Color(54, 58, 58));
        _renderer.DrawRing(p, 0.46f, 0.62f + pulse, 32, glowColor, 0.14f);
        _renderer.DrawRing(p, 0.17f, 0.24f, 20, ready ? Color.White : new Color(135, 145, 145), 0.16f);

        for (int i = 0; i < 4; i++)
        {
            float angle = MathHelper.PiOver2 * i + _sceneTime * (ready ? 0.35f : 0.08f);
            var dir = new Vector3(MathF.Cos(angle), 0f, MathF.Sin(angle));
            var side = new Vector3(-dir.Z, 0f, dir.X);
            var center = p + dir * 0.34f + new Vector3(0f, GroundY + 0.17f, 0f);
            _renderer.DrawTriangle(center + dir * 0.13f, center - dir * 0.10f + side * 0.08f, center - dir * 0.10f - side * 0.08f, glowColor);
        }
    }

    // Verteilt Richtungspfeile entlang der aktuell geöffneten Route.
    private void DrawPathDirectionArrows()
    {
        for (int i = 0; i < _path.Count - 1; i++)
        {
            var a = _path[i];
            var b = _path[i + 1];
            var dir = b - a;
            dir.Y = 0f;
            float len = dir.Length();
            if (len < 1.2f) continue;
            dir /= len;
            var perp = new Vector3(-dir.Z, 0f, dir.X);
            int count = Math.Max(1, (int)(len / 4.2f));

            for (int n = 0; n < count; n++)
            {
                float t = (n + 1f) / (count + 1f);
                var c = Vector3.Lerp(a, b, t) + new Vector3(0f, GroundY + 0.090f, 0f);
                DrawPathArrow(c, dir, perp);
            }
        }
    }

    // Zeichnet einen einzelnen dreieckigen Richtungspfeil auf dem Weg.
    private void DrawPathArrow(Vector3 center, Vector3 dir, Vector3 perp)
    {
        var tip = center + dir * 0.34f;
        var left = center - dir * 0.24f + perp * 0.20f;
        var right = center - dir * 0.24f - perp * 0.20f;
        _renderer!.DrawTriangle(tip, left, right, new Color(255, 214, 55));
    }

    // Markiert Spawn, Wegpunkte und Schlossende entlang der aktiven Route.
    private void DrawPathMarkers()
    {
        _renderer!.DrawCylinder(_path[0] + new Vector3(0f, GroundY + 0.10f, 0f), 0.36f, 0.20f, 28, new Color(252, 188, 44));
        _renderer.DrawCylinder(_path[^1] + new Vector3(0f, GroundY + 0.10f, 0f), 0.31f, 0.18f, 28, new Color(93, 146, 196));

        for (int i = 1; i < _path.Count - 1; i++)
        {
            var p = _path[i] + new Vector3(0f, GroundY + 0.08f, 0f);
            _renderer.DrawCylinder(p, 0.13f, 0.13f, 12, new Color(255, 210, 74));
        }
    }

    // Zeichnet das Podest unter dem Schlossmodell.
    private void DrawCastleBase()
    {
        _renderer!.DrawCylinder(_castlePosition, 2.05f, 0.16f, 36, new Color(124, 132, 126));
        _renderer.DrawCylinder(_path[^1] + new Vector3(0f, 0.08f, 0f), 0.62f, 0.08f, 20, new Color(190, 151, 95));
    }

    // Zeichnet alle Tower-Spots und hebt Auswahl, Belegung und Maus-Hover unterschiedlich hervor.
    private void DrawTowerPads()
    {
        for (int i = 0; i < _towerSpots.Count; i++)
        {
            if (SpotOccupied(i)) continue;
            bool selected = i == _selectedSpot && _phase == GamePhase.Preparation;
            float pulse = selected ? 0.06f * MathF.Sin(_sceneTime * 7.0f) : 0f;
            var color = selected ? TowerColor(_selectedTower) : new Color(74, 106, 117);
            var pos = _towerSpots[i] + new Vector3(0f, 0.07f, 0f);
            _renderer!.DrawCylinder(pos, selected ? 0.52f + pulse : 0.42f, 0.12f, 24, color);
            _renderer.DrawRing(_towerSpots[i], selected ? 0.61f : 0.47f, selected ? 0.70f : 0.55f, 24, selected ? new Color(235, 220, 130) : new Color(92, 126, 136), 0.16f);
        }
    }

    // Zeigt während der Vorbereitung die Reichweite des ausgewählten Towers am aktiven Spot.
    private void DrawTowerRangePreview()
    {
        if (_phase != GamePhase.Preparation || !_placementArmed || _selectedSpot < 0 ||
            _selectedSpot >= _towerSpots.Count || SpotOccupied(_selectedSpot)) return;
        var stats = TowerStats(_selectedTower);
        var pos = _towerSpots[_selectedSpot];
        var col = TowerColor(_selectedTower);
        _renderer!.DrawRing(pos, stats.Range - 0.05f, stats.Range + 0.05f, 72, new Color(col.R, col.G, col.B, (byte)115), 0.13f);
        _renderer.DrawRing(pos, stats.Range * 0.48f, stats.Range * 0.48f + 0.035f, 56, new Color(col.R, col.G, col.B, (byte)70), 0.14f);
        DrawModel(GetTowerModel(_selectedTower), pos + new Vector3(0f, GroundY + 0.10f, 0f), 0.00017f, MathHelper.ToRadians(90f), MathHelper.ToRadians(90f), 0f, false);
    }

    // Zeichnet jedes aktive Gegnermodell mit Typ, Flug-Höhe, Rotation und Skalierung.
    private void DrawEnemies()
    {
        foreach (var enemy in _enemies)
        {
            float bob = enemy.Type == EnemyType.Flying ? MathF.Sin(enemy.TimeAlive * 5.8f) * 0.18f : MathF.Abs(MathF.Sin(enemy.TimeAlive * 8.0f)) * 0.025f;
            var groundShadow = enemy.Position + new Vector3(0f, GroundY + 0.045f, 0f);
            _renderer!.DrawCylinder(groundShadow, enemy.Type == EnemyType.Heavy ? 0.34f : 0.26f, 0.025f, 20, new Color(36, 50, 45));
            var pos = enemy.Position + new Vector3(0f, GroundY + (enemy.Type == EnemyType.Flying ? 1.90f + bob : 0.05f + bob), 0f);
            float tiltZ = enemy.Type == EnemyType.Flying ? MathF.Sin(enemy.TimeAlive * 7f) * MathHelper.ToRadians(8f) : 0f;
            DrawModel(GetEnemyModel(enemy.Type), pos, enemy.Scale, MathHelper.ToRadians(90f), enemy.RotationY, tiltZ, false);
        }
    }

    // Zeichnet alle gebauten Tower an ihren festen Bauplätzen.
    private void DrawTowers()
    {
        foreach (var tower in _towers)
        {
            _renderer!.DrawCylinder(tower.Position + new Vector3(0f, GroundY + 0.07f, 0f), 0.60f, 0.14f, 24, new Color(78, 88, 96));
            DrawModel(GetTowerModel(tower.Type), tower.Position + new Vector3(0f, GroundY + 0.08f, 0f), 0.00020f, MathHelper.ToRadians(90f), MathHelper.ToRadians(90f), 0f, false);
        }
    }

    // Zeichnet Laser, Pfeile und Cannon-Projektile anhand ihrer aktuellen Flugposition.
    private void DrawShots()
    {
        foreach (var shot in _shots)
        {
            if (shot.IsLaser)
            {
                _renderer!.DrawBeam(shot.Start, shot.End, 0.060f, shot.Color);
                continue;
            }

            var dir = shot.End - shot.Start;
            if (dir.LengthSquared() < 0.001f)
            {
                continue;
            }

            dir.Normalize();

            if (shot.Type == TowerType.Cannon)
            {
                var tail = shot.Position - dir * 0.55f;

                // Short fire trail behind the cannonball
                _renderer!.DrawBeam(tail, shot.Position, 0.090f, new Color(255, 150, 55));

                // Main cannonball
                _renderer.DrawCylinder(shot.Position, 0.165f, 0.18f, 18, new Color(42, 42, 42));

                // Small bright core so it is visible during gameplay
                _renderer.DrawCylinder(shot.Position + Vector3.Up * 0.03f, 0.090f, 0.10f, 16, new Color(255, 185, 70));
            }
            else if (shot.Type == TowerType.Arrow)
            {
                var tail = shot.Position - dir * 0.80f;
                var head = shot.Position + dir * 0.18f;

                // Thin bright arrow trail
                _renderer!.DrawBeam(tail, head, 0.040f, new Color(255, 232, 120));

                // Small arrow head / glowing point
                _renderer.DrawCylinder(head, 0.075f, 0.12f, 12, new Color(255, 245, 175));
            }
            }
        }
    

    // Zeichnet alle Informationen der Spielrunde: Ressourcen, Tower-Auswahl, Buttons und Hinweise.
    private void DrawHud()
    {
        if (_spriteBatch == null || _font == null || _pixel == null) return;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        DrawEnemyHealthBars();
        DrawPhaseBanner();

        Panel(new Rectangle(18, 18, 294, 170));
        Text("King's Last Castle", 34, 30, Color.White, 1.1f);
        Text($"Castle HP: {_castleHp}", 34, 66, new Color(205, 240, 205));
        Text($"Gold: {_gold}", 34, 94, new Color(245, 210, 100));
        Text($"Wave: {_currentWave + 1}/{_waves.Length}", 34, 122, new Color(180, 225, 240));
        Text($"Enemies: {_waveResolved}/{_waveTotal}", 34, 150, new Color(230, 232, 210));
        Text($"{CurDiff().Name} | L{_selectedLevel + 1}", 198, 154, new Color(205, 225, 226), 0.52f);
        DrawWaveProgressBar(new Rectangle(34, 176, 236, 8));
        DrawCompactTowerSelector();

        var vp = GraphicsDevice.Viewport;
        Panel(new Rectangle(18, vp.Height - 78, 900, 54));
        Text(_message, 34, vp.Height - 66, new Color(255, 230, 140));
        string selectedText = _placementArmed ? _selectedTower.ToString() : "None";
        Text($"Mouse: build/start/route  |  1/2/3: tower  |  Space: wave  |  A/D: camera  |  Esc: pause  |  Selected: {selectedText}",
            34, vp.Height - 42, Color.White, 0.66f);

        if (_phase == GamePhase.Preparation) DrawStartButton();
        if (CanTogglePathControl()) DrawRouteButton();
        if (_phase == GamePhase.Preparation || _phase == GamePhase.WaveActive) DrawRoyalTrapStatus();
        if (_phase == GamePhase.Preparation || _phase == GamePhase.WaveActive) DrawPauseButton();
        if (_phase == GamePhase.Paused) DrawPauseOverlay();
        if (_phase == GamePhase.Victory) DrawEndOverlay("VICTORY!", new Color(255, 220, 60), true);
        if (_phase == GamePhase.Defeat) DrawEndOverlay("DEFEAT", Color.Red, false);

        _spriteBatch.End();
    }

    // Blendet die aktuelle Phase als klare Meldung über der Spielwelt ein.
    private void DrawPhaseBanner()
    {
        var vp = GraphicsDevice.Viewport;
        string text = _phase switch
        {
            GamePhase.Preparation => "PREPARATION",
            GamePhase.WaveActive => $"WAVE {_currentWave + 1} ACTIVE - {_waveResolved}/{_waveTotal} enemies",
            GamePhase.Paused => "PAUSED",
            GamePhase.Victory => "VICTORY!",
            GamePhase.Defeat => "DEFEAT",
            _ => ""
        };
        if (text.Length == 0) return;
        var r = new Rectangle((vp.Width - 380) / 2, 18, 380, 42);
        Rect(r, new Color(14, 30, 34, 220));
        Rect(new Rectangle(r.X, r.Y, r.Width, 4), _phase == GamePhase.WaveActive ? new Color(220, 92, 70) : new Color(85, 150, 118));
        DrawTextCenteredInRect(text, r, Color.White, 0.80f);
    }

    // Zeichnet die kompakte Tower-Liste mit Bild, Hotkey, Kosten und Kampfrolle.
    private void DrawCompactTowerSelector()
    {
        var vp = GraphicsDevice.Viewport;
        var panel = new Rectangle(vp.Width - 390, 18, 372, 108);
        Panel(panel);
        Text("Tower Selection", panel.X + 12, panel.Y + 9, Color.White, 0.76f);
        Text("click or 1 / 2 / 3", panel.Right - 128, panel.Y + 12, new Color(200, 222, 222), 0.52f);

        for (int i = 0; i < 3; i++)
        {
            var type = TowerTypeFromIndex(i);
            var tile = TowerSelectorRow(i);
            bool selected = _placementArmed && type == _selectedTower;
            var accent = TowerColor(type);
            Rect(tile, selected ? new Color(44, 76, 78, 235) : new Color(25, 43, 48, 218));
            Rect(new Rectangle(tile.X, tile.Y, tile.Width, 2), selected ? accent : new Color(90, 116, 118));
            if (selected) Rect(new Rectangle(tile.X, tile.Y, 4, tile.Height), accent);

            var texture = GetTowerPreview(type);
            if (texture != null) DrawTextureFit(texture, new Rectangle(tile.X + 6, tile.Y + 6, 42, 48));
            Text($"{i + 1}", tile.X + 52, tile.Y + 7, accent, 0.52f);
            Text(TowerShortName(type), tile.X + 52, tile.Y + 23, Color.White, 0.55f);
            Text($"{TowerStats(type).Cost}g", tile.X + 52, tile.Y + 42, new Color(245, 216, 122), 0.48f);
        }
    }

    // Projiziert Gegnerpositionen auf den Bildschirm und zeichnet passende Health Bars.
    private void DrawEnemyHealthBars()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.Health >= enemy.MaxHealth) continue;
            var world = enemy.Position + new Vector3(0f, GroundY + (enemy.Type == EnemyType.Flying ? 2.90f : 1.25f), 0f);
            var sc = GraphicsDevice.Viewport.Project(world, _projection, _view, Matrix.Identity);
            if (sc.Z < 0f || sc.Z > 1f) continue;

            const int width = 44;
            const int height = 6;
            int x = (int)sc.X - width / 2;
            int y = (int)sc.Y - 18;
            float ratio = Math.Clamp(enemy.Health / (float)Math.Max(1, enemy.MaxHealth), 0f, 1f);
            Rect(new Rectangle(x - 1, y - 1, width + 2, height + 2), new Color(20, 20, 20, 180));
            Rect(new Rectangle(x, y, width, height), new Color(80, 20, 20, 180));
            Rect(new Rectangle(x, y, (int)(width * ratio), height), enemy.Type == EnemyType.Heavy ? new Color(235, 130, 55) : new Color(90, 220, 90));
        }
    }

    // Zeigt den Anteil bereits besiegter oder am Schloss angekommener Gegner der Welle.
    private void DrawWaveProgressBar(Rectangle r)
    {
        float ratio = _waveTotal <= 0 ? 0f : Math.Clamp(_waveResolved / (float)_waveTotal, 0f, 1f);
        Rect(new Rectangle(r.X - 1, r.Y - 1, r.Width + 2, r.Height + 2), new Color(12, 18, 20, 220));
        Rect(r, new Color(56, 69, 76, 220));
        Rect(new Rectangle(r.X, r.Y, (int)(r.Width * ratio), r.Height), new Color(245, 210, 94));
    }

    // Zeichnet den Victory- oder Defeat-Bildschirm mit Restart- und Menü-Button.
    private void DrawEndOverlay(string title, Color color, bool victory)
    {
        var r = EndOverlayRect();
        Panel(r);
        DrawTextCentered(title, r.Y + 24, color, 1.5f);
        DrawOverlayButton(RestartOverlayButtonRect(), "RESTART", new Color(70, 145, 94, 230));
        DrawOverlayButton(MenuOverlayButtonRect(), "MENU", new Color(72, 126, 132, 230));
        if (victory)
        {
            Text($"Enemies defeated: {_totalKills}  |  Trap hits: {_royalTrapHits}", r.X + 95, r.Y + 150, new Color(245, 210, 100), 0.70f);
            if (_newLevelUnlocked) Text("NEW LEVEL UNLOCKED!", r.X + 123, r.Y + 178, new Color(120, 235, 175), 0.78f);
        }
    }

    // Pause-Menü mit klaren Mausbuttons; Escape setzt das Spiel ebenfalls fort.
    private void DrawPauseOverlay()
    {
        var r = PauseOverlayRect();
        Panel(r);
        DrawTextCentered("PAUSED", r.Y + 26, new Color(245, 210, 100), 1.35f);
        DrawOverlayButton(ResumePauseButtonRect(), "RESUME", new Color(70, 145, 94, 230));
        DrawOverlayButton(PauseMenuButtonRect(), "MENU", new Color(72, 126, 132, 230));
        Text("Press Escape or click Resume", r.X + 118, r.Y + 154, new Color(215, 232, 232), 0.66f);
    }

    // Zeichnet einen einzelnen Button innerhalb des Endbildschirms.
    private void DrawOverlayButton(Rectangle r, string label, Color color)
    {
        Rect(r, color);
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), new Color(225, 220, 150));
        Rect(new Rectangle(r.X, r.Bottom - 2, r.Width, 2), new Color(30, 55, 55));
        Rect(new Rectangle(r.X, r.Y, 2, r.Height), new Color(225, 220, 150));
        Rect(new Rectangle(r.Right - 2, r.Y, 2, r.Height), new Color(30, 55, 55));
        DrawTextCenteredInRect(label, r, Color.White, 0.86f);
    }

    // Zeichnet den Button zum Starten der Welle einschließlich Hover-Zustand.
    private void DrawStartButton()
    {
        var r = StartButtonRect();
        var color = _towers.Count == 0 ? new Color(85, 100, 104, 220) : new Color(70, 145, 94, 230);
        Rect(r, color);
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), new Color(225, 220, 150));
        Rect(new Rectangle(r.X, r.Bottom - 2, r.Width, 2), new Color(30, 70, 50));
        Rect(new Rectangle(r.X, r.Y, 2, r.Height), new Color(225, 220, 150));
        Rect(new Rectangle(r.Right - 2, r.Y, 2, r.Height), new Color(30, 70, 50));
        DrawTextCenteredInRect("START WAVE", r, Color.White, 1f);
    }

    // Zeichnet den Short-/Long-Path-Schalter einschließlich aktivem Routennamen.
    private void DrawRouteButton()
    {
        var r = RouteButtonRect();
        var color = _selectedPathMode == PathMode.Long ? new Color(72, 126, 132, 225) : new Color(155, 116, 70, 225);
        Rect(r, color);
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), new Color(225, 220, 150));
        Rect(new Rectangle(r.X, r.Bottom - 2, r.Width, 2), new Color(45, 70, 72));
        Rect(new Rectangle(r.X, r.Y, 2, r.Height), new Color(225, 220, 150));
        Rect(new Rectangle(r.Right - 2, r.Y, 2, r.Height), new Color(45, 70, 72));
        DrawTextCenteredInRect(_selectedPathMode == PathMode.Long ? "ROUTE: LONG" : "ROUTE: SHORT", r, Color.White, 0.78f);
    }

    // Zeigt, wann die Short-Path-Falle erneut eingesetzt werden kann.
    private void DrawRoyalTrapStatus()
    {
        var route = RouteButtonRect();
        var r = new Rectangle(route.X, route.Y - 34, route.Width, 28);
        var color = _royalTrapCooldown <= 0f ? new Color(120, 102, 45, 225) : new Color(48, 68, 72, 220);
        Rect(r, color);
        DrawTextCenteredInRect($"ROYAL TRAP: {RoyalTrapStatusText()}", r, Color.White, 0.54f);
    }

    private void DrawPauseButton()
    {
        var r = PauseButtonRect();
        Rect(r, new Color(48, 68, 72, 225));
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), new Color(205, 220, 190));
        DrawTextCenteredInRect("PAUSE", r, Color.White, 0.62f);
    }

    private Rectangle PauseButtonRect() => new(338, 18, 102, 42);

    // Berechnet die anklickbare Fläche des Start-Buttons abhängig von der Fenstergröße.
    private Rectangle StartButtonRect()
    {
        var vp = GraphicsDevice.Viewport;
        return new Rectangle(vp.Width - 220, vp.Height - 78, 190, 54);
    }

    // Berechnet die anklickbare Fläche des Route-Buttons.
    private Rectangle RouteButtonRect()
    {
        var start = StartButtonRect();
        return new Rectangle(start.X, start.Y - 62, start.Width, start.Height);
    }

    // Liefert das zentrale Rechteck für Victory- und Defeat-Overlay.
    private Rectangle EndOverlayRect()
    {
        var vp = GraphicsDevice.Viewport;
        return new Rectangle((vp.Width - 460) / 2, (vp.Height - 218) / 2, 460, 218);
    }

    // Liefert die Klickfläche für den Restart-Button im Endbildschirm.
    private Rectangle RestartOverlayButtonRect()
    {
        var r = EndOverlayRect();
        return new Rectangle(r.X + 72, r.Y + 92, 140, 42);
    }

    // Liefert die Klickfläche für den Menü-Button im Endbildschirm.
    private Rectangle MenuOverlayButtonRect()
    {
        var r = EndOverlayRect();
        return new Rectangle(r.X + 248, r.Y + 92, 140, 42);
    }

    private Rectangle PauseOverlayRect()
    {
        var vp = GraphicsDevice.Viewport;
        return new Rectangle((vp.Width - 460) / 2, (vp.Height - 196) / 2, 460, 196);
    }

    private Rectangle ResumePauseButtonRect()
    {
        var r = PauseOverlayRect();
        return new Rectangle(r.X + 72, r.Y + 88, 140, 42);
    }

    private Rectangle PauseMenuButtonRect()
    {
        var r = PauseOverlayRect();
        return new Rectangle(r.X + 248, r.Y + 88, 140, 42);
    }

    // Zeichnet das Hauptmenü über einer weiterhin sichtbaren Vorschau der 3D-Spielwelt.
    private void DrawMenu()
    {
        if (_spriteBatch == null || _font == null || _pixel == null) return;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        var vp = GraphicsDevice.Viewport;
        Rect(new Rectangle(0, 0, vp.Width, vp.Height), new Color(8, 16, 20, 150));
        if (_showCredits)
        {
            DrawCreditsPage();
            _spriteBatch.End();
            return;
        }

        Panel(new Rectangle((vp.Width - 690) / 2, 64, 690, 548));
        DrawTextCenteredShadow("King's Last Castle", 112, Color.White, 1.65f);
        DrawTextCentered("Final Build", 164, new Color(245, 210, 100), 0.95f);
        DrawMenuBadges();

        for (int i = 0; i < MenuItemCount; i++) DrawMenuButton(i);
        DrawMenuSummary();

        DrawTextCentered("Mouse: choose and build  |  1/2/3 towers  |  Space wave  |  A/D camera  |  Esc pause", 638, new Color(215, 235, 240), 0.64f);
        _spriteBatch.End();
    }

    // Zeichnet einen der drei Menüpunkte und hebt die aktuelle Auswahl hervor.
    private void DrawMenuButton(int idx)
    {
        var r = MenuButton(idx);
        bool selected = idx == _selectedMenuItem;
        Rect(r, selected ? new Color(89, 147, 120, 238) : new Color(24, 42, 48, 218));
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), selected ? new Color(245, 214, 120) : new Color(95, 126, 128));
        if (selected) Rect(new Rectangle(r.X, r.Y, 7, r.Height), new Color(245, 214, 120));

        string label = idx switch
        {
            0 => "Start Game",
            1 => $"Difficulty: {CurDiff().Name}",
            2 => $"Level: {_selectedLevel + 1} - {CurLevel().Name}",
            _ => "Credits"
        };
        var sz = _font!.MeasureString(label);
        Text(label, (int)(r.X + (r.Width - sz.X) * 0.5f), r.Y + 17, Color.White, 1f);
        if (selected) Text(">", r.X + 22, r.Y + 17, new Color(255, 232, 132), 1f);
    }

    // Zeigt die wichtigsten Merkmale der aktuellen Version als kompakte Menü-Badges.
    private void DrawMenuBadges()
    {
        var vp = GraphicsDevice.Viewport;
        int x = (vp.Width - 570) / 2;
        int y = 204;
        DrawBadge(new Rectangle(x, y, 178, 34), "3 Waves", new Color(64, 104, 125, 230));
        DrawBadge(new Rectangle(x + 196, y, 178, 34), "Easy / Hard", new Color(110, 90, 130, 230));
        DrawBadge(new Rectangle(x + 392, y, 178, 34), "Live Path Switch", new Color(120, 95, 62, 230));
    }

    // Fasst Level, Schwierigkeit, Startwerte und Pfadkontrolle im Menü zusammen.
    private void DrawMenuSummary()
    {
        var vp = GraphicsDevice.Viewport;
        var diff = CurDiff();
        var r = new Rectangle((vp.Width - 590) / 2, 535, 590, 38);
        Rect(r, new Color(12, 26, 30, 210));
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), new Color(245, 214, 120));
        string text = $"{CurLevel().Name} | {diff.Name}: {diff.StartGold}g, {diff.CastleHp} HP | Levels {_unlockedLevelCount}/{_levels.Count}";
        var sz = _font!.MeasureString(text) * 0.75f;
        Text(text, (int)(r.X + (r.Width - sz.X) * 0.5f), r.Y + 10, new Color(230, 242, 238), 0.75f);
    }

    // Zeichnet ein kleines farbiges Informationsfeld im Hauptmenü.
    private void DrawBadge(Rectangle r, string label, Color col)
    {
        Rect(r, col);
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), new Color(230, 230, 190));
        var sz = _font!.MeasureString(label) * 0.72f;
        Text(label, (int)(r.X + (r.Width - sz.X) * 0.5f), r.Y + 9, Color.White, 0.72f);
    }

    // Berechnet die Klickfläche eines Menüpunkts anhand seines Index.
    private Rectangle MenuButton(int idx)
    {
        var vp = GraphicsDevice.Viewport;
        return new Rectangle((vp.Width - 520) / 2, 264 + idx * 66, 520, 52);
    }

    // Credits bleiben bewusst kompakt und verwenden denselben Menü-Stil wie der Rest des Spiels.
    private void DrawCreditsPage()
    {
        var vp = GraphicsDevice.Viewport;
        var panel = new Rectangle((vp.Width - 650) / 2, 94, 650, 500);
        Panel(panel);
        DrawTextCenteredShadow("CREDITS", 126, Color.White, 1.45f);
        DrawTextCentered("Fire_Team", 178, new Color(245, 210, 100), 0.92f);

        Text("Aziz Zarrouki", panel.X + 86, panel.Y + 150, Color.White, 0.82f);
        Text("Gameplay, game state, HUD and progression", panel.X + 86, panel.Y + 181, new Color(195, 220, 220), 0.62f);
        Text("Jihed Khattat", panel.X + 86, panel.Y + 228, Color.White, 0.82f);
        Text("Enemies, paths, levels and game world", panel.X + 86, panel.Y + 259, new Color(195, 220, 220), 0.62f);
        Text("Rayan Brigui", panel.X + 86, panel.Y + 306, Color.White, 0.82f);
        Text("Towers, combat, audio, camera and presentation", panel.X + 86, panel.Y + 337, new Color(195, 220, 220), 0.62f);

        DrawOverlayButton(CreditsBackButtonRect(), "BACK", new Color(72, 126, 132, 230));
    }

    private Rectangle CreditsBackButtonRect()
    {
        var vp = GraphicsDevice.Viewport;
        return new Rectangle((vp.Width - 160) / 2, 520, 160, 44);
    }

    // Berechnet die Zeile eines Towers in der kompakten Tower-Auswahl.
    private Rectangle TowerSelectorRow(int idx)
    {
        var vp = GraphicsDevice.Viewport;
        return new Rectangle(vp.Width - 380 + idx * 116, 54, 108, 60);
    }

    // Ermittelt, ob der Mauszeiger über einer Tower-Zeile liegt.
    private TowerType? TowerUnderMouse(MouseState mouse)
    {
        for (int i = 0; i < 3; i++)
        {
            if (TowerSelectorRow(i).Contains(mouse.X, mouse.Y)) return TowerTypeFromIndex(i);
        }
        return null;
    }

    // Sucht den Tower-Spot, dessen projizierte Bildschirmposition am nächsten zum Mauszeiger liegt.
    private int AnySpotUnderMouse(int mx, int my)
    {
        int best = -1;
        float bestDist = 40f;
        var mouse = new Vector2(mx, my);
        for (int i = 0; i < _towerSpots.Count; i++)
        {
            if (SpotOccupied(i)) continue;
            var sc = GraphicsDevice.Viewport.Project(_towerSpots[i] + new Vector3(0f, GroundY + 0.25f, 0f), _projection, _view, Matrix.Identity);
            if (sc.Z < 0f || sc.Z > 1f) continue;
            float dist = Vector2.Distance(mouse, new Vector2(sc.X, sc.Y));
            if (dist < bestDist)
            {
                best = i;
                bestDist = dist;
            }
        }
        return best;
    }

    // Skaliert eine Textur proportional in das Zielrechteck, ohne sie zu verzerren.
    private void DrawTextureFit(Texture2D tex, Rectangle bounds)
    {
        float scale = Math.Min(bounds.Width / (float)tex.Width, bounds.Height / (float)tex.Height);
        int w = Math.Max(1, (int)(tex.Width * scale));
        int h = Math.Max(1, (int)(tex.Height * scale));
        _spriteBatch!.Draw(tex, new Rectangle(bounds.X + (bounds.Width - w) / 2, bounds.Y + (bounds.Height - h) / 2, w, h), Color.White);
    }

    // Lädt eine optionale UI-PNG direkt aus Content/UI; bei Fehlen wird ohne Bild weitergearbeitet.
    private Texture2D? LoadUiTexture(string name)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Content", "Ui", name);
        if (!File.Exists(path)) return null;
        using var stream = File.OpenRead(path);
        return Texture2D.FromStream(GraphicsDevice, stream);
    }

    // Zeichnet einen halbtransparenten UI-Hintergrund mit Rand.
    private void Panel(Rectangle r)
    {
        Rect(r, new Color(20, 32, 38, 210));
        Rect(new Rectangle(r.X, r.Y, r.Width, 2), new Color(225, 200, 130));
        Rect(new Rectangle(r.X, r.Bottom - 2, r.Width, 2), new Color(90, 120, 130));
        Rect(new Rectangle(r.X, r.Y, 2, r.Height), new Color(225, 200, 130));
        Rect(new Rectangle(r.Right - 2, r.Y, 2, r.Height), new Color(90, 120, 130));
    }

    // Zeichnet ein einfarbiges 2D-Rechteck mit der weißen 1x1-Pixeltextur.
    private void Rect(Rectangle r, Color color) => _spriteBatch!.Draw(_pixel!, r, color);

    // Zentriert Text horizontal im gesamten Fenster.
    private void DrawTextCentered(string text, int y, Color color, float scale = 1f)
    {
        var size = _font!.MeasureString(text) * scale;
        Text(text, (int)((GraphicsDevice.Viewport.Width - size.X) * 0.5f), y, color, scale);
    }

    // Zentriert Text horizontal und vertikal innerhalb eines Rechtecks.
    private void DrawTextCenteredInRect(string text, Rectangle r, Color color, float scale = 1f)
    {
        var size = _font!.MeasureString(text) * scale;
        Text(text, (int)(r.X + (r.Width - size.X) * 0.5f), (int)(r.Y + (r.Height - size.Y) * 0.5f), color, scale);
    }

    // Zeichnet zentrierten Text mit Schatten für bessere Lesbarkeit vor der 3D-Welt.
    private void DrawTextCenteredShadow(string text, int y, Color color, float scale)
    {
        var size = _font!.MeasureString(text) * scale;
        int x = (int)((GraphicsDevice.Viewport.Width - size.X) * 0.5f);
        Text(text, x + 3, y + 4, new Color(0, 0, 0, 160), scale);
        Text(text, x, y, color, scale);
    }

    // Zeichnet normalen UI-Text an einer festen Bildschirmposition.
    private void Text(string text, int x, int y, Color color, float scale = 1f)
        => _spriteBatch!.DrawString(_font!, text, new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

    // Ordnet jedem Tower-Typ seine UI- und Schussfarbe zu.
    private static Color TowerColor(TowerType type) => type switch
    {
        TowerType.Cannon => new Color(145, 155, 162),
        TowerType.Crystal => new Color(94, 225, 238),
        _ => new Color(235, 210, 120)
    };

    // Liefert das passende Vorschaubild für die Tower-Auswahl.
    private Texture2D? GetTowerPreview(TowerType type) => type switch
    {
        TowerType.Cannon => _cannonTowerPreview,
        TowerType.Crystal => _crystalTowerPreview,
        _ => _arrowTowerPreview
    };

    // Liefert das 3D-Modell des angeforderten Gegner-Typs.
    private Model? GetEnemyModel(EnemyType type) => type switch
    {
        EnemyType.Heavy => _heavyEnemyModel,
        EnemyType.Flying => _flyingEnemyModel,
        _ => _standardEnemyModel
    };

    // Liefert das 3D-Modell des angeforderten Tower-Typs.
    private Model? GetTowerModel(TowerType type) => type switch
    {
        TowerType.Cannon => _cannonTowerModel,
        TowerType.Crystal => _magicTowerModel,
        _ => _arrowTowerModel
    };

    // Liefert den vollständigen Anzeigenamen eines Towers.
    private static string TowerDisplayName(TowerType type) => type switch
    {
        TowerType.Cannon => "Cannon",
        TowerType.Crystal => "Crystal Tower",
        _ => "Arrow Tower"
    };

    // Liefert einen kurzen Namen für kompakte HUD-Bereiche.
    private static string TowerShortName(TowerType type) => type switch
    {
        TowerType.Cannon => "Cannon",
        TowerType.Crystal => "Crystal",
        _ => "Arrow"
    };

    // Wandelt eine Zeilennummer der Tower-Liste in den zugehörigen Tower-Typ um.
    private static TowerType TowerTypeFromIndex(int index) => index switch
    {
        1 => TowerType.Cannon,
        2 => TowerType.Crystal,
        _ => TowerType.Arrow
    };

    // Zeichnet ein FBX-Modell mit Position, Skalierung, Rotation und optionaler Beleuchtung.
    private void DrawModel(Model? model, Vector3 pos, float scale, float rx, float ry, float rz, bool lit)
    {
        if (model == null) return;
        var bones = new Matrix[model.Bones.Count];
        model.CopyAbsoluteBoneTransformsTo(bones);
        var world = Matrix.CreateScale(scale)
            * Matrix.CreateRotationX(rx)
            * Matrix.CreateRotationY(ry)
            * Matrix.CreateRotationZ(rz)
            * Matrix.CreateTranslation(pos);

        foreach (var mesh in model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                if (effect is not BasicEffect basicEffect) continue;
                basicEffect.World = bones[mesh.ParentBone.Index] * world;
                basicEffect.View = _view;
                basicEffect.Projection = _projection;
                basicEffect.LightingEnabled = lit;
                if (lit) basicEffect.EnableDefaultLighting();
            }
            mesh.Draw();
        }
    }

}
