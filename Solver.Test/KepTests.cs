using Gurobi;
using Solver.Gurobi;
using Solver.Lib;

namespace Solver.Test;

public class KepTests
{
    [Test]
    [TestCase(10, 3, 0.2, 2.8962)]
    [TestCase(11, 3, 0.2, 2.9376)]
    [TestCase(12, 3, 0.2, 3.2928)]
    [TestCase(13, 3, 0.2, 3.4310)]
    [TestCase(14, 3, 0.2, 4.5519)]
    [TestCase(15, 3, 0.2, 4.5519)]
    [TestCase(18, 3, 0.2, 6.7845)]
    [TestCase(10, 3, 0.5, 5.7710)]
    [TestCase(10, 4, 0.2, 3.6748)]
    [TestCase(10, 4, 0.5, 5.8556)]
    public void TinyLowDensityAssignment(int n, int k, double density, double expectedObjective)
    {
        var (A, w) = CreateCompatibility(n, density, 42);
        var problem = CreateEdgeAssignment(A, w, k);

        // problem.PreSolve();

        var solution = problem.Minimize();
        
        var checker = CreateEdgeAssignment(A, w, k);
        Assert.That(checker.Validate(solution), "Solution is not valid according to checker§");

        Console.WriteLine("objective: " + -solution.GetObjectiveValue());
        Assert.That(-solution.GetObjectiveValue(), Is.EqualTo(expectedObjective).Within(0.0001));
    }

    [Test]
    [TestCase(10, 3, 0.2, 2.8962)]
    [TestCase(15, 3, 0.2, 4.5519)]
    [TestCase(20, 3, 0.2, 9.8199)]
    [TestCase(25, 3, 0.2, 13.3576)]
    [TestCase(30, 3, 0.2, 19.0218)]
    
    [TestCase(10, 3, 0.5, 5.7710)]
    [TestCase(20, 3, 0.5, 15.3730)]
    [TestCase(30, 3, 0.5, 24.3331)]
    
    [TestCase(10, 4, 0.2, 3.6748)]
    [TestCase(20, 4, 0.2, 11.5086)]
    [TestCase(30, 4, 0.2, 21.0538, Ignore = "> 1 min")]
    
    [TestCase(10, 4, 0.5, 5.8556)]
    [TestCase(20, 4, 0.5, 16.5220)]
    [TestCase(30, 4, 0.5, 25.4677, Ignore = "> 2 min")]
    
    [TestCase(30, 5, 0.2, 0)]
    [TestCase(30, 5, 0.5, 0)]
    [TestCase(40, 5, 0.5, 0)]
    [TestCase(40, 3, 0.5, 0)]
    public void AssignmentGurobi(int n, int k, double density, double expectedObjective)
    {
        var (A, w) = CreateCompatibility(n, density, 42);
        var problem = CreateEdgeAssignment(A, w, k);

        var objective = problem.GurobiSolve();

        Console.WriteLine("objective: " + -objective);
        Assert.That(-objective, Is.EqualTo(expectedObjective).Within(0.0001));
    }

    [Test]
    [TestCase(10, 3, 0.2, 2.8962)]
    [TestCase(11, 3, 0.2, 2.9376)]
    [TestCase(12, 3, 0.2, 3.2928)]
    [TestCase(13, 3, 0.2, 3.4310)]
    [TestCase(14, 3, 0.2, 4.5519)]
    [TestCase(15, 3, 0.2, 4.5519)]
    [TestCase(18, 3, 0.2, 6.7845, Ignore = "takes many minutes")]
    [TestCase(10, 3, 0.5, 5.7710)]
    [TestCase(10, 4, 0.2, 3.6748)]
    [TestCase(10, 4, 0.5, 5.8556, Ignore = "Takes 1:50 min")]
    public void TinyLowDensityExtended(int n, int k, double density, double expectedObjective)
    {
        var (A, w) = CreateCompatibility(n, density, 42);
        var problem = CreateExtended(A, w, k);

        // problem.PreSolve();
        var solution = problem.Minimize();

        Console.WriteLine("objective: " + -solution.GetObjectiveValue());
        Assert.That(-solution.GetObjectiveValue(), Is.EqualTo(expectedObjective).Within(0.0001));
    }

    [Test]
    [TestCase(10, 3, 0.2, 2.8962)]
    [TestCase(15, 3, 0.2, 4.5519)]
    [TestCase(20, 3, 0.2, 9.8199)]
    [TestCase(25, 3, 0.2, 13.3576)]
    [TestCase(30, 3, 0.2, 19.0218)]
    
    [TestCase(10, 3, 0.5, 5.7710)]
    [TestCase(20, 3, 0.5, 15.3730)]
    [TestCase(30, 3, 0.5, 24.3331)]
    
    [TestCase(10, 4, 0.2, 3.6748)]
    [TestCase(20, 4, 0.2, 11.5086)]
    [TestCase(30, 4, 0.2, 21.0538)]
    
    [TestCase(10, 4, 0.5, 5.8556)]
    [TestCase(20, 4, 0.5, 16.5220)]
    [TestCase(30, 4, 0.5, 25.4677)]
    
    [TestCase(30, 5, 0.2, 0)]
    [TestCase(30, 5, 0.5, 0)]
    [TestCase(40, 5, 0.5, 0)]
    [TestCase(40, 3, 0.5, 0)]
    public void ExtendedGurobi(int n, int k, double density, double expectedObjective)
    {
        var (A, w) = CreateCompatibility(n, density, 42);
        var problem = CreateExtendedGrb(A, w, k);
        
        problem.Optimize();
        var objective = problem.ObjVal;

        Console.WriteLine("objective: " + -objective);
        Assert.That(-objective, Is.EqualTo(expectedObjective).Within(0.0001));
    }

    [Test]
    [TestCase(10, 3, 0.2, 2.8962)]
    [TestCase(11, 3, 0.2, 2.9376)]
    [TestCase(12, 3, 0.2, 3.2928)]
    [TestCase(13, 3, 0.2, 3.4310)]
    [TestCase(14, 3, 0.2, 4.5519)]
    [TestCase(15, 3, 0.2, 4.5519)]
    [TestCase(18, 3, 0.2, 6.7845)]
    [TestCase(10, 3, 0.5, 5.7710)]
    [TestCase(10, 4, 0.2, 3.6748)]
    [TestCase(10, 4, 0.5, 5.8556)]
    public void TinyLowDensityPersonalIdea(int n, int k, double density, double expectedObjective)
    {
        var (A, w) = CreateCompatibility(n, density, 42);
        var problem = CreatePersonalIdea(A, w, k);

        // problem.PreSolve();

        var solution = problem.Minimize();
        
        var checker = CreatePersonalIdea(A, w, k);
        Assert.That(checker.Validate(solution), "Solution is not valid according to checker");

        Console.WriteLine("objective: " + -solution.GetObjectiveValue());
        Assert.That(-solution.GetObjectiveValue(), Is.EqualTo(expectedObjective).Within(0.0001));
    }

    [Test]
    [TestCase(10, 3, 0.2, 2.8962)]
    [TestCase(15, 3, 0.2, 4.5519)]
    [TestCase(20, 3, 0.2, 9.8199)]
    [TestCase(25, 3, 0.2, 13.3576)]
    [TestCase(30, 3, 0.2, 19.0218)]
    
    [TestCase(10, 3, 0.5, 5.7710)]
    [TestCase(20, 3, 0.5, 15.3730)]
    [TestCase(30, 3, 0.5, 24.3331)]
    
    [TestCase(10, 4, 0.2, 3.6748)]
    [TestCase(20, 4, 0.2, 11.5086)]
    [TestCase(30, 4, 0.2, 21.0538)]
    
    [TestCase(10, 4, 0.5, 5.8556)]
    [TestCase(20, 4, 0.5, 16.5220)]
    [TestCase(30, 4, 0.5, 25.4677, Ignore = "~3 min")]
    
    [TestCase(30, 5, 0.2, 0)]
    [TestCase(30, 5, 0.5, 0)]
    [TestCase(40, 5, 0.5, 0)]
    [TestCase(40, 3, 0.5, 0)]
    public void PersonalGurobi(int n, int k, double density, double expectedObjective)
    {
        var (A, w) = CreateCompatibility(n, density, 42);
        var problem = CreatePersonalGrb(A, w, k);

        problem.Optimize();
        var objective = problem.ObjVal;

        Console.WriteLine("objective: " + -objective);
        Assert.That(-objective, Is.EqualTo(expectedObjective).Within(0.0001));
    }

    private static (bool[,] A, double[,] w) CreateCompatibility(int n, double density, int seed)
    {
        var A = new bool[n, n];
        var w = new double[n, n];
        var rng = new Random(seed);
        
        // create the array by start top left and then expand to the bottom right by adding 'rings'
        // that way you ensure a larger size always improves the objective
        for (int i = 0; i < n; i++)
        for (int j = 0; j < i; j++)
        {
            if (rng.NextDouble() < density)
            {
                A[i, j] = true;
                w[i, j] = rng.NextDouble();
            }

            if (rng.NextDouble() < density)
            {
                A[j, i] = true;
                w[j, i] = rng.NextDouble();
            }
        }

        return (A, w);
    }

    private static IntegerProblem CreateEdgeAssignment(bool[,] A, double[,] w, int k)
    {
        var n = A.LengthI();
        var L = Enumerable.Range(0, n).ToList();

        var problem = new IntegerProblem();
        // 6i
        var x = problem.AddBinaryVariables(A);
        // todo don't create y_il where d_il + d_li > k
        // 6h
        var y = problem.AddBinaryVariables(TrueUpper(n));

        // 6a
        problem.Objective = -SumSum(w, x);

        // 6b
        problem.AddConstraints(x.SumOverI(), Comparison.Equals, x.SumOverJ());
        // 6c
        problem.AddConstraints(x.SumOverJ(), Comparison.LessEqual, 1);
        // 6b & 6c imply
        problem.AddConstraints(x.SumOverI(), Comparison.LessEqual, 1);
        // 6d
        problem.AddConstraints(y.SumOverI(), Comparison.LessEqual, k);
        // 6e
        problem.AddConstraints(y.SumOverJ(), Comparison.Equals, x.SumOverJ());
        // 6b & 6e imply
        problem.AddConstraints(y.SumOverJ(), Comparison.LessEqual, 1);
        // problem.AddConstraints(y.SumOverJ(), Comparison.Equals, 1);
        // 6f
        foreach (var l in L)
        foreach (var (i, j) in Indices(A))
        {
            if (V(l).Contains(i))
                problem.AddConstraint(y[i, l] + x[i, j] <= 1 + y[j, l]);
        }

        // 6g
        foreach (var l in L)
        foreach (var i in V(l))
            problem.AddConstraint(y[i, l] <= y[l, l]);

        return problem;

        // todo actually restrict and apply everywhere i and l are used in combination
        IEnumerable<int> V(int l) => Enumerable.Range(0, n);
    }

    private static IntegerProblem CreateExtended(bool[,] A, double[,] w, int k)
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
        
        var problem = new IntegerProblem();
        var x = problem.AddBinaryVariables(xTemplate);

        var xSumOverJ = x.SumOverJ();

        // 9a
        problem.Objective = -SumSum(w, x.SumOverK());

        // 9b
        problem.AddConstraints(x.SumOverI(), Comparison.Equals, xSumOverJ);
        // 9c
        problem.AddConstraints(x.SumSumOverJK(), Comparison.LessEqual, 1);
        // 9d
        problem.AddConstraints(x.SumSumOverIJ(), Comparison.LessEqual, k);
        // 9e
        for (int l = 0; l < n; l++)
        for (int i = l + 1; i < n; i++)
            problem.AddConstraint(xSumOverJ[i, l] <= xSumOverJ[l, l]);
        
        return problem;
    }

    private static GRBModel CreateExtendedGrb(bool[,] A, double[,] w, int k)
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

        var env = new GRBEnv();
        env.Start();
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

    private static IntegerProblem CreatePersonalIdea(bool[,] A, double[,] w, int k)
    {
        var n = A.LengthI();

        var problem = new IntegerProblem();
        var x = problem.AddBinaryVariables(A);

        // cycle_i has range i..n
        var cycle = problem.AddVariables(Enumerable.Range(0, n).Select(i => VariableType.Range(i, n-1)));
        var positionInCycle = problem.AddVariables(1..k, n);
        var isLastInCycle = problem.AddBinaryVariables(n);
        
        problem.Objective = -SumSum(w, x);

        // as many connections in as out
        problem.AddConstraints(x.SumOverI(), Comparison.Equals, x.SumOverJ());
        // problem.AddConstraints(x.SumOverI(), Comparison.Equals, 1);
        // at most 1 connection out
        problem.AddConstraints(x.SumOverJ(), Comparison.LessEqual, 1);

        // isLast_i == 1  =>  cycle_i == i
        for (int i = 0; i < n; i++)
        {
            problem.AddConstraint(cycle[i] - i <= n * (1 - isLastInCycle[i]));
            // cycle_i >= i is a part of the definition
        }
        
        // x_ij == 1  =>  cycle_i == cycle_j
        foreach (var (i, j) in Indices(A))
        {
            problem.AddConstraint(cycle[i] - cycle[j] <= n * (1 - x[i, j]));
            problem.AddConstraint(cycle[j] - cycle[i] <= n * (1 - x[i, j]));
        }
        
        // x_ij == 1  =>  position_i + 1 <= position_j  or  isLast_i == 1 
        // x_ij == 1  =>  position_i + 1 >= position_j  // or  isLast_i == 1 
        foreach (var (i, j) in Indices(A))
        {
            problem.AddConstraint(positionInCycle[i] + 1 - positionInCycle[j] <= k * (1 - x[i, j] + isLastInCycle[i]));
            // problem.AddConstraint(positionInCycle[j] - positionInCycle[i] - 1 <= k * (1 - x[i, j]));
        }
        
        return problem;
    }

    private static GRBModel CreatePersonalGrb(bool[,] A, double[,] w, int k)
    {
        var n = A.LengthI();

        var env = new GRBEnv();
        env.Start();
        var problem = new GRBModel(env);
        var x = problem.AddBinaryVars(A, "x");

        // cycle_i has range i..n
        var cycle = problem.AddVars(Enumerable.Range(0, n).Select(i => VariableType.Range(i, n-1)), "y");
        var positionInCycle = problem.AddVars(1..k, n, "u");
        var isLastInCycle = problem.AddBinaryVars(n, "z");
        
        problem.SetObjective(-SumSum(w, x));

        // as many connections in as out
        problem.AddConstrs(x.SumOverI(), GRB.EQUAL, x.SumOverJ(), "in == out");
        // problem.AddConstraints(x.SumOverI(), Comparison.Equals, 1);
        // at most 1 connection out
        problem.AddConstrs(x.SumOverJ(), GRB.LESS_EQUAL, 1, "out <= 1");

        // isLast_i == 1  =>  cycle_i == i
        for (int i = 0; i < n; i++)
        {
            problem.AddConstr(cycle[i] - i <= n * (1 - isLastInCycle[i]), $"z[{i}]==1 => y[{i}]=={i}");
            // cycle_i >= i is a part of the definition
        }
        
        // x_ij == 1  =>  cycle_i == cycle_j
        foreach (var (i, j) in Indices(A))
        {
            problem.AddConstr(cycle[i] - cycle[j] <= n * (1 - x[i, j]), $"x[{i},{j}]==1 => y[{i}]==y[{j}]");
            problem.AddConstr(cycle[j] - cycle[i] <= n * (1 - x[i, j]), $"x[{i},{j}]==1 => y[{i}]==y[{j}]");
        }
        
        // x_ij == 1  =>  position_i + 1 <= position_j  or  isLast_i == 1 
        // x_ij == 1  =>  position_i + 1 >= position_j  // or  isLast_i == 1 
        foreach (var (i, j) in Indices(A))
        {
            problem.AddConstr(positionInCycle[i] + 1 - positionInCycle[j] <= k * (1 - x[i, j] + isLastInCycle[i]), $"x[{i},{j}]==1 => u[{i}]<u[{j}] or z[{i}==1");
            // problem.AddConstraint(positionInCycle[j] - positionInCycle[i] - 1 <= k * (1 - x[i, j]));
        }
        
        return problem;
    }

    private static string ToString(VariableType[] values)
    {
        return "[" + String.Join(',', values) + "]";
    }

    private static IEnumerable<(int i, int j)> Indices(bool[,] enabled)
    {
        var (lengthI, lengthJ) = enabled.Dim();

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (enabled[i, j]) yield return (i, j);
        }
    }

    private static DoubleExpression SumSum(double[,] weights, Variable[,] variables)
    {
        return SumSum(variables);
        var (lengthI, lengthJ) = variables.Dim();

        var scales = new double[variables.Length];
        var parts = new Variable[variables.Length];
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            scales[i * lengthJ + j] = weights[i, j];
            parts[i * lengthJ + j] = variables[i, j];
        }

        return new SumDoubleExpression(parts, scales);
    }

    private static Expression SumSum(Variable[,] variables)
    {
        var (lengthI, lengthJ) = variables.Dim();

        var parts = new Variable[variables.Length];
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            parts[i * lengthJ + j] = variables[i, j];

        return SumExpression.Create(parts);
    }

    private static DoubleExpression SumSum(double[,] weights, Expression[,] expressions)
    {
        return SumSum(expressions);
        var (lengthI, lengthJ) = expressions.Dim();

        var scales = new double[expressions.Length];
        var parts = new Expression[expressions.Length];
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            scales[i * lengthJ + j] = weights[i, j];
            parts[i * lengthJ + j] = expressions[i, j];
        }

        return new SumDoubleExpression(parts, scales);
    }

    private static Expression SumSum(Expression[,] expressions)
    {
        var (lengthI, lengthJ) = expressions.Dim();

        var parts = new Expression[expressions.Length];
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            parts[i * lengthJ + j] = expressions[i, j];

        return SumExpression.Create(parts);
    }

    private static GRBLinExpr SumSum(double[,] weights, GRBVar?[,] variables)
    {
        // return variables.SumSum();
        var (lengthI, lengthJ) = variables.Dim();

        var sum = new GRBLinExpr();
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (!ReferenceEquals(variables[i, j], null))
                sum.AddTerm(weights[i, j], variables[i, j]);
        }

        return sum;
    }

    private static GRBLinExpr SumSum(double[,] weights, GRBLinExpr?[,] expressions)
    {
        // return expressions.SumSum();
        var (lengthI, lengthJ) = expressions.Dim();

        var sum = new GRBLinExpr();
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (!ReferenceEquals(expressions[i, j], null))
                sum.MultAdd(weights[i, j], expressions[i, j]);
        }

        return sum;
    }

    private static bool[,] TrueUpper(int n)
    {
        var result = new bool[n, n];

        for (int i = 0; i < n; i++)
        for (int j = i; j < n; j++)
            result[i, j] = true;
        
        return result;
    }

    private static bool[,,] Duplicate(bool[,] array, int lengthK)
    {
        var (lengthI, lengthJ) = array.Dim();

        var result = new bool[lengthI, lengthJ, lengthK];
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        for (int k = 0; k < lengthK; k++)
            result[i, j, k] = array[i, j];

        return result;
    }
}