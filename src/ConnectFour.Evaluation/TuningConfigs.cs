using ConnectFour.AI;

namespace ConnectFour.Evaluation
{
    // Minimax configuration for evaluation
    public sealed record MinimaxConfig(string Label, int Depth, string WeightGroup, HeuristicWeights Weights);

    // MCTS configiguration for evaluation
    public sealed record MctsConfig(string Label, int Iterations, double ExplorationConstant);
}
