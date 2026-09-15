# Universal Office Data Manager

A universal, fully offline Windows desktop app for building custom data
trackers — Attendance, Payments, Customers, Employees, Students, Bills,
Inventory, Expenses, Contacts, Tasks, or anything else — without touching
Excel formulas or a spreadsheet grid.

Everything is stored locally in a single SQLite file, `office_manager.db`.
There is no cloud sync, no Firebase/Supabase, no account, and no network
call anywhere in the app.

**This repository is being built in stages.** This is **Stage 1**.

## What Stage 1 includes

- A compiling, runnable WPF (.NET 10) desktop application
- A local SQLite database (`office_manager.db`), created automatically on
  first run, with indexed tables for workspaces, sheets, fields and records
- **Workspaces**: create, rename, delete — the top-level containers for
  your sheets
- **Sheets**: create, rename, delete inside a workspace
- **Dynamic fields** per sheet, with 12 working field types: Text, Number,
  Decimal, Currency, Percentage, Phone, Email, Address, Date, Time,
  Dropdown, Yes/No — including required-field and dropdown-option config
- **Records**: add, edit, delete records through a form generated from
  your sheet's fields, with basic validation (required fields, numeric
  fields, email format)
- A **dashboard** with live counts (workspaces / sheets / records) and a
  recently-updated-sheets list
- A read-only data grid per sheet with columns generated from your fields
- Sidebar navigation with right-click menus for workspaces and sheets
- A clean, modern light/dark two-tone UI (dark sidebar, light content)

Every button and menu in this stage does exactly what it says — nothing
is a stub.

## What is *not* in Stage 1 yet

These are real, explicitly planned follow-on stages, not abandoned scope:

- Table inline editing, card/kanban view, saved views, grouping, column
  reordering/hiding, conditional formatting
- Search, advanced filters, bulk edit, duplicate detection, undo/redo
- Formula/rules engine (SUM, AVERAGE, IF, date math, auto totals, etc.),
  Formula/Calculated/Lookup/Linked-Record field types
- More field types: Multi-select, Status, Priority, Checkbox (as distinct
  from Yes/No), Image, PDF, File, Signature, Barcode, QR, Auto-ID
- Ready-made templates (Attendance, Payment Tracker, Salary, Fees, Sales,
  Purchases, Invoices, ...) and user-saved custom templates
- Excel (.xlsx) / CSV import & export with column mapping and validation
  reports
- PDF reports, invoices, printing, chart-based dashboards
- Attachments, document/barcode/QR scanning, reminders, audit history,
  PIN/biometric lock
- `.ombackup` full-state backup/restore, automatic local backups, backup
  validation, pre-restore safety backup
- Automated tests (database, records, formulas, import/export, backup)
- A signed installer / single-file release build

## Tech stack

- **.NET 10** (current LTS), **C# / WPF**, hand-rolled MVVM (no external
  MVVM framework — `ObservableObject` + `AsyncRelayCommand` live in
  `ViewModels/Common`)
- **Microsoft.Data.Sqlite** — the only third-party package in Stage 1
- Plain ADO.NET repositories (no ORM) for predictable SQL and easy
  extension as the schema grows in later stages

## Project layout

```
src/UniversalOfficeDataManager/
  Models/         Workspace, Sheet, FieldDefinition, FieldType, RecordItem
  Data/           SQLite connection/path helpers, schema creation
  Repositories/   One repository per entity, plain ADO.NET + SQL
  Services/       IDialogService — keeps ViewModels free of direct
                   Window/MessageBox calls
  ViewModels/     MainViewModel (navigation/sidebar), DashboardViewModel,
                   SheetDetailViewModel, + small node/row view-models
  Views/          MainWindow + the dashboard/sheet/dialog XAML views
  Styles/         Colors.xaml, Styles.xaml — shared look and feel
  Converters/     Small value converters used by the XAML
```

## Where your data lives

`%LocalAppData%\UniversalOfficeDataManager\office_manager.db`

Nothing is written anywhere else, and nothing leaves this folder.

## Building

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
and Windows (WPF only runs on Windows).

```powershell
dotnet restore UniversalOfficeDataManager.sln
dotnet build UniversalOfficeDataManager.sln --configuration Release
dotnet run --project src/UniversalOfficeDataManager/UniversalOfficeDataManager.csproj
```

`.github/workflows/build.yml` runs the same restore/build/publish on
`windows-latest` for every push, so you can confirm it compiles without
owning a Windows machine yourself — check the **Actions** tab after
pushing. If a build fails there, paste the error back and it'll get
fixed before the next stage is added.

## Roadmap

1. ✅ **Stage 1** — project skeleton, SQLite, dashboard, workspaces,
   sheets, dynamic fields, basic records (this stage)
2. Table/card views, search, filter, sort, group, saved views, bulk edit
3. Formula/rules engine + calculated & lookup/linked-record fields
4. Templates (built-in + user-saved)
5. Excel/CSV import & export, PDF reports/invoices/printing, chart
   dashboards
6. Attachments, scanning, reminders, audit history, PIN/biometric lock
7. `.ombackup` backup/restore, automatic backups, validation
8. Automated test suite
9. Signed installer / single-file release packaging
