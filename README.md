# POW Game - Prisoner's Dilemma Simulator

A game theory implementation simulating the Prisoner's Dilemma with various AI strategies.

## Overview

This project implements the classic Prisoner's Dilemma game where two players interact repeatedly, choosing to either **Cooperate** or **Defect**. Each choice combination results in different payoffs.

## Payoff Matrix

|   | Cooperate | Defect |
|---|-----------|--------|
| **Cooperate** | (3, 3) | (0, 5) |
| **Defect** | (5, 0) | (1, 1) |

## Features

- Multiple AI strategy implementations
- Round-robin tournament
- Score tracking and statistics
- Game history logging

## Strategies

- **TitForTat** - Copy opponent's last move
- **AlwaysCooperate** - Always cooperate
- **AlwaysDefect** - Always defect
- **Random** - Random choice
- **Grudger** - Defect after opponent defects

## Getting Started

```bash
python pow_game.py
```

## Game Rules

1. Players play multiple rounds against each other
2. Each round, both players simultaneously choose to Cooperate or Defect
3. Payoffs are awarded based on both players' choices
4. After each game, strategies are ranked by total score
