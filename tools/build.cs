// Regenerates assets/*.svg and the activity table in README.md from live GitHub data.
//
//     dotnet run tools/build.cs             fetch fresh data, fall back to the cache on failure
//     dotnet run tools/build.cs --offline   use tools/live-cache.json only
//
// GitHub strips scripts and <style> from READMEs, but an SVG loaded as an image keeps its own CSS.
// So everything that moves on the profile is CSS keyframes inside these files, and everything that
// looks live is refreshed by .github/workflows/refresh.yml.

#:property InvariantGlobalization=true
#:property PublishAot=false

using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

var tools = Path.GetDirectoryName(SourcePath())!;
var root = Path.GetFullPath(Path.Combine(tools, ".."));
var assets = Path.Combine(root, "assets");
Directory.CreateDirectory(assets);

var live = await Live.Load(tools, args.Contains("--offline"));
var fontCss = Fonts.Css(tools);
var glyphs = File.ReadAllLines(Path.Combine(tools, "fonts", "name-paths.txt"));

Save("hero", Hero());
Save("terminal", Terminal());
Save("uams", Uams());
Save("admissions", Admissions());
Save("pingpong", PingPong());
Save("desktop", Desktop());
Save("tasktracker", TaskTracker());
Save("restaurant", Restaurant());
Save("stack", Stack());
Save("patterns", Patterns());
Save("activity", Activity());
UpdateReadme(Path.Combine(root, "README.md"));

Console.WriteLine($"Built assets: {live.Total} contributions, latest push {live.Latest?.Name}");

void Save(string name, Svg svg)
{
    File.WriteAllText(Path.Combine(assets, $"{name}.svg"), svg.Render(fontCss));
}

// ───────────────────────────────── Hero ─────────────────────────────────

Svg Hero()
{
    const double T = 8, Intro = 1.7, SparkT = 16;
    var s = new Svg(804, 290, "Seif Hesham, backend developer building .NET systems");
    s.Def("""<pattern id="g" width="16" height="16" patternUnits="userSpaceOnUse"><circle cx="1" cy="1" r=".8" class="dt"/></pattern><linearGradient id="f" x2="0" y2="1"><stop offset="0" stop-color="#fff"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></linearGradient><mask id="m"><rect width="804" height="210" fill="url(#f)"/></mask>""");
    s.Raw("""<rect width="804" height="210" fill="url(#g)" mask="url(#m)"/>""");

    s.Css("""
        @keyframes draw{0%{stroke-dashoffset:1000;fill-opacity:0}62%{stroke-dashoffset:0;fill-opacity:0}100%{stroke-dashoffset:0;fill-opacity:1}}
        .gf{fill:var(--ink);stroke:var(--ink);stroke-width:.8;stroke-dasharray:1000;animation:draw 1.5s ease-out both}
        .gs{fill:none;stroke:var(--sig);stroke-width:2.2;stroke-linecap:round;stroke-dasharray:70 930;opacity:0}
        @keyframes grow{from{stroke-dashoffset:760}to{stroke-dashoffset:0}}
        .gw{stroke-dasharray:760;animation:grow 1.1s .7s ease-out both}
        """);

    // The name: outlines trace in once, then a spark walks around one letter at a time, forever.
    s.Raw("""<g transform="translate(22 142)">""");
    for (var i = 0; i < glyphs.Length; i++)
        s.Raw($"""<path class="gf" pathLength="1000" d="{glyphs[i]}" style="animation-delay:{N(i * .055)}s"/>""");
    for (var i = 0; i < glyphs.Length; i++)
    {
        var t = i * 1.3;
        var style = s.Frames(SparkT,
        [
            (0, "opacity:0;stroke-dashoffset:0"), (t, "opacity:0;stroke-dashoffset:0"),
            (t + .01, "opacity:1;stroke-dashoffset:0"), (t + 1.6, "opacity:1;stroke-dashoffset:-1000"),
            (t + 1.61, "opacity:0;stroke-dashoffset:-1000"), (SparkT, "opacity:0;stroke-dashoffset:-1000"),
        ], Intro + .6);
        s.Raw($"""<path class="gs pk0" pathLength="1000" d="{glyphs[i]}" style="{style}"/>""");
    }
    s.Raw("</g>");

    // Wide layout: a request walking the pipeline these projects actually use, and its response.
    s.Group("w", () =>
    {
        s.T(22, 24, "github.com/seifhesham22", "", 12);
        if (live.Latest is { } latest)
        {
            var text = $"last push: {Repos.Short(latest.Name)}, {Day(latest.PushedAt)}";
            s.T(782, 24, text, "e", 12);
            PulseDot(s, 782 - text.Length * 7.03 - 12, 20, 3.5);
        }
        s.T(22, 186, "Backend developer building .NET systems that stay legible as they grow.", "d", 22, "fill:var(--mut)");

        (double x, double y)[] forward = [(22, 240), (782, 240)];
        (double x, double y)[] back = [(782, 258), (22, 258)];
        s.Raw("""<path class="wire gw" d="M22 240H782"/><path class="wire gw" d="M782 258H22"/>""");

        string[] stops = ["request", "endpoint", "MediatR", "handler", "domain", "EF Core", "PostgreSQL"];
        var at = s.Packet(forward, T, .2, 3.6, delay: Intro);
        var ret = s.Packet(back, T, 4, 6.4, tone: "ok", delay: Intro);
        for (var i = 0; i < stops.Length; i++)
        {
            var x = 22 + i * (760.0 / (stops.Length - 1));
            var anchor = i == 0 ? "" : i == stops.Length - 1 ? "e" : "a";
            var when = at[0] + (at[1] - at[0]) * i / (stops.Length - 1);
            s.T(x, 228, stops[i], anchor, 12);
            s.Dot(x, 240, 3.5, "nd");
            var flash = s.Flash(T, [when], hold: .3, fade: .7, delay: Intro);
            s.T(x, 228, stops[i], $"{anchor} ink hl", 12, flash);
            s.Dot(x, 240, 5, "sg hl", flash);
        }

        var ok = s.Flash(T, [ret[1]], hold: .7, fade: .5, delay: Intro);
        s.Dot(22, 258, 3.5, "nd");
        s.Dot(782, 258, 3.5, "nd");
        s.T(22, 278, "200 OK", "gn", 12, ok + ";opacity:0");
        s.Dot(22, 258, 5, "gnf hl", ok);
    });

    // Narrow layout: the image is under 560px wide, so the same viewBox gets bigger type and no labels.
    s.Group("n", () =>
    {
        s.T(22, 188, "Backend developer building .NET", "d", 31, "fill:var(--mut)");
        s.T(22, 228, "systems that stay legible as they grow.", "d", 31, "fill:var(--mut)");
        s.Raw("""<path class="wire" d="M22 266H782" style="stroke-width:2"/>""");
        s.Packet([(22, 266), (782, 266)], T, .2, 3.6, delay: Intro, width: 5);
        s.Packet([(782, 266), (22, 266)], T, 4, 6.4, tone: "ok", delay: Intro, width: 5);
    });
    return s;
}

// ──────────────────────────────── Terminal ───────────────────────────────

Svg Terminal()
{
    const double T = 24, Hold = 22.4;
    var s = new Svg(804, 306, "A dotnet watch log listing what Seif is working on and with");
    var stackLine = "ASP.NET Core, EF Core, PostgreSQL, SignalR, RabbitMQ";
    var repoCount = live.Repos.Count;
    var pushed = live.Latest is { } l ? $"{Repos.Short(l.Name)}, {Day(l.PushedAt)}" : "n/a";

    s.Raw("""<rect x=".5" y=".5" width="803" height="305" rx="12" class="fr"/>""");
    s.Raw("""<path d="M.5 35V12.5a12 12 0 0 1 12-12h779a12 12 0 0 1 12 12V35z" class="st"/><path class="ln" d="M.5 35H803.5"/>""");
    s.Raw("""<path d="M10 35V16a6 6 0 0 1 6-6h156a6 6 0 0 1 6 6V35z" class="tb"/>""");
    s.T(22, 27, ">_", "sg", 12);
    s.T(44, 27, "dotnet watch", "ink", 12);
    s.Raw("""<path class="ctl" d="M722 18h11M758 13h9v9h-9zM792 13l9 9m0-9-9 9" transform="translate(-14 0)"/>""");

    // (text, tone, appears at). A real terminal prints green "info:" and plain text, so this does too.
    s.Group("w", () =>
    {
        (string tag, string text, string cls, double t)[] lines =
        [
            ("", "dotnet watch : Started", "", 1.8),
            ("info", "Microsoft.Hosting.Lifetime[14]", "", 2.6),
            ("", "      Now listening on: https://github.com/seifhesham22", "ink", 2.6),
            ("info", "Seif.Now[0]", "", 3.4),
            ("", $"      Last push: {pushed}", "ink", 3.4),
            ("info", "Seif.Stack[0]", "", 4.2),
            ("", $"      {stackLine}", "ink", 4.2),
            ("info", "Seif.Stats[0]", "", 5),
            ("", $"      {live.Total} contributions in the last year, {repoCount} public projects", "ink", 5),
            ("info", "Microsoft.Hosting.Lifetime[0]", "", 5.8),
            ("", "      Application started. Press Ctrl+C to shut down.", "ink", 5.8),
            ("", "dotnet watch : Waiting for a file to change...", "", 6.6),
            ("", "dotnet watch : File changed: Now.cs. Restarting...", "sg hl", 20.6),
        ];
        Prompt(s, 62, 12.5);
        for (var i = 0; i < lines.Length; i++)
        {
            var (tag, text, cls, t) = lines[i];
            var y = 62 + (i + 1) * 17.6;
            var style = s.Window(T, t, Hold, fadeIn: .12, fadeOut: .5);
            if (tag != "")
                s.Raw($"""<g style="{style}">{s.Tx(22, y, "info:", "gn", 12.5)}{s.Tx(22 + 6 * 7.324, y, text, cls, 12.5)}</g>""");
            else
                s.T(22, y, text, cls, 12.5, style);
        }
        var blink = s.Frames(T, [(0, "opacity:0"), (6.8, "opacity:0"), (6.81, "opacity:1"), (20.5, "opacity:1"), (20.51, "opacity:0"), (T, "opacity:0")]);
        s.Raw($"""<g style="{blink}"><rect class="sg cur" x="22" y="{N(62 + 12 * 17.6 + 7)}" width="8" height="2"/></g>""");
    });

    s.Group("n", () =>
    {
        (string tag, string text, double t)[] lines =
        [
            ("info:", " listening on github.com/seifhesham22", 2.6),
            ("info:", $" last push {pushed}", 3.4),
            ("info:", " ASP.NET Core, EF Core, PostgreSQL,", 4.2),
            ("", "       SignalR, RabbitMQ", 4.2),
            ("info:", $" {live.Total} contributions last year", 5),
            ("info:", " application started", 5.8),
        ];
        Prompt(s, 78, 25, compact: true);
        for (var i = 0; i < lines.Length; i++)
        {
            var (tag, text, t) = lines[i];
            var y = 78 + (i + 1) * 36;
            var style = s.Window(T, t, Hold, fadeIn: .12, fadeOut: .5);
            s.Raw($"""<g style="{style}">{s.Tx(22, y, tag, "gn", 25)}{s.Tx(22 + 5 * 14.65, y, text, "ink", 25)}</g>""");
        }
    });
    return s;

    void Prompt(Svg svg, double y, double size, bool compact = false)
    {
        var advance = size * .5859;
        var prompt = compact ? "> " : @"PS C:\seif> ";
        var window = svg.Window(T, 0, Hold, fadeIn: .01, fadeOut: .5);
        var sb = new StringBuilder();
        sb.Append(svg.Tx(22, y, prompt, "sg", size));
        var x = 22 + prompt.Length * advance;
        const string Command = "dotnet watch";
        for (var i = 0; i < Command.Length; i++)
        {
            if (Command[i] == ' ') continue;
            var t = .4 + i * .08;
            sb.Append(svg.Tx(x + i * advance, y, Command[i].ToString(), "ink", size, svg.Window(T, t, 40, fadeIn: .01, fadeOut: 0)));
        }
        svg.Raw($"""<g style="{window}">{sb}</g>""");
    }
}

// ───────────────────────────────── Cards ─────────────────────────────────

Svg Uams() => Card("UniversityAssetManagementSystem-UAMS", "UAMS", "modular monolith · .NET 10",
    "University asset management: room layouts on a", "canvas, maintenance tickets, and a VK bot.",
    ["EF Core", "PostgreSQL", "MediatR", "FluentValidation"], s =>
{
    // TicketStatus and its transition table, from UAMS.Room/Models/Ticket.cs. InspectionRequested
    // and InspectionDone are drawn as one node, and the terminal side-exits are left out.
    const double T = 18;
    var nodes = new Dictionary<string, (double x, double y, string label, int place)>
    {
        ["open"] = (34, 128, "Open", 0),
        ["inspect"] = (100, 128, "Inspection", 0),
        ["fix"] = (166, 128, "SentForFix", 0),
        ["fixed"] = (232, 128, "Fixed", 0),
        ["confirmed"] = (298, 128, "Confirmed", 0),
        ["closed"] = (364, 128, "Closed", 0),
        ["parts"] = (199, 168, "AwaitingParts", 1),
        ["replace"] = (150, 205, "SentForReplacement", 1),
        ["replaced"] = (262, 205, "Replaced", 1),
    };
    (double x, double y) P(string k) => (nodes[k].x, nodes[k].y);

    s.T(22, 97, "ticket lifecycle, simplified from 12 states", "dm", 10.5);
    foreach (var (a, b) in new[] { ("open", "inspect"), ("inspect", "fix"), ("fix", "fixed"), ("fixed", "confirmed"), ("confirmed", "closed"), ("fix", "parts"), ("parts", "fixed"), ("inspect", "replace"), ("replace", "replaced"), ("replaced", "confirmed") })
        s.Raw($"""<path class="wire" d="M{N(P(a).x)} {N(P(a).y)}L{N(P(b).x)} {N(P(b).y)}"/>""");

    var hits = nodes.Keys.ToDictionary(k => k, _ => new List<double>());
    void Run(string[] route, double t0, double t1)
    {
        var at = s.Packet(route.Select(P).ToArray(), T, t0, t1);
        for (var i = 0; i < route.Length; i++) hits[route[i]].Add(at[i]);
    }
    Run(["open", "inspect", "fix", "fixed", "confirmed", "closed"], .2, 4.8);
    Run(["open", "inspect", "fix", "parts", "fixed", "confirmed", "closed"], 6.1, 11.3);
    Run(["open", "inspect", "replace", "replaced", "confirmed", "closed"], 12.5, 17.1);

    foreach (var (key, n) in nodes)
    {
        var y = n.place == 0 ? n.y - 11 : n.y + 16;
        var flash = s.Flash(T, hits[key], hold: .2, fade: .5);
        var done = key == "closed";
        s.T(n.x, y, n.label, "a", 10);
        s.T(n.x, y, n.label, $"a {(done ? "gn" : "ink")} hl", 10, flash);
        s.Dot(n.x, n.y, 3.5, "nd");
        s.Dot(n.x, n.y, 5, $"{(done ? "gnf" : "sg")} hl", flash);
    }
});

Svg Admissions() => Card("tsu_admission_backend_2", "Admissions", "4 services · .NET 8",
    "University admissions as four services, each", "with its own database, talking over events.",
    ["MassTransit", "RabbitMQ", "PostgreSQL", "S3"], s =>
{
    // Event names and consumers are the real ones from BuildingBlocks/Contracts and */Consumers.
    // RabbitMQ delivers at least once, so the second UserRegistered has to change nothing.
    const double T = 15, BoxY = 106, Bus = 174;
    (string name, string sub)[] services = [("Identity", ":8081"), ("Admission", ":8082"), ("Files", ":8083"), ("Notifications", "worker")];
    double X(int i) => 16 + i * 94 + 43;

    for (var i = 0; i < services.Length; i++)
    {
        var x = X(i);
        s.Raw($"""<path class="wire" d="M{N(x)} {N(BoxY + 40)}V{Bus}"/>""");
        s.Raw($"""<ellipse class="cyl" cx="{N(x)}" cy="87" rx="9" ry="3"/><path class="cyl" d="M{N(x - 9)} 87v9c0 4 18 4 18 0v-9"/><path class="wire" d="M{N(x)} 99V{BoxY}"/>""");
        s.Raw($"""<rect class="bx" x="{N(x - 43)}" y="{BoxY}" width="86" height="40" rx="7"/>""");
    }
    s.Raw($"""<path class="wire" d="M16 {Bus}H384" style="stroke-width:2"/>""");
    s.T(16, 192, "RabbitMQ", "", 10.5);
    s.T(384, 192, "one database per service", "e dm", 10.5);

    (double x, double y)[] Route(int from, int to) => [(X(from), BoxY + 40), (X(from), Bus), (X(to), Bus), (X(to), BoxY + 40)];
    var first = s.Packet(Route(0, 1), T, .3, 2.1);
    var second = s.Packet(Route(0, 1), T, 3.2, 5, tone: "dup");
    var access = s.Packet(Route(1, 2), T, 6.9, 8.7);
    var email = s.Packet(Route(1, 3), T, 10.2, 12);

    void Hit(int i, double t, string tone = "sg")
    {
        s.Raw($"""<rect class="{tone}s hl" x="{N(X(i) - 43)}" y="{BoxY}" width="86" height="40" rx="7" style="{s.Flash(T, [t], hold: .6, fade: .8)}"/>""");
    }
    Hit(1, first[3]);
    Hit(1, second[3], "dm");
    Hit(2, access[3]);
    Hit(3, email[3], "gn");

    for (var i = 0; i < services.Length; i++)
    {
        s.T(X(i), BoxY + 17, services[i].name, "a ink", 10);
        s.T(X(i), BoxY + 31, services[i].sub, "a dm", 10);
    }

    (string text, string cls, double on, double off)[] log =
    [
        ("UserRegistered: Admission creates the applicant", "ink", .3, 3),
        ("redelivered: existence check, nothing changes", "sg", 3.2, 6.7),
        ("AdmissionAccessChanged: Files updates access", "ink", 6.9, 10),
        ("SendEmailNotification: the worker sends it", "gn", 10.2, 14.4),
    ];
    foreach (var (text, cls, on, off) in log)
        s.T(16, 218, text, cls + " hl", 11, s.Window(T, on, off));
});

Svg PingPong() => Card("PingPongBackend", "PingPong", "vertical slices · .NET 10",
    "A Discord-style chat backend: friends, live DMs,", "and servers with rank-based role permissions.",
    ["SignalR", "EF Core", "PostgreSQL", "MediatR"], s =>
{
    // A DM goes in over HTTP and is pushed to every device of both participants.
    const double T = 9, Hub = 200, HubY = 130;
    (double x, double y)[] devices = [(46, 102), (46, 156), (354, 102), (354, 156)];

    foreach (var (x, y) in devices)
        s.Raw($"""<path class="wire" d="M{N(x)} {N(y)}L{N(Hub)} {N(HubY)}"/>""");
    s.T(46, 188, "sender", "a", 10.5);
    s.T(354, 188, "recipient", "a", 10.5);

    var inbound = s.Packet([devices[0], (Hub, HubY)], T, .3, 1.6);
    s.T(112, 104, "HTTP", "a sg hl", 10.5, s.Flash(T, [inbound[0]], hold: .9, fade: .4));

    var hub = s.Flash(T, [inbound[1]], hold: .5, fade: .8);
    s.Raw($"""<circle class="bx" cx="{N(Hub)}" cy="{N(HubY)}" r="27"/>""");
    s.Dot(Hub, HubY, 27, "sg hl", hub + ";fill-opacity:.2");
    s.T(Hub, HubY + 4, "SignalR", "a ink", 11);

    var arrive = 0.0;
    foreach (var d in devices)
    {
        var at = s.Packet([(Hub, HubY), d], T, 2, 3.2);
        arrive = at[1];
    }
    foreach (var (x, y) in devices)
    {
        s.Raw($"""<rect class="bg" x="{N(x - 16)}" y="{N(y - 11)}" width="32" height="22" rx="4"/>""");
        s.Raw($"""<rect class="bx" x="{N(x - 16)}" y="{N(y - 11)}" width="32" height="22" rx="4"/>""");
        var bubble = s.Flash(T, [arrive], hold: 1.8, fade: .6);
        s.Raw($"""<rect class="sg hl" x="{N(x - 10)}" y="{N(y - 5)}" width="20" height="4" rx="2" style="{bubble}"/><rect class="sg hl" x="{N(x - 10)}" y="{N(y + 2)}" width="12" height="4" rx="2" style="{bubble}"/>""");
    }

    // Roles carry a 16-flag bitfield and a rank. Three example roles flip through it.
    (string name, int mask)[] roles = [("@everyone", 0b0000_0000_0000_0111), ("moderator", 0b0000_1111_0001_0111), ("owner", 0b1111_1111_1111_1111)];
    const double RoleSpan = T / 3;
    s.T(130, 201, "16-flag permissions, by rank", "dm", 10);
    for (var r = 0; r < roles.Length; r++)
        s.T(22, 220, roles[r].name, "ink hl", 11, s.Window(T, r * RoleSpan + .05, (r + 1) * RoleSpan - .2, fadeIn: .15, fadeOut: .15));
    for (var bit = 0; bit < 16; bit++)
    {
        var x = 130 + bit * 15.5;
        s.Raw($"""<rect class="bx" x="{N(x)}" y="208" width="12.5" height="14" rx="3"/>""");
        var windows = new List<(double, string)> { (0, "opacity:0") };
        for (var r = 0; r < roles.Length; r++)
            if ((roles[r].mask >> bit & 1) == 1)
            {
                var from = r * RoleSpan + bit * .025;
                windows.Add((from, "opacity:0"));
                windows.Add((from + .08, "opacity:1"));
                windows.Add(((r + 1) * RoleSpan - .15, "opacity:1"));
                windows.Add(((r + 1) * RoleSpan - .02, "opacity:0"));
            }
        windows.Add((T, "opacity:0"));
        s.Raw($"""<rect class="sg hl" x="{N(x)}" y="208" width="12.5" height="14" rx="3" style="{s.Frames(T, windows)}"/>""");
    }
});

Svg Desktop() => Card("PingPongDesktopApp", "PingPong Desktop", "MVVM · Avalonia 12 · .NET 10",
    "A desktop client in progress. Splash, login and", "register screens so far, plus token storage.",
    ["Avalonia", "CommunityToolkit.Mvvm", "C#"], s =>
{
    // Replays the sign-in flow, and lights up the real control or service as each part of it runs.
    const double T = 14, Wx = 22, Wy = 86, W = 256, H = 142, Cx = Wx + W / 2;
    double[] press = [2.9, 6.3];
    const double Fail = 4, Success = 7.3, End = 12.8;

    s.Raw($"""<rect class="bx" x="{Wx}" y="{Wy}" width="{W}" height="{H}" rx="8"/><path class="ln" d="M{Wx} {Wy + 20}H{Wx + W}"/>""");
    s.T(Wx + 10, Wy + 14, "PingPong", "", 10);
    s.Raw($"""<path class="ctl" style="stroke-width:1" d="M{Wx + W - 46} {Wy + 11}h7M{Wx + W - 29} {Wy + 7}h7v7h-7zM{Wx + W - 12} {Wy + 7}l6 7m0-7-6 7"/>""");

    void Dots(double x, double y, string style)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 3; i++)
            sb.Append($"""<circle class="sg bob" cx="{N(x - 10 + i * 10)}" cy="{N(y)}" r="3" style="animation-delay:{N(i * .14)}s"/>""");
        s.Raw($"""<g class="hl" style="{style}">{sb}</g>""");
    }
    s.Css("@keyframes bob{0%,60%,100%{transform:translateY(0)}30%{transform:translateY(-5px)}}.bob{animation:bob .9s ease-in-out infinite}");

    // Splash
    var splash = s.Window(T, 0, 1.5, fadeIn: .25, fadeOut: .2);
    s.Raw($"""<g class="hl" style="{splash}">{s.Tx(Cx, Wy + 76, "PingPong", "a d", 20)}</g>""");
    Dots(Cx, Wy + 96, splash);

    // Login form. It shakes when the first attempt fails, then the second one signs in.
    var form = s.Window(T, 1.6, Success - .1, fadeIn: .25, fadeOut: .2);
    var shake = s.Frames(T,
    [
        (0, "transform:translateX(0)"), (Fail, "transform:translateX(0)"), (Fail + .07, "transform:translateX(-7px)"),
        (Fail + .14, "transform:translateX(7px)"), (Fail + .21, "transform:translateX(-5px)"), (Fail + .28, "transform:translateX(5px)"),
        (Fail + .35, "transform:translateX(-2px)"), (Fail + .42, "transform:translateX(0)"), (T, "transform:translateX(0)"),
    ]);
    s.Raw($"""<g style="{form}"><g style="{shake}">""");
    s.Raw($"""<rect class="fd" x="{Cx - 92}" y="{Wy + 32}" width="184" height="24" rx="5"/><rect class="dmf" x="{Cx - 83}" y="{Wy + 42}" width="72" height="4" rx="2"/>""");
    s.Raw($"""<rect class="fd" x="{Cx - 92}" y="{Wy + 62}" width="184" height="24" rx="5"/>""");
    s.Raw($"""<rect class="sgs hl" x="{Cx - 92}" y="{Wy + 62}" width="184" height="24" rx="5" style="{s.Flash(T, [Fail], hold: .7, fade: .5)}"/>""");
    for (var i = 0; i < 6; i++)
    {
        var dot = s.Frames(T,
        [
            (0, "opacity:0"), (2 + i * .12, "opacity:0"), (2.01 + i * .12, "opacity:1"), (5.1, "opacity:1"), (5.15, "opacity:0"),
            (5.35 + i * .12, "opacity:0"), (5.36 + i * .12, "opacity:1"), (T, "opacity:1"),
        ]);
        s.Dot(Cx - 80 + i * 11, Wy + 74, 2.6, "inkf", dot);
    }
    s.Raw("</g>");

    // The button swaps its label for LoadingDots while a request is in flight.
    s.Raw($"""<rect class="btn" x="{Cx - 92}" y="{Wy + 96}" width="184" height="30" rx="6"/>""");
    var label = s.Frames(T,
    [
        (0, "opacity:1"), (press[0], "opacity:1"), (press[0] + .05, "opacity:0"), (Fail, "opacity:0"), (Fail + .05, "opacity:1"),
        (press[1], "opacity:1"), (press[1] + .05, "opacity:0"), (T, "opacity:0"),
    ]);
    s.T(Cx, Wy + 115, "Log in", "a ink", 12, label);
    var loading = s.Frames(T,
    [
        (0, "opacity:0"), (press[0], "opacity:0"), (press[0] + .05, "opacity:1"), (Fail, "opacity:1"), (Fail + .05, "opacity:0"),
        (press[1], "opacity:0"), (press[1] + .05, "opacity:1"), (T, "opacity:1"),
    ]);
    Dots(Cx, Wy + 111, loading);
    s.Raw("</g>");

    // Signed in
    var welcome = s.Window(T, Success, End, fadeIn: .3, fadeOut: .4);
    s.Raw($"""<g class="hl" style="{welcome}"><path class="chk" d="M{Cx - 14} {Wy + 66}l10 10 18-20"/>{s.Tx(Cx, Wy + 104, "Signed in", "a d", 18)}</g>""");

    // The classes doing the work, from Controls/ and Services/ in the repo.
    (string name, double[] at, double hold)[] parts =
    [
        ("LoadingDots", [.1, press[0], press[1]], 1),
        ("ShakeBehavior", [Fail], .9),
        ("TokenStore", [Success], 4),
    ];
    for (var i = 0; i < parts.Length; i++)
    {
        var y = 124 + i * 30;
        var flash = s.Flash(T, parts[i].at, hold: parts[i].hold, fade: .3);
        s.Dot(291, y - 4, 2.5, "nd");
        s.T(298, y, parts[i].name, "", 10.5);
        s.Dot(291, y - 4, 3.2, "sg hl", flash);
        s.T(298, y, parts[i].name, "ink hl", 10.5, flash);
    }
});

Svg TaskTracker() => Card("TaskTracker", "TaskTracker", "GraphQL · web + mobile · .NET 10",
    "A task board with a GraphQL API and React and", "Expo clients. Moving a task changes its status.",
    ["Hot Chocolate", "React", "Expo", "PostgreSQL"], s =>
{
    // moveTask puts a task at the bottom of a column, and its status is that column's category.
    const double T = 11, Top = 84, ColW = 112, Gap = 8, First = 24, CardW = 96, CardH = 20;
    string[] columns = ["ToDo", "InProgress", "Done"];
    double Cx(int i) => First + i * (ColW + Gap);
    double Row(int r) => Top + 24 + r * 25;

    void TaskCard(double x, double y, string cls = "bx", double bar = 44) =>
        s.Raw($"""<rect class="{cls}" x="{N(x)}" y="{N(y)}" width="{CardW}" height="{CardH}" rx="4"/><rect class="dmf" x="{N(x + 8)}" y="{N(y + 8)}" width="{N(bar)}" height="4" rx="2"/>""");

    for (var i = 0; i < columns.Length; i++)
    {
        s.Raw($"""<rect class="cl" x="{N(Cx(i))}" y="{Top}" width="{ColW}" height="100" rx="7"/>""");
        s.T(Cx(i) + 8, Top + 15, columns[i], "", 10.5);
    }
    TaskCard(Cx(0) + 8, Row(0), bar: 62);
    TaskCard(Cx(0) + 8, Row(1), bar: 38);
    TaskCard(Cx(1) + 8, Row(0), bar: 54);
    TaskCard(Cx(2) + 8, Row(0), bar: 46);

    double dx = ColW + Gap, dy = Row(1) - Row(2);
    double[] moves = [1.4, 5.8];
    const double Travel = .7;
    string Tr(double x, double y) => $"transform:translate({N(x)}px,{N(y)}px)";
    var move = s.Frames(T,
    [
        (0, Tr(0, 0) + ";opacity:0"), (.3, Tr(0, 0) + ";opacity:1"), (moves[0], Tr(0, 0)), (moves[0] + Travel, Tr(dx, dy)),
        (moves[1], Tr(dx, dy)), (moves[1] + Travel, Tr(2 * dx, dy)), (10.3, Tr(2 * dx, dy) + ";opacity:1"), (10.7, Tr(2 * dx, dy) + ";opacity:0"), (T, Tr(2 * dx, dy) + ";opacity:0"),
    ], timing: "cubic-bezier(.45,0,.2,1)");
    s.Raw($"""<g style="{move}"><rect class="bg" x="{N(Cx(0) + 8)}" y="{N(Row(2))}" width="{CardW}" height="{CardH}" rx="4"/><rect class="sgs" x="{N(Cx(0) + 8)}" y="{N(Row(2))}" width="{CardW}" height="{CardH}" rx="4"/><rect class="sg" x="{N(Cx(0) + 16)}" y="{N(Row(2) + 8)}" width="56" height="4" rx="2"/></g>""");

    // Both clients send the same mutation to the same GraphQL endpoint.
    const double Y = 216;
    (string text, double x, double w)[] pills = [("web", 24, 36), ("mobile", 66, 52), ("moveTask", 166, 64), ("PostgreSQL", 296, 80)];
    s.Raw($"""<path class="wire" d="M118 {Y}H166M230 {Y}H296"/>""");
    foreach (var (text, x, w) in pills)
    {
        s.Raw($"""<rect class="bx" x="{N(x)}" y="{Y - 11}" width="{N(w)}" height="22" rx="11"/>""");
        s.T(x + w / 2, Y + 4, text, "a", 10.5);
    }
    foreach (var m in moves)
    {
        s.Packet([(118, Y), (166, Y)], T, m - .5, m - .15);
        s.Packet([(230, Y), (296, Y)], T, m - .1, m + .35);
    }
    s.T(198, Y + 4, "moveTask", "a sg hl", 10.5, s.Flash(T, moves.Select(m => m - .2), hold: .3, fade: .4));
});

Svg Restaurant() => Card("RestaurantBackend", "RestaurantBackend", "clean architecture · ASP.NET",
    "Restaurant management API with JWT + refresh", "tokens, custom middleware and Serilog logging.",
    ["EF Core", "SQL Server", "AutoMapper", "xUnit", "Moq"], s =>
{
    // Every project references Core and Core references nothing, so all the traffic runs inward.
    const double T = 7.5, Cx = 200, Cy = 150;
    s.T(22, 97, "every dependency points at Core", "dm", 10.5);

    s.Css("@keyframes spin{to{transform:rotate(360deg)}}.spin{transform-origin:200px 150px;animation:spin 40s linear infinite}");
    s.Raw($"""<circle class="ring spin" cx="{Cx}" cy="{Cy}" r="48"/>""");

    (string name, double x, double y, double w)[] rings = [("Restaurant.API", 22, 150, 100), ("Infrastructure", 278, 150, 100), ("ServiceTests", 152, 216, 96)];
    (double x, double y)[][] routes =
    [
        [(122, 150), (166, 150)],
        [(278, 150), (234, 150)],
        [(200, 202), (200, 184)],
    ];
    foreach (var route in routes)
        s.Raw($"""<path class="wire" d="M{N(route[0].x)} {N(route[0].y)}L{N(route[1].x)} {N(route[1].y)}"/>""");
    foreach (var (name, x, y, w) in rings)
    {
        s.Raw($"""<rect class="bx" x="{N(x)}" y="{N(y - 14)}" width="{N(w)}" height="28" rx="7"/>""");
        s.T(x + w / 2, y + 4, name, "a ink", 10.5);
    }

    var arrivals = new List<double>();
    double[] starts = [.4, 2.8, 5.2];
    for (var i = 0; i < routes.Length; i++)
        arrivals.Add(s.Packet(routes[i], T, starts[i], starts[i] + .9)[1]);

    s.Raw($"""<circle class="bx" cx="{Cx}" cy="{Cy}" r="34"/>""");
    s.Dot(Cx, Cy, 34, "sg hl", s.Flash(T, arrivals, hold: .25, fade: .9, peak: .22));
    s.T(Cx, Cy + 6, "Core", "a d", 17);
});

Svg Stack()
{
    const double T = 9, X = 34;
    var s = new Svg(400, 348, "The stack, in the order a request meets it");
    Frame(s, "Stack", "in the order a request meets it");
    (string layer, string items)[] rows =
    [
        ("client", "Avalonia, React, Expo"),
        ("edge", "ASP.NET Core, Hot Chocolate, SignalR"),
        ("auth", "Identity, JWT + refresh, 2FA"),
        ("app", "MediatR, FluentValidation"),
        ("data", "EF Core, PostgreSQL, SQL Server, S3"),
        ("events", "RabbitMQ, MassTransit"),
        ("ship", "Docker, GitHub Actions, xUnit, Serilog"),
    ];
    double Y(int i) => 104 + i * 35;

    (double x, double y)[] spine = [(X, Y(0)), (X, Y(rows.Length - 1))];
    s.Raw($"""<path class="wire" d="M{X} {Y(0)}V{Y(rows.Length - 1)}"/>""");
    var at = s.Packet(spine, T, .2, 7.2);
    for (var i = 0; i < rows.Length; i++)
    {
        var when = at[0] + (at[1] - at[0]) * i / (rows.Length - 1);
        var flash = s.Flash(T, [when], hold: .25, fade: .8);
        s.Dot(X, Y(i), 3.5, "nd");
        s.Dot(X, Y(i), 5, "sg hl", flash);
        s.T(50, Y(i) + 4, rows[i].layer, "", 11);
        s.T(50, Y(i) + 4, rows[i].layer, "sg hl", 11, flash);
        s.T(104, Y(i) + 4, rows[i].items, "ink", 11.5);
    }
    return s;
}

Svg Patterns()
{
    const double T = 10.5;
    var s = new Svg(400, 348, "Architecture patterns and the projects that use them");
    Frame(s, "Patterns", "and where they show up");
    (string pattern, string where)[] rows =
    [
        ("modular monolith", "UAMS"),
        ("database per service", "Admissions"),
        ("vertical slices", "UAMS, PingPong, TaskTracker"),
        ("rich domain model", "UAMS, PingPong, Admissions"),
        ("idempotent consumers", "Admissions"),
        ("clean architecture", "RestaurantBackend"),
        ("results over exceptions", "PingPong"),
    ];
    double Y(int i) => 104 + i * 35;
    for (var i = 0; i < rows.Length; i++)
    {
        var flash = s.Flash(T, [.2 + i * 1.45], hold: .75, fade: .5);
        s.Raw($"""<rect class="sg hl" x="3.5" y="{N(Y(i) - 17)}" width="393" height="35" style="{flash}{";fill-opacity:.09"}"/><rect class="sg hl" x="3.5" y="{N(Y(i) - 17)}" width="2.5" height="35" style="{flash}"/>""");
        if (i > 0) s.Raw($"""<path class="ln" d="M22 {N(Y(i) - 17.5)}H378"/>""");
        s.T(22, Y(i) + 4, rows[i].pattern, "ink", 11.5);
        s.T(378, Y(i) + 4, rows[i].where, "e", 10.5);
    }
    return s;
}

// ──────────────────────────────── Activity ───────────────────────────────

Svg Activity()
{
    var s = new Svg(804, 252, "Contribution heatmap for the last year");
    if (live.Days.Count == 0) return s;

    var first = live.Days.Min(d => d.Date);
    var start = first.AddDays(-(int)first.DayOfWeek);
    int Week(DateOnly d) => (d.DayNumber - start.DayNumber) / 7;
    var weeks = live.Days.Max(d => Week(d.Date)) + 1;

    s.Css($"@keyframes sw{{0%{{opacity:0}}1%{{opacity:1}}6%{{opacity:1}}12%,100%{{opacity:0}}}}.sw{{animation:sw 9s linear infinite backwards}}");

    void Cell(double x, double y, double size, Contribution day, double delay)
    {
        s.Raw($"""<rect class="h{day.Level}" x="{N(x)}" y="{N(y)}" width="{N(size)}" height="{N(size)}" rx="{N(size * .2)}"/>""");
        if (day.Level > 0)
            s.Raw($"""<rect class="hi hl sw" x="{N(x)}" y="{N(y)}" width="{N(size)}" height="{N(size)}" rx="{N(size * .2)}" style="animation-delay:{N(delay)}s"/>""");
    }

    s.Group("w", () =>
    {
        (string value, string label)[] tiles =
        [
            (live.Total.ToString("N0"), "contributions in the last year"),
            (live.ActiveDays.ToString(), "active days"),
            ($"{live.LongestStreak} days", "longest streak"),
            ($"{live.CSharpShare}%", "of public code is C#"),
        ];
        for (var i = 0; i < tiles.Length; i++)
        {
            var x = 22 + i * 190 + (i > 0 ? 40 : 0);
            s.T(x, 44, tiles[i].value, "d", 30);
            s.T(x, 64, tiles[i].label, "", 11.5);
        }

        const double X0 = 48, Y0 = 108, Step = 13.8, Size = 11.8;
        var lastMonth = -1; var lastLabelWeek = -9;
        foreach (var day in live.Days.OrderBy(d => d.Date))
        {
            var w = Week(day.Date);
            if (day.Date.Day <= 7 && day.Date.DayOfWeek == DayOfWeek.Sunday && day.Date.Month != lastMonth && w - lastLabelWeek >= 3)
            {
                s.T(X0 + w * Step, Y0 - 7, day.Date.ToString("MMM"), "", 10.5);
                lastMonth = day.Date.Month; lastLabelWeek = w;
            }
            Cell(X0 + w * Step, Y0 + (int)day.Date.DayOfWeek * Step, Size, day, w * .045);
        }
        foreach (var (name, row) in new[] { ("Mon", 1), ("Wed", 3), ("Fri", 5) })
            s.T(22, Y0 + row * Step + 9.5, name, "", 10.5);

        const double LegendY = 222;
        s.T(48, LegendY + 10, "last 12 months", "", 10.5);
        var legendX = 782 - 5 * Step - 34;
        s.T(legendX - 8, LegendY + 10, "Less", "e", 10.5);
        for (var level = 0; level <= 4; level++)
            s.Raw($"""<rect class="h{level}" x="{N(legendX + level * Step)}" y="{LegendY}" width="{Size}" height="{Size}" rx="2.4"/>""");
        s.T(legendX + 5 * Step + 6, LegendY + 10, "More", "", 10.5);
    });

    // Narrow: only the recent half, large enough to read at phone width.
    s.Group("n", () =>
    {
        s.T(22, 40, $"{live.Total:N0} contributions in the last year", "d", 32);
        const int Shown = 30;
        var step = (804 - 44) / (double)Shown;
        var from = weeks - Shown;
        foreach (var day in live.Days.Where(d => Week(d.Date) >= from))
        {
            var w = Week(day.Date) - from;
            Cell(22 + w * step, 62 + (int)day.Date.DayOfWeek * step, step - 3.5, day, w * .08);
        }
    });
    return s;
}

// ──────────────────────────────── Shared ─────────────────────────────────

Svg Card(string repo, string title, string sub, string line1, string line2, string[] chips, Action<Svg> stage)
{
    var s = new Svg(400, 348, $"{title}. {line1} {line2}");
    Frame(s, title, sub);
    s.Raw("""<path class="ctl" style="stroke-width:1.6" d="M366 36l11-11m-8.5 0h8.5v8.5"/>""");

    if (live.Repos.FirstOrDefault(r => r.Name == repo) is { } info)
    {
        var text = $"pushed {Day(info.PushedAt)}";
        s.T(378, 63, text, "e dm", 11);
        var dotX = 378 - text.Length * 6.445 - 10;
        if (DateTime.UtcNow - info.PushedAt < TimeSpan.FromDays(14)) PulseDot(s, dotX, 59, 3);
        else s.Dot(dotX, 59, 3, "dmf");
    }

    s.Def("""<pattern id="s" width="12" height="12" patternUnits="userSpaceOnUse"><circle cx="1" cy="1" r=".7" class="dt"/></pattern>""");
    s.Raw("""<rect x="3.5" y="76" width="393" height="162" fill="url(#s)"/><path class="ln" d="M3.5 76H396.5M3.5 238H396.5"/>""");
    stage(s);

    s.T(22, 262, line1, "ink", 12);
    s.T(22, 279, line2, "ink", 12);

    var x = 22.0;
    foreach (var chip in chips)
    {
        var w = chip.Length * 6.445 + 16;
        s.Raw($"""<rect class="bx" x="{N(x)}" y="298" width="{N(w)}" height="24" rx="12"/>""");
        s.T(x + w / 2, 314, chip, "a", 11);
        x += w + 6;
    }
    if (x > 384) throw new InvalidOperationException($"Chips overflow on {title}: {x}");
    return s;
}

static void Frame(Svg s, string title, string sub)
{
    s.Raw("""<rect x="3.5" y="3.5" width="393" height="341" rx="14" class="fr"/>""");
    s.T(22, 43, title, "d", 25);
    s.T(22, 63, sub, "", 11.5);
}

static void PulseDot(Svg s, double x, double y, double r)
{
    s.Css("@keyframes pu{0%{opacity:.55;transform:scale(1)}70%,100%{opacity:0;transform:scale(2.6)}}.pu{transform-box:fill-box;transform-origin:center;animation:pu 2.4s ease-out infinite}");
    s.Dot(x, y, r, "sg pu hl0");
    s.Dot(x, y, r, "sg");
}

void UpdateReadme(string path)
{
    const string Start = "<!-- activity-table:start -->", End = "<!-- activity-table:end -->";
    if (!File.Exists(path)) return;
    var text = File.ReadAllText(path);
    var a = text.IndexOf(Start); var b = text.IndexOf(End);
    if (a < 0 || b < a) return;

    var table = new StringBuilder("\n\n| Month | Contributions |\n| --- | ---: |\n");
    foreach (var month in live.Days.GroupBy(d => (d.Date.Year, d.Date.Month)).OrderBy(g => g.Key))
        table.Append($"| {new DateOnly(month.Key.Year, month.Key.Month, 1):MMM yyyy} | {month.Sum(d => d.Count)} |\n");
    table.Append($"| **Total** | **{live.Total}** |\n\n");

    File.WriteAllText(path, text[..(a + Start.Length)] + table + text[b..]);
}

static string Day(DateTime date) => date.ToString("d MMM yyyy");
static string N(double value) => Svg.N(value);
static string SourcePath([CallerFilePath] string path = "") => path;

// ──────────────────────────────── Drawing ────────────────────────────────

sealed class Svg(int width, int height, string title)
{
    readonly StringBuilder _css = new(), _defs = new(), _body = new();
    readonly HashSet<string> _cssOnce = [], _defsOnce = [];
    int _keyframes;

    public static string N(double value) => Math.Round(value, 2).ToString("0.##");
    static string Escape(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    public void Raw(string markup) => _body.Append(markup);
    public void Css(string rules) { if (_cssOnce.Add(rules)) _css.Append(rules.Replace("\n", "").Replace("    ", "")); }
    public void Def(string markup) { if (_defsOnce.Add(markup)) _defs.Append(markup); }

    public void Group(string cls, Action content)
    {
        _body.Append($"""<g class="{cls}">""");
        content();
        _body.Append("</g>");
    }

    public string Tx(double x, double y, string text, string cls = "", double size = 12, string style = "")
    {
        var classes = cls == "" ? "" : $" class=\"{cls}\"";
        // Multiple spaces are the log's indentation, so they have to survive.
        return $"""<text x="{N(x)}" y="{N(y)}"{classes} style="font-size:{N(size)}px;{style}" xml:space="preserve">{Escape(text)}</text>""";
    }

    public void T(double x, double y, string text, string cls = "", double size = 12, string style = "") => _body.Append(Tx(x, y, text, cls, size, style));

    public void Dot(double x, double y, double r, string cls, string style = "") =>
        _body.Append($"""<circle cx="{N(x)}" cy="{N(y)}" r="{N(r)}" class="{cls}" style="{style}"/>""");

    /// <summary>Emits keyframes from absolute times in a looping timeline, and returns the style that plays them.</summary>
    public string Frames(double period, IEnumerable<(double t, string css)> frames, double delay = 0, string timing = "linear")
    {
        var name = $"k{_keyframes++}";
        var sb = new StringBuilder($"@keyframes {name}{{");
        foreach (var (t, css) in frames)
            sb.Append(Math.Round(Math.Clamp(t / period, 0, 1) * 100, 3).ToString("0.###")).Append("%{").Append(css).Append('}');
        _css.Append(sb.Append('}'));
        return $"animation:{name} {N(period)}s {timing} {N(delay)}s infinite backwards";
    }

    /// <summary>Visible only between <paramref name="on"/> and <paramref name="off"/>.</summary>
    public string Window(double period, double on, double off, double fadeIn = .2, double fadeOut = .2, double delay = 0)
    {
        if (off >= period) return Frames(period, [(0, "opacity:0"), (on, "opacity:0"), (on + fadeIn, "opacity:1"), (period, "opacity:1")], delay);
        return Frames(period, [(0, "opacity:0"), (on, "opacity:0"), (on + fadeIn, "opacity:1"), (off, "opacity:1"), (off + fadeOut, "opacity:0"), (period, "opacity:0")], delay);
    }

    /// <summary>Lights an overlay up briefly at each of the given times.</summary>
    public string Flash(double period, IEnumerable<double> times, double hold = .35, double fade = .6, double peak = 1, double delay = 0)
    {
        var frames = new List<(double, string)> { (0, "opacity:0") };
        foreach (var t in times.OrderBy(x => x))
        {
            frames.Add((t, "opacity:0"));
            frames.Add((t + .05, $"opacity:{N(peak)}"));
            frames.Add((t + .05 + hold, $"opacity:{N(peak)}"));
            frames.Add((t + .05 + hold + fade, "opacity:0"));
        }
        frames.Add((period, "opacity:0"));
        return Frames(period, frames, delay);
    }

    /// <summary>Sends a light along a polyline between t0 and t1. Returns when it reaches each point.</summary>
    public double[] Packet((double x, double y)[] points, double period, double t0, double t1, string tone = "sig", double delay = 0, double width = 2.5)
    {
        const double Head = 14, Tail = 42;
        var cumulative = new double[points.Length];
        for (var i = 1; i < points.Length; i++)
            cumulative[i] = cumulative[i - 1] + Math.Sqrt(Math.Pow(points[i].x - points[i - 1].x, 2) + Math.Pow(points[i].y - points[i - 1].y, 2));
        var length = cumulative[^1];
        var d = "M" + string.Join("L", points.Select(p => $"{N(p.x)} {N(p.y)}"));

        var endHead = -length;
        var endTail = Tail - Head - length;
        string Trail(double offset, double opacity) => $"opacity:{N(opacity)};stroke-dashoffset:{N(offset)}px";

        var tail = Frames(period, [(0, Trail(Tail, 0)), (t0, Trail(Tail, 0)), (t0 + .01, Trail(Tail, .28)), (t1, Trail(endTail, .28)), (t1 + .14, Trail(endTail, 0)), (period, Trail(endTail, 0))], delay);
        var head = Frames(period, [(0, Trail(Head, 0)), (t0, Trail(Head, 0)), (t0 + .01, Trail(Head, 1)), (t1, Trail(endHead, 1)), (t1 + .01, Trail(endHead, 0)), (period, Trail(endHead, 0))], delay);
        _body.Append($"""<path class="pk {tone}" d="{d}" style="stroke-width:{N(width)};stroke-dasharray:{Tail}px 9999px;{tail}"/>""");
        _body.Append($"""<path class="pk {tone}" d="{d}" style="stroke-width:{N(width)};stroke-dasharray:{Head}px 9999px;{head}"/>""");

        var speed = (length + Head) / (t1 - t0);
        return cumulative.Select(c => t0 + c / speed).ToArray();
    }

    public string Render(string fonts) =>
        $"""<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}" role="img" aria-label="{Escape(title)}"><title>{Escape(title)}</title><style>{fonts}{Theme.Palette}{Theme.Base}{_css}</style><defs>{_defs}</defs>{_body}</svg>""";
}

static class Theme
{
    // Colours are translucent wherever they can be, so they sit right on GitHub's light, dark and dimmed pages.
    public const string Dark = ":root{--ink:#EDEAE4;--mut:#9BA4B0;--dm:#7D8590;--line:rgba(240,246,252,.13);--wire:rgba(240,246,252,.24);--panel:rgba(240,246,252,.022);--tab:rgba(240,246,252,.06);--sig:#FF8A3D;--ok:#3FB950;--bg:#0d1117;--h0:rgba(240,246,252,.07);--h1:#5C3318;--h2:#A5541D;--h3:#E2732D;--h4:#FFAD70;--hi:#FFE3CC}";
    public const string Light = ":root{--ink:#16191E;--mut:#57606A;--dm:#6E7781;--line:rgba(31,35,40,.14);--wire:rgba(31,35,40,.26);--panel:rgba(31,35,40,.018);--tab:rgba(31,35,40,.05);--sig:#D9480F;--ok:#1A7F37;--bg:#fff;--h0:rgba(31,35,40,.07);--h1:#FFD3B5;--h2:#FF9F60;--h3:#E2621C;--h4:#9C3A09;--hi:#5C2003}";

    // One file holds both palettes. An SVG inside <img> resolves this query against the page's color-scheme,
    // which GitHub sets from the visitor's theme. A linked <picture> would not work: GitHub wraps the <img>
    // in a second link, and the nested anchors push it out of the <picture>.
    public const string Palette = Dark + "@media (prefers-color-scheme:light){" + Light + "}";

    public const string Base =
        "text{font-family:m,ui-monospace,Menlo,Consolas,monospace;fill:var(--mut)}" +
        ".d{font-family:d,system-ui,'Segoe UI',sans-serif;font-weight:700;fill:var(--ink);letter-spacing:-.01em}" +
        ".ink{fill:var(--ink)}.dm{fill:var(--dm)}.sg{fill:var(--sig)}.gn{fill:var(--ok)}.a{text-anchor:middle}.e{text-anchor:end}" +
        ".inkf{fill:var(--ink)}.dmf{fill:var(--dm);opacity:.55}.gnf{fill:var(--ok)}.nd{fill:var(--mut)}.dt{fill:var(--line)}.bg{fill:var(--bg)}" +
        ".ln{stroke:var(--line);fill:none}.wire{stroke:var(--wire);stroke-width:1.2;fill:none}" +
        ".fr{fill:var(--panel);stroke:var(--line)}.bx{fill:var(--panel);stroke:var(--wire)}.st{fill:var(--panel)}.tb{fill:var(--tab)}.cl{fill:var(--panel);stroke:var(--line)}" +
        ".fd{fill:none;stroke:var(--wire)}.btn{fill:var(--tab);stroke:var(--wire)}.cyl{fill:none;stroke:var(--wire);stroke-width:1.1}.ring{fill:none;stroke:var(--wire);stroke-dasharray:3 7}" +
        ".ctl{fill:none;stroke:var(--mut);stroke-width:1.2;stroke-linecap:round;stroke-linejoin:round}.chk{fill:none;stroke:var(--ok);stroke-width:3.5;stroke-linecap:round;stroke-linejoin:round}" +
        ".sgs{fill:none;stroke:var(--sig);stroke-width:1.6}.dms{fill:none;stroke:var(--dm);stroke-width:1.6}.gns{fill:none;stroke:var(--ok);stroke-width:1.6}" +
        ".pk{fill:none;stroke-linecap:round;opacity:0}.sig{stroke:var(--sig)}.ok{stroke:var(--ok)}.dup{stroke:var(--dm)}.hl{opacity:0}.hl0{opacity:.5}" +
        ".cur{animation:bl 1.1s steps(2) infinite}@keyframes bl{from{opacity:1}to{opacity:0}}" +
        ".h0{fill:var(--h0)}.h1{fill:var(--h1)}.h2{fill:var(--h2)}.h3{fill:var(--h3)}.h4{fill:var(--h4)}.hi{fill:var(--hi)}" +
        ".n{display:none}@media (max-width:560px){.w{display:none}.n{display:inline}}" +
        "@media (prefers-reduced-motion:reduce){*{animation:none!important}.pk,.gs,.hl,.pu{display:none}}";
}

static class Fonts
{
    public static string Css(string tools)
    {
        string Face(string family, string file, string extra) =>
            $"@font-face{{font-family:{family};{extra}src:url(data:font/woff2;base64,{Convert.ToBase64String(File.ReadAllBytes(Path.Combine(tools, "fonts", file)))}) format('woff2')}}";
        return Face("d", "display.woff2", "font-weight:700;") + Face("m", "mono.woff2", "");
    }
}

static class Repos
{
    // How a repo appears in prose when its GitHub name is not the name people should read.
    public static string Short(string name) => name switch
    {
        "UniversityAssetManagementSystem-UAMS" => "UAMS",
        "tsu_admission_backend_2" => "Admissions",
        "PingPongBackend" => "PingPong",
        "PingPongDesktopApp" => "PingPong Desktop",
        _ => name,
    };
}

// ─────────────────────────────── Live data ───────────────────────────────

sealed record Repo(string Name, DateTime PushedAt);
sealed record Contribution(DateOnly Date, int Level, int Count);

sealed record LiveData(List<Repo> Repos, List<Contribution> Days, int CSharpShare)
{
    public Repo? Latest => Repos.MaxBy(r => r.PushedAt);
    public int Total => Days.Sum(d => d.Count);
    public int ActiveDays => Days.Count(d => d.Count > 0);
    public int LongestStreak
    {
        get
        {
            int best = 0, run = 0;
            foreach (var day in Days.OrderBy(d => d.Date))
                best = Math.Max(best, run = day.Count > 0 ? run + 1 : 0);
            return best;
        }
    }
}

static class Live
{
    const string Login = "seifhesham22";

    public static async Task<LiveData> Load(string tools, bool offline)
    {
        var cachePath = Path.Combine(tools, "live-cache.json");
        if (!offline)
        {
            try
            {
                var fresh = await Fetch();
                File.WriteAllText(cachePath, fresh.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
            }
            catch (Exception e) when (File.Exists(cachePath))
            {
                Console.WriteLine($"Using cached data, fetch failed: {e.Message}");
            }
        }
        return Parse(JsonNode.Parse(File.ReadAllText(cachePath))!);
    }

    static async Task<JsonObject> Fetch()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("seifhesham22-profile");
        if (Environment.GetEnvironmentVariable("GITHUB_TOKEN") is { Length: > 0 } token)
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var listed = JsonNode.Parse(await http.GetStringAsync($"https://api.github.com/users/{Login}/repos?per_page=100&sort=pushed"))!.AsArray();
        var repos = new JsonArray();
        long csharp = 0, all = 0;
        foreach (var repo in listed)
        {
            var name = repo!["name"]!.GetValue<string>();
            if (repo["fork"]!.GetValue<bool>() || name == Login) continue;
            repos.Add(new JsonObject { ["name"] = name, ["pushedAt"] = repo["pushed_at"]!.GetValue<string>() });
            var languages = JsonNode.Parse(await http.GetStringAsync($"https://api.github.com/repos/{Login}/{name}/languages"))!.AsObject();
            foreach (var (language, bytes) in languages)
            {
                all += bytes!.GetValue<long>();
                if (language == "C#") csharp += bytes.GetValue<long>();
            }
        }

        // The contribution calendar is public HTML. No token needed, and it matches what visitors see.
        using var anonymous = new HttpClient();
        anonymous.DefaultRequestHeaders.UserAgent.ParseAdd("seifhesham22-profile");
        var html = await anonymous.GetStringAsync($"https://github.com/users/{Login}/contributions");

        var counts = Regex.Matches(html, """<tool-tip[^>]*for="(contribution-day-component-\d+-\d+)"[^>]*>([^<]+)<""")
            .ToDictionary(m => m.Groups[1].Value, m => Regex.Match(m.Groups[2].Value, @"^(\d+) contribution") is { Success: true } c ? int.Parse(c.Groups[1].Value) : 0);
        var days = new JsonArray();
        foreach (var cell in Regex.Matches(html, """data-date="([\d-]+)"\s+id="(contribution-day-component-\d+-\d+)"\s+data-level="(\d)""").OrderBy(m => m.Groups[1].Value))
            days.Add($"{cell.Groups[1].Value} {cell.Groups[3].Value} {counts.GetValueOrDefault(cell.Groups[2].Value)}");
        if (days.Count < 300) throw new InvalidDataException($"Expected a year of contribution cells, found {days.Count}");

        return new JsonObject { ["cSharpShare"] = all == 0 ? 0 : (int)Math.Round(100.0 * csharp / all), ["repos"] = repos, ["days"] = days };
    }

    static LiveData Parse(JsonNode json)
    {
        var repos = json["repos"]!.AsArray().Select(r => new Repo(r!["name"]!.GetValue<string>(), r["pushedAt"]!.GetValue<DateTime>().ToUniversalTime())).ToList();
        var days = json["days"]!.AsArray().Select(d =>
        {
            var parts = d!.GetValue<string>().Split(' ');
            return new Contribution(DateOnly.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }).ToList();
        return new LiveData(repos, days, json["cSharpShare"]!.GetValue<int>());
    }
}
