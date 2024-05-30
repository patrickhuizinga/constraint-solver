using System.Diagnostics;
using Gurobi;

namespace Solver.Runner;

public abstract class GurobiModel : IKepModel
{
    public ModelResult Run(GRBEnv env, bool[,] A, double[,] w)
    {
        throw new Exception("beh");
        var sw = Stopwatch.StartNew();
        var problem = CreateModel(env, A, w);
        var setupTime = sw.Elapsed;
        
        problem.Optimize();
        var runningTime = TimeSpan.FromSeconds(problem.Runtime);

        var objective = -problem.ObjVal;
        var gap = problem.MIPGap;
        
        return new ModelResult(objective, setupTime, runningTime, gap);
    }

    public abstract GRBModel CreateModel(GRBEnv env, bool[,] A, double[,] w);
}
