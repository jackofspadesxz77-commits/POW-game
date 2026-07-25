"""
Prisoner's Dilemma Game Simulator

A game theory implementation simulating repeated rounds of the Prisoner's Dilemma
with various AI strategies competing against each other.
"""

from enum import Enum
from typing import Dict, List, Tuple
from dataclasses import dataclass
from abc import ABC, abstractmethod


class Choice(Enum):
    """Available choices for each player"""
    COOPERATE = "Cooperate"
    DEFECT = "Defect"


@dataclass
class GameResult:
    """Result of a single round"""
    player1_choice: Choice
    player2_choice: Choice
    player1_score: int
    player2_score: int


class Strategy(ABC):
    """Abstract base class for player strategies"""
    
    def __init__(self, name: str):
        self.name = name
        self.history: List[Choice] = []
        self.score = 0
        self.opponent_history: List[Choice] = []
    
    @abstractmethod
    def choose(self) -> Choice:
        """Decide whether to cooperate or defect"""
        pass
    
    def record_round(self, my_choice: Choice, opponent_choice: Choice, my_score: int):
        """Record the result of a round"""
        self.history.append(my_choice)
        self.opponent_history.append(opponent_choice)
        self.score += my_score
    
    def reset(self):
        """Reset strategy for a new game"""
        self.history = []
        self.opponent_history = []
        self.score = 0


class TitForTat(Strategy):
    """Copy opponent's last move"""
    
    def __init__(self):
        super().__init__("Tit for Tat")
    
    def choose(self) -> Choice:
        if not self.opponent_history:
            return Choice.COOPERATE
        return self.opponent_history[-1]


class AlwaysCooperate(Strategy):
    """Always cooperate"""
    
    def __init__(self):
        super().__init__("Always Cooperate")
    
    def choose(self) -> Choice:
        return Choice.COOPERATE


class AlwaysDefect(Strategy):
    """Always defect"""
    
    def __init__(self):
        super().__init__("Always Defect")
    
    def choose(self) -> Choice:
        return Choice.DEFECT


class Random(Strategy):
    """Make random choices"""
    
    def __init__(self):
        super().__init__("Random")
    
    def choose(self) -> Choice:
        import random
        return random.choice([Choice.COOPERATE, Choice.DEFECT])


class Grudger(Strategy):
    """Cooperate until opponent defects, then always defect"""
    
    def __init__(self):
        super().__init__("Grudger")
    
    def choose(self) -> Choice:
        if Choice.DEFECT in self.opponent_history:
            return Choice.DEFECT
        return Choice.COOPERATE


class PrisonersDilemma:
    """Main game engine for Prisoner's Dilemma"""
    
    # Payoff matrix: (player1_score, player2_score)
    PAYOFF_MATRIX = {
        (Choice.COOPERATE, Choice.COOPERATE): (3, 3),
        (Choice.COOPERATE, Choice.DEFECT): (0, 5),
        (Choice.DEFECT, Choice.COOPERATE): (5, 0),
        (Choice.DEFECT, Choice.DEFECT): (1, 1),
    }
    
    def __init__(self, rounds: int = 10):
        self.rounds = rounds
        self.game_history: List[GameResult] = []
    
    def play_round(self, player1: Strategy, player2: Strategy) -> GameResult:
        """Play a single round between two players"""
        choice1 = player1.choose()
        choice2 = player2.choose()
        
        score1, score2 = self.PAYOFF_MATRIX[(choice1, choice2)]
        
        result = GameResult(choice1, choice2, score1, score2)
        self.game_history.append(result)
        
        player1.record_round(choice1, choice2, score1)
        player2.record_round(choice2, choice1, score2)
        
        return result
    
    def play_game(self, player1: Strategy, player2: Strategy) -> Tuple[int, int]:
        """Play a complete game (multiple rounds) between two players"""
        player1.reset()
        player2.reset()
        self.game_history = []
        
        for _ in range(self.rounds):
            self.play_round(player1, player2)
        
        return player1.score, player2.score
    
    def print_results(self, player1: Strategy, player2: Strategy):
        """Print detailed results of the last game"""
        print(f"\n{'='*60}")
        print(f"{player1.name} vs {player2.name}")
        print(f"{'='*60}")
        print(f"{'Round':<8} {'Player1':<15} {'Player2':<15} {'Score1':<8} {'Score2':<8}")
        print(f"{'-'*60}")
        
        cumulative_score1 = 0
        cumulative_score2 = 0
        
        for i, result in enumerate(self.game_history, 1):
            cumulative_score1 += result.player1_score
            cumulative_score2 += result.player2_score
            print(f"{i:<8} {result.player1_choice.value:<15} {result.player2_choice.value:<15} "
                  f"{cumulative_score1:<8} {cumulative_score2:<8}")
        
        print(f"{'-'*60}")
        print(f"Final Score: {player1.name}: {player1.score} | {player2.name}: {player2.score}")
        print(f"{'='*60}\n")


def run_tournament():
    """Run a round-robin tournament with all strategies"""
    strategies = [
        TitForTat(),
        AlwaysCooperate(),
        AlwaysDefect(),
        Random(),
        Grudger(),
    ]
    
    game = PrisonersDilemma(rounds=20)
    scores: Dict[str, int] = {strategy.name: 0 for strategy in strategies}
    
    print("\n" + "="*60)
    print("PRISONER'S DILEMMA TOURNAMENT")
    print("="*60 + "\n")
    
    # Play all matchups
    for i, player1 in enumerate(strategies):
        for player2 in strategies[i+1:]:
            score1, score2 = game.play_game(player1, player2)
            scores[player1.name] += score1
            scores[player2.name] += score2
            print(f"{player1.name:<20} vs {player2.name:<20} : "
                  f"{score1:>3} - {score2:<3}")
    
    # Print final rankings
    print("\n" + "="*60)
    print("FINAL RANKINGS")
    print("="*60)
    
    sorted_scores = sorted(scores.items(), key=lambda x: x[1], reverse=True)
    for rank, (strategy, score) in enumerate(sorted_scores, 1):
        print(f"{rank}. {strategy:<20} - Score: {score}")
    
    print("="*60 + "\n")


if __name__ == "__main__":
    run_tournament()
