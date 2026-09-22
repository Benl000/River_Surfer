# Game Design Document — River Surfer

| | |
|---|---|
| **Working title** | River Surfer |
| **Team** | Ben Lutenberg (204713945), Peleg Wortzel (209126275) |
| **Genre** | 3-Lane Endless Runner / Arcade score-chaser |
| **Target platform** | PC (Windows) + Mobile (Android), standalone builds |
| **Engine / Unity version** | Unity 6 (6000.3.12f1), URP, 3D |
| **Orientation & reference resolution** | Landscape, 1920 × 1080 reference |
| **Expected session length** | 1 – 3 minutes per run |
| **Document version** | v1.0 — 2026-09-22 |

---

## 1. High Concept

The player controls a speedboat moving forward on a 3-lane river. The water uses a scrolling shader, while obstacles and coins translate towards the camera. The player instantly snaps between left, center, and right lanes to dodge. Collecting coins increases the global speed. Touch an obstacle, and the run ends instantly. Restart takes under one second.

### Design pillars

1. **Deterministic dodging** — Lane switching snaps precisely to fixed coordinates. No physics-based sliding, ensuring every collision is strictly the player's fault.
2. **Readability above all** — Obstacles (buoys, rocks) must contrast sharply with the stylized water shader so the player can plan moves ahead, even at high speeds.
3. **Pure arcade loop** — Fast restarts, zero downtime. This is why there is no complex meta-progression, inventory management, or story.

---

## 2. Reference & Inspiration

![Concept Art / Moodboard](images/moodboard_placeholder.png)

- **Primary reference:** Subway Surfers. Taking: 3-lane switching mechanics, forward-scrolling illusion. Not taking: meta-progression, coin-based shop, power-ups, police chase mechanics.
- **Visual reference:** Kenney.nl Watercraft Pack / Low-poly stylized aesthetics.
- **Video:** [Subway Surfers Gameplay Reference](https://www.youtube.com/watch?v=fwaXGAhEKGA)

---

## 3. Core Game Loop

```mermaid
stateDiagram-v2
    [*] --> MainMenu
    MainMenu --> Playing: click start
    Playing --> GameOver: collision with obstacle
    GameOver --> Playing: click retry (after 1.0 s lockout)
