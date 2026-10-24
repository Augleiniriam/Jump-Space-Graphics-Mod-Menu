# Jump Space Graphics Menu

Menu MelonLoader en jeu, ouvrable par **F6** et fermable par **Échap**. Réglages : qualité globale, textures, ombres, distance des ombres, antialiasing, filtrage anisotrope, VSync, limite FPS, résolution de sortie et échelle dynamique.

## Compilation

MelonLoader est installé dans le dossier Steam, mais le jeu doit encore être lancé une fois pour générer `MelonLoader/Il2CppAssemblies`. Lance Jump Space, attends la fin du démarrage de MelonLoader, puis ferme le jeu.

Depuis le dossier `Mod Jump Space`, compile ensuite avec :

```powershell
dotnet build .\JumpSpaceGraphicsMenu\JumpSpaceGraphicsMenu.csproj -c Release
```

La DLL sera produite dans `Mod Jump Space\Mods\JumpSpaceGraphicsMenu.dll`. Copie-la ensuite dans `C:\Program Files (x86)\Steam\steamapps\common\JumpSpace\Mods`. Les réglages persistants sont dans `UserData/JumpSpaceGraphicsMenu.json`. L'échelle interne dépend du support de résolution dynamique du pipeline du jeu.
"# Jump-Space-Graphics-Mod-Menu" 
