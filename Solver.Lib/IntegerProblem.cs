namespace Solver.Lib;

public class IntegerProblem
{
    private readonly VariableCollection _variables;
    private readonly List<IConstraint> _constraints;
    private readonly List<List<int>> _variableToConstraint;

    public DoubleExpression Objective { get; set; }

    public bool IsInfeasible { get; private set; }

    public IntegerProblem()
    {
        _variables = new VariableCollection();
        _constraints = [];
        _variableToConstraint = [];
        Objective = Expression.Zero;
        IsInfeasible = false;
    }

    public IntegerProblem(IntegerProblem source)
    {
        _variables = new VariableCollection(source._variables);
        _constraints = source._constraints;
        _variableToConstraint = source._variableToConstraint;
        Objective = source.Objective;
        IsInfeasible = source.IsInfeasible;
    }

    public VariableType this[Variable variable]
    {
        get => _variables[variable.Index];
        set => _variables[variable.Index] = value;
    }

    public VariableType[] this[Variable[] variables]
    {
        get
        {
            var result = new VariableType[variables.Length];
            for (int i = 0; i < variables.Length; i++)
            {
                result[i] = this[variables[i]];
            }

            return result;
        }
    }

    public VariableType[,] this[Variable[,] variables]
    {
        get
        {
            var lengthI = variables.GetLength(0);
            var lengthJ = variables.GetLength(1);

            var result = new VariableType[lengthI, lengthJ];
            for (int i = 0; i < lengthI; i++)
            for (int j = 0; j < lengthJ; j++)
            {
                result[i, j] = this[variables[i, j]];
            }

            return result;
        }
    }

    public VariableType[,,] this[Variable[,,] variables]
    {
        get
        {
            var lengthI = variables.GetLength(0);
            var lengthJ = variables.GetLength(1);
            var lengthK = variables.GetLength(2);

            var result = new VariableType[lengthI, lengthJ, lengthK];
            for (int i = 0; i < lengthI; i++)
            for (int j = 0; j < lengthJ; j++)
            for (int k = 0; k < lengthK; k++)
            {
                result[i, j, k] = this[variables[i, j, k]];
            }

            return result;
        }
    }
    
    public bool IsSolved => _variables
        .Select((v, i) => v.IsConstant || _variableToConstraint[i].IsEmpty())
        .All(b => b);

    public double GetObjectiveValue() => Objective.GetMin(_variables);

    public Variable AddVariable(VariableType variable)
    {
        var index = _variables.Count;
        _variables.Add(variable);
        _variableToConstraint.Add([]);
        return new Variable(index);
    }

    public Variable[] AddVariables(VariableType variable, int count)
    {
        var result = new Variable[count];

        for (int i = 0; i < count; i++)
            result[i] = AddVariable(variable);

        return result;
    }

    public Variable[,] AddVariables(VariableType variable, int countI, int countJ)
    {
        var result = new Variable[countI, countJ];

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
            result[i, j] = AddVariable(variable);

        return result;
    }

    public Variable[,,] AddVariables(VariableType variable, int countI, int countJ, int countK)
    {
        var result = new Variable[countI, countJ, countK];

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        for (int k = 0; k < countK; k++)
        {
            result[i, j, k] = AddVariable(variable);
        }

        return result;
    }

    public Variable AddBinaryVariable() => AddVariable(VariableType.Binary);

    public Variable[] AddBinaryVariables(int count) => AddVariables(VariableType.Binary, count);

    public Variable[,] AddBinaryVariables(int countI, int countJ) => AddVariables(VariableType.Binary, countI, countJ);

    public Variable[,,] AddBinaryVariables(int countI, int countJ, int countK) =>
        AddVariables(VariableType.Binary, countI, countJ, countK);

    public void AddConstraint(IConstraint constraint)
    {
        int constraintIndex = _constraints.Count;
        _constraints.Add(constraint);

        foreach (var index in constraint.GetVariableIndices())
            _variableToConstraint[index].Add(constraintIndex);
    }

    public void AddConstraints(IEnumerable<IConstraint> constraints)
    {
        foreach (var constraint in constraints)
            AddConstraint(constraint);
    }

    public void AddConstraints<TConstraint>(TConstraint[,] constraints) where TConstraint : IConstraint
    {
        var countI = constraints.GetLength(0);
        var countJ = constraints.GetLength(1);

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            AddConstraint(constraints[i, j]);
        }
    }

    public void AddConstraint(Expression left, Comparison comparison, Expression right)
    {
        AddConstraint(
            Constraint.Create(left, comparison, right));
    }

    public void AddConstraints<TExpression>(
        TExpression[] left, Comparison comparison, Expression right)
        where TExpression : Expression
    {
        foreach (var leftItem in left) 
            AddConstraint(leftItem, comparison, right);
    }

    public void AddConstraints<TExpression>(
        TExpression[,] left, Comparison comparison, Expression right)
        where TExpression : Expression
    {
        var countI = left.GetLength(0);
        var countJ = left.GetLength(1);

        for (int i = 0; i < countI; i++)
        for (int j = 0; j < countJ; j++)
        {
            AddConstraint(left[i, j], comparison, right);
        }
    }

    public RestrictResult Restrict()
    {
        RestrictResult finalResult = RestrictResult.NoChange;
        while (true)
        {
            var result = RestrictOnce();

            switch (result)
            {
                case RestrictResult.Infeasible:
                    return RestrictResult.Infeasible;
                case RestrictResult.NoChange:
                    return finalResult;
                case RestrictResult.Change:
                    finalResult = RestrictResult.Change;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, "unknown result " + result);
            }
        }
    }

    private RestrictResult RestrictOnce()
    {
        var result = RestrictResult.NoChange;

        var modifiedVariables = _variables.GetModifications()
            .Distinct()
            .ToList();
        _variables.ClearModifications();
        var modifiedConstraints = modifiedVariables
            .SelectMany(varIndex => _variableToConstraint[varIndex])
            .Distinct();

        if (modifiedVariables.IsEmpty())
        {
            modifiedConstraints = Enumerable.Range(0, _constraints.Count);
        }

        foreach (var constraint in modifiedConstraints.Select(i => _constraints[i]))
        {
            var constraintResult = constraint.Restrict(_variables);
            switch (constraintResult)
            {
                case RestrictResult.Infeasible:
                    IsInfeasible = true;
                    return RestrictResult.Infeasible;
                case RestrictResult.Change:
                    result = RestrictResult.Change;
                    break;
            }
        }
        
        return result;
    }

    public IntegerProblem FindFeasible()
    {
        return FindFeasible(this);

    }

    private static IntegerProblem FindFeasible(IntegerProblem problem)
    {
        problem.Restrict();
        if (problem.IsInfeasible || problem.IsSolved)
            return problem;

        var variableIndex = problem.GetSmallestVariable();
        if (variableIndex == -1)
            throw new InvalidOperationException("There is no worst constraint?");

        var variable = problem._variables[variableIndex];

        // Larger values are more likely be decisive, probably
        for (int value = variable.Max; value >= variable.Min; value--)
        {
            var clone = new IntegerProblem(problem);
            clone._variables[variableIndex] = value;

            var solution = FindFeasible(clone);
            if (!solution.IsInfeasible)
                return solution;
        }

        return problem;
    }

    private int GetSmallestVariable()
    {
        // By returning the variable with the least 'range', we have fewer options to consider
        // and thereby need fewer guesses to make progress.

        var (bestIndex, bestScore) = GetSmallestObjectiveVariable();

        if (bestScore <= 1)
            return bestIndex;
        
        for (int i = 0; i < _variables.Count; i++)
        {
            var variable = _variables[i];
            int score = variable.Size;

            if (score <= 0 || bestScore <= score) continue;

            // can't get smaller than this
            if (score == 1) return i;

            bestScore = score;
            bestIndex = i;
        }

        return bestIndex;
    }

    private (int index, double score) GetSmallestObjectiveVariable()
    {
        int bestIndex = -1;
        double bestScore = double.MaxValue;
        foreach (var (index, scale) in Objective.GetVariables())
        {
            var variable = _variables[index];
            
            int size = variable.Size;
            if (size <= 0) continue;

            var score = size -Math.Abs(scale);

            if (bestScore <= score) continue;

            // compared to GetSmallestVariable(), `diff` can go below 1, since we remove 
            bestScore = score;
            bestIndex = index;
        }

        return (bestIndex, bestScore);
    }

    public IntegerProblem Minimize()
    {
        foreach (var (index, scale) in Objective.GetVariables())
        {
            if (_variableToConstraint[index].IsEmpty())
            {
                _variables[index] = scale > 0
                    ? _variables[index].Min
                    : _variables[index].Max;
            }
        }
        
        return Minimize(this);
    }

    private static IntegerProblem Minimize(IntegerProblem problem)
    {
        var bestSolution = problem;
        double bestObjective = double.MaxValue;
        
        var pq = new PriorityQueue<IntegerProblem, double>();
        pq.Enqueue(problem, problem.GetObjectiveValue());
        
        while (pq.TryDequeue(out var attempt, out double possibleObjective))
        {
            if (bestObjective <= possibleObjective)
                return bestSolution;
            
            attempt.Restrict();
            if (attempt.IsInfeasible) continue;
            
            if (attempt.IsSolved)
            {
                var objective = attempt.GetObjectiveValue();
                if (objective < bestObjective)
                {
                    bestObjective = objective;
                    bestSolution = attempt;
                }
                continue;
            }
            
            var variableIndex = attempt.GetSmallestVariable();

            var variable = attempt._variables[variableIndex];

            for (int value = variable.Min; value <= variable.Max; value++)
            {
                var clone = new IntegerProblem(attempt);
                clone._variables[variableIndex] = value;
                pq.Enqueue(clone, clone.GetObjectiveValue());
            }
        }

        return bestSolution;
    }
}