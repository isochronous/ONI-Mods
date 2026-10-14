# PLib with PDialog.Draggable

`PLib.dll` here is PLib 4.25.0 by Peter Han (MIT, see `LICENSE`) with one change: `PDialog.Draggable`,
which lets the planner's window be moved by its title bar and keeps it inside the screen. The change is
not in a released PLib yet. The mod builds against this copy by default and merges it into
`UpgradePlanner.dll`, as it does the NuGet package.

Source: [peterhaneve/ONIMods](https://github.com/peterhaneve/ONIMods) at `c08ef77`, plus commit
`4d9d8fc` "PDialog: Draggable, a dialog moved by its title bar and kept inside its parent"
(branch `draggable-dialog` of the local clone in `incubator/plib/ONIMods`). Built with:

```
dotnet build PLib/PLib.csproj -c Release -p:Platform=Vanilla -p:TargetFrameworks=netstandard2.1
```

The assembly version is the same as the released 4.25.0, so PLib's registry treats it as that release
when other mods carry PLib too; the draggable code lives in the dialog classes merged into this mod only.

To build with the NuGet PLib instead (the window then stays put): `-p:PLibDll= -p:PLibDraggable=false`.
