<div align="center">

<a href="https://github.com/seifhesham22?tab=repositories"><img alt="Seif Hesham. Backend developer building .NET systems that stay legible as they grow. An animated request travels through endpoint, MediatR, handler, domain, EF Core and PostgreSQL, and a 200 OK comes back." src="assets/hero.svg" width="804"></a>

</div>

I'm Seif, a backend developer working mostly in C# and .NET. I care about systems where the boundaries are obvious: modular monoliths, vertical slices, and services that talk through events.

Right now I'm building [TaskTracker](https://github.com/seifhesham22/TaskTracker), a GraphQL API with web and mobile clients, and a [desktop client](https://github.com/seifhesham22/PingPongDesktopApp) for PingPong in Avalonia.

<div align="center">

<a href="https://github.com/seifhesham22?tab=repositories"><img alt="A dotnet watch log: listening on github.com/seifhesham22, the most recent push, the main stack, and contributions in the last year." src="assets/terminal.svg" width="804"></a>

</div>

### Things I've built

Each card replays what the project actually does. Click one to open the repo.

<div align="center">

<a href="https://github.com/seifhesham22/UniversityAssetManagementSystem-UAMS"><img alt="UAMS. University asset management with room layouts, maintenance tickets and a VK bot. A ticket moves through its state machine from Open to Closed." src="assets/uams.svg" width="400"></a> <a href="https://github.com/seifhesham22/tsu_admission_backend_2"><img alt="Admissions. Four services with their own databases exchanging events over RabbitMQ. A redelivered message changes nothing." src="assets/admissions.svg" width="400"></a> <a href="https://github.com/seifhesham22/PingPongBackend"><img alt="PingPong. A Discord-style chat backend. A message goes in over HTTP and SignalR pushes it to every device of both participants, above a 16-flag permission bitfield." src="assets/pingpong.svg" width="400"></a> <a href="https://github.com/seifhesham22/PingPongDesktopApp"><img alt="PingPong Desktop. An Avalonia client in progress, replaying its sign-in flow: splash, loading dots, a shake on failure, then signed in." src="assets/desktop.svg" width="400"></a> <a href="https://github.com/seifhesham22/TaskTracker"><img alt="TaskTracker. A task board with a GraphQL API and React and Expo clients. A task moves across the ToDo, InProgress and Done columns." src="assets/tasktracker.svg" width="400"></a> <a href="https://github.com/seifhesham22/RestaurantBackend"><img alt="RestaurantBackend. A clean architecture API where the API, Infrastructure and test projects all depend inward on Core." src="assets/restaurant.svg" width="400"></a>

</div>

<details>
<summary><b>How each one is put together</b></summary>
<br>

- **[UAMS](https://github.com/seifhesham22/UniversityAssetManagementSystem-UAMS).** One deployable split into modules that own their data and talk through interfaces, never each other's tables. Tickets run on a 12-state machine with its transition rules on the entity, and unit tests cover them.
- **[Admissions](https://github.com/seifhesham22/tsu_admission_backend_2).** Identity, Admission, Files and a notifications worker, one database each. RabbitMQ delivers at least once, so consumers use existence checks and an `OccurredAtUtc` comparison to make redelivery harmless.
- **[PingPong](https://github.com/seifhesham22/PingPongBackend).** Each feature is one folder holding its request, handler, DTOs and route, with no repository or service layer. Roles carry a 16-flag bitfield and a rank, and members can only act on roles ranked below them.
- **[PingPong Desktop](https://github.com/seifhesham22/PingPongDesktopApp).** Avalonia and MVVM. Early, but the screens are real, down to a custom `LoadingDots` control and a `ShakeBehavior` for failed attempts.
- **[TaskTracker](https://github.com/seifhesham22/TaskTracker).** Hot Chocolate GraphQL with DataLoaders, plus React and Expo clients in one repo. A task's status is the category of its column, so moving a task is how its status changes.
- **[RestaurantBackend](https://github.com/seifhesham22/RestaurantBackend).** Layered clean architecture with JWT and refresh tokens, Serilog, and service tests written with xUnit and Moq.

</details>

### What I reach for

<div align="center">

<a href="https://github.com/seifhesham22?tab=repositories"><img alt="Stack, in the order a request meets it. Client: Avalonia, React, Expo. Edge: ASP.NET Core, Hot Chocolate, SignalR. Auth: Identity, JWT and refresh, 2FA. App: MediatR, FluentValidation. Data: EF Core, PostgreSQL, SQL Server, S3. Events: RabbitMQ, MassTransit. Ship: Docker, GitHub Actions, xUnit, Serilog." src="assets/stack.svg" width="400"></a> <a href="https://github.com/seifhesham22?tab=repositories"><img alt="Patterns and where they show up. Modular monolith: UAMS. Database per service: Admissions. Vertical slices: UAMS, PingPong, TaskTracker. Rich domain model: UAMS, PingPong, Admissions. Idempotent consumers: Admissions. Clean architecture: RestaurantBackend. Results over exceptions: PingPong." src="assets/patterns.svg" width="400"></a>

</div>

### Activity

<div align="center">

<a href="https://github.com/seifhesham22"><img alt="Heatmap of contributions over the last 12 months, with total contributions, active days, longest streak and share of public code written in C#. The numbers are in the table below." src="assets/activity.svg" width="804"></a>

</div>

<details>
<summary><b>The numbers by month</b></summary>

<!-- activity-table:start -->

| Month | Contributions |
| --- | ---: |
| Nov 2025 | 0 |
| Dec 2025 | 0 |
| Jan 2026 | 48 |
| Feb 2026 | 28 |
| Mar 2026 | 4 |
| Apr 2026 | 13 |
| May 2026 | 96 |
| Jun 2026 | 50 |
| Jul 2026 | 150 |
| Aug 2026 | 177 |
| Sep 2026 | 3 |
| Oct 2026 | 35 |
| Nov 2026 | 56 |
| **Total** | **660** |

<!-- activity-table:end -->

</details>

<br>

<sub>This page is generated by [a C# script](tools/build.cs) and refreshed daily by a GitHub Action. It adapts to your theme and screen size. Set in Bricolage Grotesque and Cascadia Mono.</sub>
