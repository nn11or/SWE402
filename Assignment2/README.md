# Assignment 2: 2D Roguelike Game

SWE 402 - Game Development

## Description
A turn-based 2D roguelike game built in Unity 6 with procedural level generation, enemies, and resource management.

## Features Implemented

### Core (Sections 1-6)
- Procedural 8x8 board generation with random ground/wall tiles
- Grid-based player movement with passability checks
- Turn system with event-driven enemy AI
- Food resource system with UI display
- Destructible walls and enemy combat
- Exit cell for level progression
- Game over / restart system

### Additional Requirements (Section 7)
- **Audio**: Background music via AudioSource (looped, reduced volume) + SFX via PlayOneShot for all game events
- **Visual Effects**: ParticleSystem effects for wall destruction, food collection, and enemy death
- **Smooth Movement**: Coroutine-based lerp movement between cells with input blocking
- **Object Pooling**: Pre-instantiated pools for CellObjects; recycled via SetActive on level transitions
- **Code Standards**: [SerializeField] on all inspector-exposed privates, [Range] and [Tooltip] on tuning variables

## How to Play
- **Arrow Keys / WASD**: Move the player
- **Bump into walls**: Attack walls (destroy after multiple hits)
- **Bump into enemies**: Attack enemies
- **Collect food**: Walk over food items to gain food points
- **Reach the exit**: Proceed to the next level
- **Press Enter**: Restart after Game Over

## Build Targets
- Windows (standalone .exe)
- macOS (.app)
- WebGL (index.html)
