using Gurobi;
using Solver.Gurobi;
using Solver.Lib;

using static Solver.Runner.ModelHelper;

namespace Solver.Runner;

public class ExtendedEdgeModel(int k) : GurobiModel
{
    public override GRBModel CreateModel(GRBEnv env, bool[,] A, double[,] w)
    {
        var n = A.LengthI();

        var xTemplate = Duplicate(A, n);
        for (int l = 0; l < n; l++)
        for (int i = 0; i < l; i++)
        for (int j = 0; j < n; j++)
        {
            // for each subgraph l, disable all edges to and from nodes i < l
            xTemplate[i, j, l] = false;
            xTemplate[j, i, l] = false;
        }

        var problem = new GRBModel(env);
        
        var x = problem.AddBinaryVars(xTemplate);

        var xSumOverJ = x.SumOverJ();

        // 9a
        problem.SetObjective(-SumSum(w, x.SumOverK()));

        // 9b
        problem.AddConstrs(x.SumOverI(), GRB.EQUAL, xSumOverJ, "9b");
        // 9c
        problem.AddConstrs(x.SumSumOverJK(), GRB.LESS_EQUAL, 1, "9c");
        // 9d
        problem.AddConstrs(x.SumSumOverIJ(), GRB.LESS_EQUAL, k, "9d");
        // 9e
        for (int l = 0; l < n; l++)
        for (int i = l + 1; i < n; i++)
            problem.AddConstr(xSumOverJ[i, l] <= xSumOverJ[l, l], $"9e^{l}_{i}");
        
        return problem;
    }
}
