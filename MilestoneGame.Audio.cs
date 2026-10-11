using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace KingsLastCastle;

// Verwaltet ausschließlich die Audio-Seite des Spiels: Laden, Starten,
// Stoppen und Abspielen von Hintergrundmusik und Kampf-Soundeffekten.
public sealed partial class MilestoneGame
{
    // Lädt Tower- und Schloss-Sounds direkt aus Content/Audio und bereitet die Musik vor.
    // Falls eine Datei fehlt, bleibt das Spiel spielbar und zeigt nur eine Statusmeldung.
    private void CreateAudio()
    {
        try
        {
            string audioDirectory = Path.Combine(AppContext.BaseDirectory, "Content", "Audio");
            _arrowSound = SoundEffect.FromFile(Path.Combine(audioDirectory, "tower_arrow.wav"));
            _cannonSound = SoundEffect.FromFile(Path.Combine(audioDirectory, "tower_cannon.wav"));
            _crystalSound = SoundEffect.FromFile(Path.Combine(audioDirectory, "tower_crystal.wav"));
            _castleDamageSound = SoundEffect.FromFile(Path.Combine(audioDirectory, "castle_damage.wav"));
            _victorySound = SoundEffect.FromFile(Path.Combine(audioDirectory, "victory.wav"));
            _defeatSound = SoundEffect.FromFile(Path.Combine(audioDirectory, "defeat.wav"));
            _royalTrapSound = SoundEffect.FromFile(Path.Combine(audioDirectory, "royal_trap.wav"));
        }
        catch
        {
            _message = "Tower sound effects could not start, but the game is still playable.";
        }

        try
        {
            string musicPath = Path.Combine(AppContext.BaseDirectory, "Content", "Audio", "the_tree_of_life.ogg");
            if (!File.Exists(musicPath)) throw new FileNotFoundException("Background music file is missing.", musicPath);
            _backgroundMusic = Song.FromUri("The Tree of Life", new Uri(musicPath, UriKind.Absolute));
            MediaPlayer.IsRepeating = true;
            MediaPlayer.IsMuted = false;
            MediaPlayer.Volume = 0.28f;
        }
        catch
        {
            _message = "Background music could not be loaded, but the game is still playable.";
        }
    }

    // Startet die Hintergrundmusik als leise Endlosschleife, sofern sie nicht bereits läuft.
    private void StartMusic()
    {
        if (_backgroundMusic == null || MediaPlayer.State == MediaState.Playing) return;
        try
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.IsMuted = false;
            MediaPlayer.Volume = 0.28f;
            MediaPlayer.Play(_backgroundMusic);
            if (MediaPlayer.State != MediaState.Playing)
                throw new InvalidOperationException("MonoGame did not enter the Playing state.");
        }
        catch
        {
            _message = "Background music playback failed, but the game is still playable.";
        }
    }

    // Stoppt die Musik sicher, ohne dass ein Audiofehler Menüwechsel oder Programmende blockiert.
    private void StopMusic()
    {
        try
        {
            if (MediaPlayer.State != MediaState.Stopped) MediaPlayer.Stop();
        }
        catch
        {
            // Audio shutdown must never block returning to the menu or closing the game.
        }
    }

    // Pausiert und setzt die Musik gemeinsam mit dem Pause-Menü fort.
    private void PauseMusic()
    {
        try
        {
            if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Pause();
        }
        catch
        {
            // Das Pause-Menü bleibt auch ohne Audio-Steuerung funktionsfähig.
        }
    }

    private void ResumeMusic()
    {
        try
        {
            if (MediaPlayer.State == MediaState.Paused) MediaPlayer.Resume();
            else StartMusic();
        }
        catch
        {
            StartMusic();
        }
    }

    // Spielt den passenden Sound mit eigener Lautstärke und Tonhöhe für jeden Tower-Typ ab.
    private void PlayTowerSound(TowerType type)
    {
        switch (type)
        {
            case TowerType.Arrow: _arrowSound?.Play(1.00f, 0.05f, 0f); break;
            case TowerType.Cannon: _cannonSound?.Play(0.52f, -0.18f, 0f); break;
            case TowerType.Crystal: _crystalSound?.Play(0.28f, 0.08f, 0f); break;
        }
    }

    // Bestätigt das Ende einer Runde mit einem kurzen, zurückhaltenden Klang.
    private void PlayEndSound(bool victory)
    {
        if (victory) _victorySound?.Play(0.55f, 0f, 0f);
        else _defeatSound?.Play(0.50f, -0.05f, 0f);
    }

    // Akustisches Feedback für die aufgeladene Falle auf dem Short Path.
    private void PlayRoyalTrapSound() => _royalTrapSound?.Play(0.48f, -0.08f, 0f);

}
