using System;
using System.IO;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(JumpSpaceGraphicsMenu.GraphicsMenuMod), "Jump Space Graphics Menu", "1.0.0", "Codex")]

namespace JumpSpaceGraphicsMenu;

public sealed class GraphicsMenuMod : MelonMod
{
    private static readonly string ConfigFile = Path.Combine(MelonEnvironment.UserDataDirectory, "JumpSpaceGraphicsMenu.json");
    private readonly Settings _cfg = new();
    private bool _open;
    private CursorLockMode _oldLock;
    private bool _oldCursor;
    private Rect _window = new(40, 40, 440, 610);
    private string _message = "F6 : ouvrir ou fermer";

    public override void OnInitializeMelon() { Load(); Apply(); LoggerInstance.Msg("Menu graphique prêt — F6"); }
    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.F6)) Toggle();
        if (_open && Input.GetKeyDown(KeyCode.Escape)) Toggle();
    }
    public override void OnGUI() { if (_open) _window = GUI.Window(67124, _window, Draw, "JUMP SPACE | GRAPHISMES"); }
    public override void OnApplicationQuit() { Save(); if (_open) Toggle(); }

    private void Draw(int id)
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Résolution actuelle : " + Screen.width + " × " + Screen.height);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Résolution −")) ChangeResolution(-1);
        if (GUILayout.Button("Résolution +")) ChangeResolution(1);
        GUILayout.EndHorizontal();
        GUILayout.Label("Résolution de sortie, distincte du rendu interne.");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("VSync : " + (_cfg.VSync ? "ON" : "OFF"))) { _cfg.VSync = !_cfg.VSync; Apply(); }
        if (GUILayout.Button("Limite FPS : " + (_cfg.Fps == 0 ? "illimitée" : _cfg.Fps.ToString())))
        { int[] a = { 30, 60, 90, 120, 144, 0 }; _cfg.Fps = a[(Array.IndexOf(a, _cfg.Fps) + 1) % a.Length]; Apply(); }
        GUILayout.EndHorizontal();
        GUILayout.Space(8); GUILayout.Label("QUALITÉ");
        Cycle("Préréglage global", QualitySettings.names, _cfg.Quality, v => { _cfg.Quality = v; QualitySettings.SetQualityLevel(v, true); Save(); });
        Cycle("Textures", new[] { "Maximale", "Élevée", "Moyenne", "Basse" }, _cfg.Textures, v => { _cfg.Textures = v; QualitySettings.masterTextureLimit = 3 - v; Save(); });
        Cycle("Ombres", new[] { "Désactivées", "Hard Only", "Tous types" }, _cfg.Shadows, v => { _cfg.Shadows = v; QualitySettings.shadows = (ShadowQuality)v; Save(); });
        Cycle("Antialiasing", new[] { "Désactivé", "2×", "4×", "8×" }, _cfg.Antialiasing, v => { _cfg.Antialiasing = v; QualitySettings.antiAliasing = v == 0 ? 0 : 1 << v; Save(); });
        Cycle("Filtrage anisotrope", new[] { "Désactivé", "Par texture", "Forcé" }, _cfg.Anisotropy, v => { _cfg.Anisotropy = v; QualitySettings.anisotropicFiltering = (AnisotropicFiltering)v; Save(); });
        GUILayout.Space(6); GUILayout.Label("Distance des ombres : " + _cfg.Shadow.ToString("0") + " m");
        _cfg.Shadow = GUILayout.HorizontalSlider(_cfg.Shadow, 0f, 200f); QualitySettings.shadowDistance = _cfg.Shadow;
        GUILayout.Space(6); GUILayout.Label("Échelle interne : " + _cfg.Scale.ToString("0.00") + "×");
        GUILayout.Label("Fonctionne si le jeu active la résolution dynamique.");
        _cfg.Scale = (float)Math.Round(GUILayout.HorizontalSlider(_cfg.Scale, 0.5f, 1.5f), 2);
        ScalableBufferManager.ResizeBuffers(_cfg.Scale, _cfg.Scale);
        GUILayout.Space(8); GUILayout.BeginHorizontal();
        if (GUILayout.Button("Profil performance")) Performance();
        if (GUILayout.Button("Valeurs par défaut")) { _cfg.Reset(); Apply(); }
        if (GUILayout.Button("Sauvegarder")) Save();
        GUILayout.EndHorizontal(); GUILayout.Label(_message); GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 24));
    }

    private static void Cycle(string name, string[] options, int value, Action<int> set)
    {
        if (options.Length == 0) return;
        GUILayout.BeginHorizontal(); GUILayout.Label(name, GUILayout.Width(165));
        if (GUILayout.Button("◀", GUILayout.Width(38))) { value = (value - 1 + options.Length) % options.Length; set(value); }
        GUILayout.Label(options[Mathf.Clamp(value, 0, options.Length - 1)], GUILayout.Width(145));
        if (GUILayout.Button("▶", GUILayout.Width(38))) { value = (value + 1) % options.Length; set(value); }
        GUILayout.EndHorizontal();
    }
    private static int AaIndex() => QualitySettings.antiAliasing switch { 2 => 1, 4 => 2, 8 => 3, _ => 0 };

    private void ChangeResolution(int direction)
    {
        Resolution[] modes = Screen.resolutions;
        if (modes == null || modes.Length == 0) { _message = "Modes de résolution indisponibles."; return; }
        int index = 0; long best = long.MaxValue;
        for (int i = 0; i < modes.Length; i++)
        {
            long d = Math.Abs(modes[i].width - Screen.width) + Math.Abs(modes[i].height - Screen.height);
            if (d < best) { best = d; index = i; }
        }
        index = Mathf.Clamp(index + direction, 0, modes.Length - 1);
        Screen.SetResolution(modes[index].width, modes[index].height, Screen.fullScreen);
        _message = "Résolution : " + modes[index].width + " × " + modes[index].height;
    }

    private void Performance()
    {
        _cfg.VSync = false; _cfg.Fps = 120; _cfg.Shadow = 35f; _cfg.Scale = 0.75f; _cfg.Quality = 0; _cfg.Textures = 2; _cfg.Shadows = 1; _cfg.Antialiasing = 0; _cfg.Anisotropy = 0;
        QualitySettings.SetQualityLevel(0, true); QualitySettings.masterTextureLimit = 1;
        QualitySettings.shadows = ShadowQuality.HardOnly; QualitySettings.antiAliasing = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        Apply(); _message = "Profil performance appliqué."; Save();
    }
    private void Apply()
    {
        QualitySettings.vSyncCount = _cfg.VSync ? 1 : 0;
        Application.targetFrameRate = _cfg.VSync ? -1 : _cfg.Fps;
        QualitySettings.shadowDistance = _cfg.Shadow;
        int maxQuality = Math.Max(0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(Mathf.Clamp(_cfg.Quality, 0, maxQuality), true);
        QualitySettings.masterTextureLimit = 3 - Mathf.Clamp(_cfg.Textures, 0, 3);
        QualitySettings.shadows = (ShadowQuality)Mathf.Clamp(_cfg.Shadows, 0, 2);
        QualitySettings.antiAliasing = _cfg.Antialiasing == 0 ? 0 : 1 << Mathf.Clamp(_cfg.Antialiasing, 1, 3);
        QualitySettings.anisotropicFiltering = (AnisotropicFiltering)Mathf.Clamp(_cfg.Anisotropy, 0, 2);
        ScalableBufferManager.ResizeBuffers(_cfg.Scale, _cfg.Scale);
    }
    private void Toggle()
    {
        _open = !_open;
        if (_open) { _oldLock = Cursor.lockState; _oldCursor = Cursor.visible; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        else { Cursor.lockState = _oldLock; Cursor.visible = _oldCursor; }
    }
    private void Load()
    {
        try { if (File.Exists(ConfigFile)) { Settings? s = System.Text.Json.JsonSerializer.Deserialize<Settings>(File.ReadAllText(ConfigFile)); if (s != null) _cfg.Copy(s); } }
        catch (Exception e) { LoggerInstance.Warning("Config non lu : " + e.Message); }
    }
    private void Save()
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!); File.WriteAllText(ConfigFile, System.Text.Json.JsonSerializer.Serialize(_cfg, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })); _message = "Réglages sauvegardés."; }
        catch (Exception e) { _message = "Erreur de sauvegarde : " + e.Message; }
    }
    private sealed class Settings
    {
        public bool VSync { get; set; } = true; public int Fps { get; set; } = 120;
        public float Shadow { get; set; } = 100f; public float Scale { get; set; } = 1f;
        public int Quality { get; set; } = 2; public int Textures { get; set; } = 0; public int Shadows { get; set; } = 2;
        public int Antialiasing { get; set; } = 0; public int Anisotropy { get; set; } = 1;
        public void Reset() { VSync = true; Fps = 120; Shadow = 100f; Scale = 1f; Quality = 2; Textures = 0; Shadows = 2; Antialiasing = 0; Anisotropy = 1; }
        public void Copy(Settings s) { VSync = s.VSync; Fps = Math.Clamp(s.Fps, 0, 360); Shadow = Math.Clamp(s.Shadow, 0f, 200f); Scale = Math.Clamp(s.Scale, 0.5f, 1.5f); Quality = Math.Clamp(s.Quality, 0, 10); Textures = Math.Clamp(s.Textures, 0, 3); Shadows = Math.Clamp(s.Shadows, 0, 2); Antialiasing = Math.Clamp(s.Antialiasing, 0, 3); Anisotropy = Math.Clamp(s.Anisotropy, 0, 2); }
    }
}


