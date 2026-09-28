# DesktopFinstatApiTester

Cross-platform desktop tester for the FinStat SK + CZ API clients. Fire any endpoint from a ribbon,
inspect the deserialized response as an object graph, and read back the curl command that would
reproduce the call.

Previously WPF + Fluent.Ribbon (Windows-only, `net10.0-windows`). Now **Avalonia 11** on `net8.0`,
with all chrome, design tokens and dialogs coming from the **Birko.Xaml** framework — so the same
code runs on Windows, Linux and macOS.

## Running it

```sh
dotnet run --project Tester/DesktopFinstatApiTester
```

Settings (API keys, station id/name, timeout, response type, endpoint URLs) live in the Settings
modal, which opens automatically until a public key is set. **Use** applies them for the session;
**Use and Save** also writes `application.json` next to the executable.

## Layout

| Path | What |
| --- | --- |
| `Program.cs`, `App.axaml(.cs)` | Avalonia entry point; merges the Birko theme, pins the Finstat variant, owns the `ApiApplication` singleton |
| `Views/MainWindow.axaml(.cs)` | The workspace — ribbon, request-history grid, response trees, status bar, settings modal, command palette |
| `Views/MainWindow_Ribbon.cs` | **The command surface as data** — one table of `RibbonTab`/`RibbonGroup`/`RibbonItem` that also feeds the Ctrl+K palette |
| `Views/MainWindow_API*.cs` | The request pipeline: parameter prompts, the call itself, curl/repeat, tree context actions |
| `Views/MainWindow_{SK,CZ}_*.cs` | One file per endpoint family. Each endpoint is a `…Request()` (parameter schema) plus the client call |
| `ViewModel/`, `Model/` | Settings/limits/response view-models and the persisted settings model |

## Birko.Xaml usage

Referenced via `$(BirkoSrc)` from `Directory.Build.props` (defaults to the sibling `Birko\Framework`
checkout, overridable with `/p:BirkoSrc=…` or `BIRKO_SRC`).

- **`Birko.Xaml.Core`** — `Ribbon.*` and `Commands.CommandItem` models, `Forms.FormField` (the
  settings schema), `Dialogs.IDialogService`, `Theming.IThemeManager`.
- **`Birko.Xaml.Avalonia`** — design tokens + restyled control themes, the `Ribbon`,
  `CommandPalette`, `Modal`, `ObjectTree` and schema-driven `Form` controls, `DialogService`,
  `AvaloniaThemeManager`.

The Data / Headers tabs are Birko `ObjectTree`s: the control walks the response graph itself (POCOs,
dictionaries, collections, `JsonNode`) and renders type-colored monospaced values. The Data tree's
context menu acts on the selected node via the control's `SelectedValue` / `SelectedPath`.

Only the **Finstat** theme is offered right now. `App.axaml` still merges `BirkoTheme.Core.axaml`
because light + dark are mandatory in any composition (`ThemeVariant.Dark` has no `InheritVariant`,
and Finstat inherits Light). To offer more, merge the extra `Themes/Tokens.<Theme>.axaml` sheets and
widen the `available:` list passed to `AvaloniaThemeManager` — the header switcher un-hides itself
once there is more than one.

## Notes from the port

- **No navigation shell.** This is a single fixed workspace, not a module-per-page app, so it uses
  the `Ribbon` control directly rather than `Birko.Xaml.Shell`'s `RibbonShellView`.
- **Dialogs are awaitable**, so the request pipeline is `async` throughout. Ribbon items hand their
  work to `FireAsync`, which routes any failure into the shared exception dialog.
- **WinForms file/folder pickers are gone** — replaced by Avalonia's `IStorageProvider`, which is
  what made the old project Windows-only.
- **Cross-thread notifications**: API calls run on the thread pool and write straight back into the
  view-models. Avalonia (unlike WPF) does not marshal those for you, so
  `ViewModel.RaisePropertyChanged` posts to the UI thread.
- Ribbon glyphs and group labels are kept in step with `Tester/WebFinstatApiTester`'s
  `src/shell/ribbon-tabs.ts`, so both testers name the same action the same way.
