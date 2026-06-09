# Ultimate 2D Movement Controller

A highly responsive and modular 2D character controller built in Unity, focused on modern platformer movement mechanics, gameplay feel, and clean gameplay programming architecture.
This project explores the design and implementation of advanced movement systems commonly used in polished platformer games, including responsive jumping, dashing, wall interactions, momentum control, and extensible state-based movement logic.

Built with scalability and readability in mind, the controller separates input handling, movement logic, physics, and abilities into reusable systems, making it suitable for experimentation, prototyping, and future gameplay research.

---

## Movement Showcase

### Coyote Time

Allows the player to jump for a brief moment after leaving a platform, improving responsiveness and reducing frustrating missed jumps.

<p align="center">
  <img src="Media/gifs/Coyote Time.gif" width="500">
</p>

### Jump Buffering

Stores jump input shortly before landing and executes the jump automatically when the player touches the ground.

<p align="center">
  <img src="Media/gifs/Jump buffer.gif" width="500">
</p>

### Variable Jump Height

Jump height depends on how long the jump button is held, enabling more precise control.

<p align="center">
  <img src="Media/gifs/Variable Jump Height.gif" width="500">
</p>

### Multiple Jumps

Supports configurable air jumps for increased mobility.

<p align="center">
  <img src="Media/gifs/Multiple Jumps.gif" width="500">
</p>

### Fast Fall

Increases downward acceleration while airborne, allowing faster descents and more responsive aerial control.

<p align="center">
  <img src="Media/gifs/Fast Fall.gif" width="500">
</p>

### Jump Trajectory Visualization

Displays predicted jump trajectories to assist with movement tuning and debugging.

<p align="center">
  <img src="Media/gifs/Trajectory Lines.gif" width="500">
</p>

---

## Features

### Implemented

- Responsive horizontal movement
- Momentum preservation when changing direction
- Variable jump height
- Coyote time
- Jump buffering
- Multiple jumps
- Fast fall
- Jump trajectory visualization
- One-way platforms
- Unity Input System integration
- Debug-friendly development workflow

### Planned

- Slope handling
- Moving platforms handling
- Wall slide
- Wall jump
- Air dash
---

## Technologies

- Unity 6
- C#
- Unity Input System
- Physics-based movement
- Finite State Machines (FSM)

---

## Goals

The purpose of this repository is to:

- Study responsive movement design in platformer games
- Explore scalable gameplay programming architecture
- Experiment with advanced movement mechanics
- Build a polished gameplay programming portfolio project

---
