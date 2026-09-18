# BUILD COMMAND — Project Inventory & Management Portal (Frontend Only)

## ROLE & CONSTRAINTS (read first)
You are a frontend-only build agent. Hard restrictions:
- Architecture: **ASP.NET Core MVC** (Controllers + Razor Views + ViewModels) — no real database, no EF, no external API calls. However, **nothing in the Views may be hardcoded/static HTML for repeating content.** Every list, table, card grid, chip group, avatar group, and stat block MUST be rendered dynamically:
  - The Controller builds strongly-typed `ViewModel` objects (e.g. `List<ProjectViewModel>`, `ProjectDetailViewModel`) from a small in-memory mock **data source class** (e.g. `Services/MockProjectDataService.cs` or a JSON file loaded at startup into a `List<T>` via `System.Text.Json`) — never by typing individual `<tr>`/`<div>` blocks per record in the `.cshtml`.
  - Views use **Razor `@foreach` loops** over the model collections to generate rows, cards, chips, avatars, pagination numbers, stat cards, etc. Adding/removing/editing an item in the mock data source must automatically change what renders — no view file should need to be touched.
  - Numbers shown anywhere (totals like `52 Total Projects`, `38 In Production`, `Showing 1–5 of 52`, pagination page count, etc.) must be **computed from the actual mock collection's length/counts** (e.g. `Model.Projects.Count(p => p.Status == "Production")`), not typed in literally.
  - On the client, JS must also treat the table/pagination/filters as **data-driven**: keep the row data as a JS array/object (e.g. rendered into a `<script type="application/json">` block or `data-*` attributes from Razor), and have JS re-render/filter/paginate/slice that array — not toggle the visibility of pre-written duplicate HTML blocks.
  - "Mock" only means the values are fabricated/sample values (since there's no real backend) — the *rendering mechanism* itself must be fully dynamic and driven by that data, exactly as a real MVC app would consume a real dataset.
- Client code: **vanilla HTML, CSS, and JavaScript only.** No Bootstrap, no Tailwind, no jQuery, no React/Vue/Angular, no npm build step, no external CSS/JS frameworks. You may use plain `<link>`/`<script>` tags to your own files only.
- Output must be a working ASP.NET Core MVC project skeleton (`.csproj`, `Program.cs`, `Controllers/`, `Models/`, `Services/`, `Views/`, `wwwroot/css`, `wwwroot/js`, `wwwroot/images`) that runs and renders the two screens described below, pixel-close to the reference mockup, entirely from the dynamic data pipeline above.
- Do not add authentication, routing beyond what's needed for these two views, or any real server-side persistence — but do not hardcode repeating UI content either. If in doubt: **one Razor loop / one JS render function per repeating UI block, zero copy-pasted markup per item.**

## PROJECT STRUCTURE TO CREATE
```
/Controllers
    HomeController.cs        -> Index() returns Dashboard view; Details(string id) returns ProjectDetail view
/Models
    ProjectViewModel.cs
    ProjectDetailViewModel.cs
    DashboardViewModel.cs
/Views
    /Home
        Index.cshtml          -> Dashboard screen
        Details.cshtml        -> Project Detail screen
    /Shared
        _Layout.cshtml        -> global header + sidebar shell
        _StatCard.cshtml (partial, optional)
/wwwroot
    /css
        site.css              -> shared tokens (colors, spacing, typography)
        dashboard.css
        project-detail.css
    /js
        site.js               -> sidebar nav, dropdown, notification bell
        dashboard.js           -> filters, tag removal, pagination, table row popover, page-size select
        project-detail.js      -> tab switching, role toggle
    /images
        mountain-banner.jpg (or SVG illustration placeholder)
```

## GLOBAL HEADER (appears above both screens, full width)
- Left: cube/hexagon-stack logo icon (blue, layered-box style) + Title `Project Inventory & Management Portal` (bold, dark navy, ~20px) + subtitle line `People • Applications • Infrastructure • A More Connected Organization` (gray, small, bullet-separated).
- Right: two right-aligned text blocks separated by a thin vertical divider line:
  - Block 1: `One Platform` / `Complete Visibility` / `Greater Impact` (3 stacked lines, small gray/navy text)
  - Block 2: `Track   Manage   Collaborate` / `Build a Smarter Tomorrow` (with an underline accent below)
- Background: very light blue-gray (`#EEF3F9`-ish). This bar is a marketing/masthead strip, distinct from the app's internal top bar.

## LEFT SIDEBAR (dark navy, ~180px wide, fixed height, present on both screens)
- Background: dark navy (`#131C33` / `#151F38`).
- Top: small logo icon.
- Nav list (icon + label), vertical stack:
  - Home (active state: blue rounded-rect highlight, white text/icon)
  - Projects (grid icon)
  - Infrastructure (server icon)
  - Reports (bar-chart icon)
  - Users (people icon)
  - Settings (gear icon)
- All inactive items: muted gray-blue icon/text; hover state = subtle lighter background.
- Bottom of sidebar: a small muted tagline, one word per line: `Better` / `Applications` / `Stronger` / `Public Services`.

## SCREEN 1 — DASHBOARD (`/Home/Index`)
### Top app bar (inside content area, white background)
- Page title `Project Inventory & Management Portal` (smaller, left).
- Center-right: search input with magnifier icon, placeholder `Search projects, owners, servers, technologies...`.
- Right: bell icon with red badge `3`; user block = circular avatar `MA` (navy circle, white initials) + name `Mehmet Arslan` + role `IT Manager` (gray, smaller) + dropdown chevron.

### Welcome banner (rounded card, blue gradient background, mountain photo/illustration on the right, ~140px tall)
- Heading `Welcome back, Mehmet!` (white, bold, large).
- Subtext `Track projects, manage resources, and keep your systems running smoothly.` (white, muted).
- Right-side caption: `A more efficient digital tomorrow` with a short underline accent.
- A mountain landscape image/illustration bleeds into the right portion of the card.

### Stat cards row (4 white rounded cards, sit overlapping the bottom of the banner, in a row)
Each card = icon in colored rounded square + big number + label:
1. Blue cube icon → **52** / `Total Projects`
2. Green circle-check icon → **38** / `In Production`
3. Orange gear icon → **7** / `In Development`
4. Purple flask icon → **4** / `Staging / Test`

### Section header
- `Projects` (bold, large) with subtitle `A centralized view of all applications and infrastructure`.
- Right-aligned button group: `Export ⌄` (outline button with dropdown caret), `Excel` (green filled button with sheet icon), `PDF` (red/pink outline button with file icon).

### Filter bar (single row, white card, rounded)
- Text input with search icon: `Search in projects...`
- Dropdown: `All Service Domains`
- Dropdown: `All Departments`
- Dropdown: `All Status`
- Button with funnel icon: `More Filters`

### Active filter chips row
- Chip `Criticality = Normal` with an `×` remove icon (light blue pill).
- Chip `Status = Production` with `×`.
- Text link `Clear All` (blue).
- Right-aligned: `Showing 1–5 of 52 projects` + pagination controls (‹ 1 2 3 4 5 ›), current page `1` highlighted blue.

### Data table (white card, rounded corners, header row light gray)
Columns: `Project Code / ID` | `Project Name` | `Service Domain` | `Department` | `Assigned Developer(s)` | `Server / IP` | `Status` (sortable, shows ↕ icon) | `Actions`.

Render every row via a **Razor `@foreach (var project in Model.Projects)` loop** against a `List<ProjectViewModel>` supplied by the Controller from the mock data service — do not write individual `<tr>` markup per project. Seed the mock data service with at least these 5 sample records (feel free to add the rest up to `52` total in the service so `Total Projects` / pagination math is genuine, not just a label):

| Code | Name | Domain | Department | Devs | Server/IP | Status |
|---|---|---|---|---|---|---|
| PRJ-0012 | eGov Citizen Portal | Citizen Services | Digital Transformation | AY, EY +2 | TR-APP-01 / 10.10.1.15 (+2 more) | Production (green pill) |
| PRJ-0023 | Finance Management System | Finance | Financial Affairs | (avatars, no initials shown) | FIN-APP-01 / 10.20.5.10 | Staging / Test (amber pill) |
| PRJ-0031 | HR Portal | Human Resources | Human Resources | ZK, MT +1 | HR-APP-01 / 10.30.2.8 (+1 more) | In Development (blue pill) |
| PRJ-0045 | Document Management System | Corporate Services | IT Operations | DA, SK +1 | DOC-APP-01 / 10.40.1.20 | Production (green pill) |
| PRJ-0058 | Analytics Platform | Data & Analytics | Strategy and Planning | YK, ST | ANA-APP-01 / 10.50.3.14 (+2 more) | Unknown (red pill) |

- `Assigned Developer(s)` cell = overlapping circular avatar chips + `+N` overflow chip. On hover/click of the overflow area, show a small dark popover card titled `Sub-Units` with a bulleted list (e.g. `Application Development`, `IT Operations`, `Business Solutions`, `PMO`) — implement with vanilla JS (toggle a positioned `<div>` on click, close on outside click).
- `Actions` cell = `Details` link/button + a `…` kebab menu button.
- Footer row: `Show [5 ▾] per page` dropdown (left) and pagination repeated (right).

## SCREEN 2 — PROJECT DETAIL (`/Home/Details/PRJ-0012`)
### Top bar
- `← Back to Projects` link (left).
- Right: two toggle/pill buttons `👤 Manager` (active/filled green) and `💻 Developer` (outline blue) — vanilla JS toggles the active class between them — plus the same avatar block `MA`.

### Title block
- `eGov Citizen Portal` (large bold) + status pill `● Production` (green).
- Right-aligned: `Edit` button (pencil icon, outline) and `…` kebab menu.
- Sub-line: `PRJ-0012    HEYS-2024-0012` (gray, monospace-ish codes).
- Description line: `A central platform for citizens to access government services online.`

### Tabs (horizontal, underline style)
`Overview` (active, blue underline) | `Infrastructure` | `Team` | `Documents` | `Activity Log`
Implement tab switching in vanilla JS (show/hide corresponding content panels; only build out the `Overview` panel content, other tabs can be empty placeholders).

### Overview tab — 2×2 grid of white rounded cards
Each card has a colored icon + colored title, and a label/value list (label gray small, value dark, right-aligned or two-column):

**1. General & Organizational Information** (blue icon, briefcase/id icon)
- Application / Project Name: `eGov Citizen Portal`
- Project ID / Application Code: `PRJ-0012`
- HEYS Code: `HEYS-2024-0012`
- Status: `Production` (green pill)
- Service Domain: `Citizen Services`
- Technical Unit: `Digital Transformation`
- Software Dev Branch Duty / Role: `Application Development`

**2. Server, Network & Deployment** (teal icon, server icon)
- Access URL: `https://portal.gov.tr` (external-link icon, styled as link)
- Server Name(s): chips `TR-APP-01` `TR-APP-02` `TR-DB-01`
- Server IP Address(es): chips `10.10.1.15` `10.10.1.16` `10.10.1.17`
- Source Code Server: `https://git.gov.tr` (link)
- Bitbucket Repository: `https://bitbucket.org/egov` (link)

**3. Technology Stack & Integrations** (purple icon, layers icon)
- Backend Technology: `.NET 8, C#`
- Frontend Technology: `React, TypeScript, JavaScript`
- Database: `PostgreSQL`
- E-Signature Support: `Yes` (green pill)
- Integrations: chips `e-Devlet` `MERNIS` `Payment Gateway` `+2 more`

**4. Security, Metrics & Vendor Operations** (orange icon, shield icon)
- Criticality Level: `Normal` (blue-gray pill)
- Processes Critical Data: `Yes` (green pill)
- Critical Data Type: chips `PII / KVKK` `Financial`, plus a red pill `Confidential`
- Active User Metrics: two stat sub-items with icons — `Internal User Count 12,430`, `External User Count 56,210`
- Vendor / Owner: `TechSolutions A.Ş.`
- Active Vendor Support: `Yes` (green pill)

## DESIGN TOKENS (put in `site.css` as CSS variables)
```css
:root{
  --navy-bg:#131C33;
  --page-bg:#F3F6FA;
  --card-bg:#FFFFFF;
  --accent-blue:#2F6FED;
  --accent-blue-light:#EAF1FE;
  --text-dark:#1B2436;
  --text-gray:#6B7280;
  --border:#E5E9F0;
  --green:#16A34A; --green-bg:#E9F9EF;
  --amber:#D97706; --amber-bg:#FFF4E0;
  --red:#DC2626; --red-bg:#FDECEC;
  --purple:#7C3AED; --purple-bg:#F1EBFF;
  --teal:#0D9488; --teal-bg:#E3F7F4;
  --radius-card:14px; --radius-pill:999px; --radius-btn:8px;
  --shadow-card:0 1px 3px rgba(16,24,40,0.06), 0 1px 2px rgba(16,24,40,0.04);
  --font: -apple-system, "Segoe UI", Roboto, Arial, sans-serif;
}
```
Use flexbox/grid only (no framework grid classes). Status pills, chips, and buttons should be small reusable CSS classes (`.badge-success`, `.badge-warning`, `.badge-danger`, `.badge-info`, `.chip`, `.btn`, `.btn-outline`).

## JS BEHAVIOR TO IMPLEMENT (vanilla only, no libraries, all data-driven)
1. Sidebar active-link highlighting on click.
2. User menu / notification bell dropdown toggle (open on click, close on outside click).
3. Dashboard, all driven off a JS array of project objects (hydrated from the Razor model, e.g. via a `<script type="application/json" id="projects-data">@Html.Raw(Json.Serialize(Model.Projects))</script>` block that JS parses on load — never a hand-typed JS array):
   - Filter dropdowns filter the in-memory array by service domain / department / status and re-render the visible rows + chip list + "Showing X of Y" count from the filtered result's real length.
   - Removable filter chips: clicking `×` removes that filter condition from the active-filter state, re-applies the filter function, and re-renders both the chip row and the table body.
   - Pagination buttons slice the (filtered) array by page and re-render the table body + active page indicator; page count is computed as `Math.ceil(filteredData.length / pageSize)`.
   - "Show N per page" select changes `pageSize` and re-renders from the same array/slice logic.
   - Developer-avatar overflow (`+N`) opens a "Sub-Units" popover built from that project's own sub-unit array (from the data object), positioned near the click target — not a static popover reused for every row.
4. Project Detail: tab click swaps visible content panel (panels populated from the `ProjectDetailViewModel`, not separate hardcoded copies); Manager/Developer toggle swaps the active pill style.
5. All interactivity must be done with plain `document.querySelector`/event listeners and small render functions (e.g. `renderTable(data)`, `renderChips(filters)`, `renderPagination(page, total)`) — no external state libraries, and no duplicated markup blocks toggled via CSS as a substitute for real re-rendering.

## DELIVERABLE
A runnable ASP.NET Core MVC solution matching the structure above, with the two Razor views styled per this spec, using vanilla CSS/JS. All values (numbers, rows, chips, cards) must come from mock **data objects/collections** flowing through Controller → ViewModel → Razor loop → (optionally) JSON → JS render function. No screen element that repeats (table rows, chips, avatars, stat cards, detail-card fields) may be individually hand-authored as static markup. Do not add any features not described here (no auth, no real navigation targets for Infrastructure/Reports/Users/Settings — they can be placeholder links).
