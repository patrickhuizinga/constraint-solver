using Gurobi;

namespace Solver.Runner;

public interface IKepModel
{
    ModelResult Run(GRBEnv env, bool[,] A, double[,] w);
}

public record ModelResult(double Objective, TimeSpan SetupTime, TimeSpan RunningTime, double ObjectiveGap);
