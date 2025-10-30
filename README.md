# 🎮 Color Chain Reaction

A level-based hyper-casual puzzle game with strategic move limits and satisfying chain reactions.

## Game Design

- **Genre:** Puzzle / Match-3 Hybrid
- **Platform:** Mobile (iOS/Android)
- **Orientation:** Portrait
- **Core Loop:** Match colored balls → Trigger chains → Clear levels with limited moves

## Key Features

- **50+ Unique Levels** with progressive difficulty
- **PowerUps System** (Bomb, Rainbow, Shuffle)
- **Move-Limited Strategy** - Think before you swipe!
- **Chain Reaction Mechanics** - One move, multiple explosions
- **Star Rating System** - 1-3 stars per level
- **Score & Combo System** with satisfying feedback

## Tech Stack

- **Unity 6000.2.7f2 LTS** (URP)
- **Addressables** - Dynamic asset loading
- **UniTask** - High-performance async operations
- **Event-Driven Architecture** - ScriptableObject Event Channels
- **Design Patterns:** Object Pool, Observer, Factory, Singleton

## Project Architecture
```
├── Core/
│   ├── EventChannels/    # Decoupled communication
│   ├── Interfaces/       # Contract definitions
│   └── Patterns/         # Singleton, Pool, Factory
├── Systems/
│   ├── Grid/            # 8x8 grid management
│   ├── Match/           # Flood-fill algorithm
│   ├── Level/           # Level progression
│   └── Score/           # Scoring & combos
├── Gameplay/
│   ├── Ball/            # Ball behavior
│   └── PowerUps/        # PowerUp implementations
└── Data/
    ├── Levels/          # ScriptableObject level configs
    └── Balls/           # Ball data configs
```

## Gameplay Mechanics

### Core Rules
- 8x8 grid with 5 different colored balls
- Swipe to move balls (only to adjacent empty cells)
- Match 3+ same color → Explosion!
- Limited moves per level
- Chain reactions give bonus points

### PowerUps
- **Bomb** - Destroys 3x3 area (5+ match)
- **Rainbow** - Matches any color (horizontal/vertical line)
- **Shuffle** - Randomizes the board (limited uses)

### Progression
- Level 1-10: Tutorial (Easy - 3 colors)
- Level 11-25: Medium (4 colors, obstacles)
- Level 26-50: Hard (5 colors, limited moves, special blocks)

## Getting Started

1. Clone the repository
2. Open with **Unity 2022.3 LTS** or newer
3. Install required packages (Addressables, UniTask)
4. Open `Assets/_Project/Scenes/_Production/Boot.unity`
5. Press Play!

## Development Practices

- **SOLID Principles**
- **Clean Code** with XML documentation
- **Event-Driven** architecture
- **Addressable** asset management
- **Object Pooling** for performance
- **UniTask** for zero-allocation async

## 👤 Developer

**MEHMET KOMAN**
- GitHub: @KomanMehmet (https://github.com/KomanMehmet?tab=repositories)
- LinkedIn: https://www.linkedin.com/in/mehmet-koman-gamedev92/

---

⭐ **Made with Unity & UniTask**
