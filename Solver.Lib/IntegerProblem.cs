using System.Diagnostics;

namespace Solver.Lib;

public class IntegerProblem
{
    private readonly VariableCollection _variables;
    private readonly List<VariableConstraintInfo> _variableToConstraint;
    private readonly List<ConstraintInfo> _constraints;

    public DoubleExpression Objective { get; set; }

    public bool IsInfeasible { get; private set; }

    public IntegerProblem()
    {
        _variables = new VariableCollection { VariableType.Zero };
        _variableToConstraint = [new VariableConstraintInfo(Variable.None.Index)];
        _constraints = [];
        Objective = Expression.Zero;
        IsInfeasible = false;
    }

    private IntegerProblem(
        VariableCollection variables,
        List<VariableConstraintInfo> variableToConstraint,
        List<ConstraintInfo> constraints,
        DoubleExpression objective,
        bool isInfeasible)
    {
        _variables = variables;
        _variableToConstraint = variableToConstraint;
        _constraints = constraints;

        Objective = objective;
        IsInfeasible = isInfeasible;
    }

    private IntegerProblem(
        VariableCollection variables,
        DoubleExpression objective,
        bool isInfeasible)
    {
        _variables = variables;
        _variableToConstraint = Enumerable.Range(0, _variables.Count).Select(VariableConstraintInfo.New).ToList();
        _constraints = new List<ConstraintInfo>();
        
        // constraints.Select(ConstraintInfo.New).ForEach(AddConstraint);

        Objective = objective;
        IsInfeasible = isInfeasible;
    }

    public IntegerProblem Clone()
    {
        var clone = new IntegerProblem(
            new VariableCollection(_variables),
            // Enumerable.Range(0, _variables.Count).Select(VariableConstraintInfo.New).ToList(),
            // new List<ConstraintInfo>(_constraints.Count),
            Objective,
            IsInfeasible);

        _constraints.Select(ConstraintInfo.New).ForEach(clone.AddConstraint);
        
        return clone;
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
            var (lengthI, lengthJ) = variables.Dim();

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
            var (lengthI, lengthJ, lengthK) = variables.Dim();

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

    public bool IsSolved => Enumerable
        .Range(0, _variables.Count)
        .All(i => _variables[i].IsConstant || _variableToConstraint[i].IsEmpty);

    public double GetObjectiveValue() => Objective.GetMin(_variables);

    public Variable AddVariable(VariableType variable)
    {
        var index = _variables.Count;
        _variables.Add(variable);
        _variableToConstraint.Add(new VariableConstraintInfo(index));
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

    public Variable[] AddVariables(IEnumerable<VariableType> variables)
    {
        if (!variables.TryGetNonEnumeratedCount(out var count))
        {
            variables = variables.ToList();
            count = variables.Count();
        }
        
        var result = new Variable[count];

        int i = 0;
        foreach (var variable in variables) 
            result[i++] = AddVariable(variable);

        return result;
    }

    public Variable AddBinaryVariable() => AddVariable(VariableType.Binary);

    public Variable[] AddBinaryVariables(int count) => AddVariables(VariableType.Binary, count);

    public Variable[,] AddBinaryVariables(int countI, int countJ) => AddVariables(VariableType.Binary, countI, countJ);

    public Variable[,,] AddBinaryVariables(int countI, int countJ, int countK) =>
        AddVariables(VariableType.Binary, countI, countJ, countK);

    public Variable[] AddBinaryVariables(bool[] template)
    {
        var result = new Variable[template.Length];
        
        for (int i = 0; i < template.Length; i++)
        {
            result[i] = template[i]
                ? AddBinaryVariable()
                : Variable.None;
        }

        return result;
    }

    public Variable[,] AddBinaryVariables(bool[,] template)
    {
        var (lengthI, lengthJ) = template.Dim();
        var result = new Variable[lengthI, lengthJ];
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            result[i, j] = template[i, j]
                ? AddBinaryVariable()
                : Variable.None;
        }

        return result;
    }

    public Variable[,,] AddBinaryVariables(bool[,,] template)
    {
        var (lengthI, lengthJ, lengthK) = template.Dim();
        var result = new Variable[lengthI, lengthJ, lengthK];
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        for (int k = 0; k < lengthK; k++)
        {
            result[i, j, k] = template[i, j, k]
                ? AddBinaryVariable()
                : Variable.None;
        }

        return result;
    }

    public Variable[,,] AddBinaryVariables(bool[,] template, int lengthK)
    {
        var (lengthI, lengthJ) = template.Dim();
        var result = new Variable[lengthI, lengthJ, lengthK];
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        for (int k = 0; k < lengthK; k++)
        {
            result[i, j, k] = template[i, j]
                ? AddBinaryVariable()
                : Variable.None;
        }

        return result;
    }

    public void AddConstraint(IConstraint constraint)
    {
        AddConstraint(
            ConstraintInfo.New(constraint, _variables));
    }

    private void AddConstraint(ConstraintInfo info)
    {
        _constraints.Add(info);

        foreach (var index in info.Constraint.GetVariableIndices())
            _variableToConstraint[index].Add(info);
    }

    public void AddConstraints(IEnumerable<IConstraint> constraints)
    {
        foreach (var constraint in constraints)
            AddConstraint(constraint);
    }

    public void AddConstraints<TConstraint>(TConstraint[,] constraints) where TConstraint : IConstraint
    {
        var (lengthI, lengthJ) = constraints.Dim();

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
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
        var (lengthI, lengthJ) = left.Dim();

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            AddConstraint(left[i, j], comparison, right);
        }
    }

    public void AddConstraints<TExpression1, TExpression2>(
        TExpression1[] left, Comparison comparison, TExpression2[] right)
        where TExpression1 : Expression
        where TExpression2 : Expression
    {
        for (var i = 0; i < left.Length; i++)
        {
            AddConstraint(left[i], comparison, right[i]);
        }
    }

    public void AddConstraints<TExpression>(
        TExpression[,] left, Comparison comparison, Expression[,] right)
        where TExpression : Expression
    {
        var (lengthI, lengthJ) = left.Dim();

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            AddConstraint(left[i, j], comparison, right[i, j]);
        }
    }


    public void PreSolve()
    {
        var result = Restrict();
        if (result == RestrictResult.Infeasible)
            return;

        do
        {
            Reduce();
        } while (Restrict() == RestrictResult.Change);
    }

    public void Reduce()
    {
        Console.WriteLine("Reducing");
        
        EliminateConstants();

        foreach (var varConst in _variableToConstraint) 
            varConst.Reduce(_variableToConstraint);
    }

    private void EliminateConstants()
    {
        foreach (var constraint in _constraints)
        {
            constraint.EliminateConstants(_variables);
            
            // Validate(this);
        }
    }

    public RestrictResult Restrict()
    {
        // Console.WriteLine("Restricting");

        var finalResult = RestrictResult.NoChange;
        while (true)
        {
            var result = RestrictOnce();
            if (result == RestrictResult.Infeasible)
            {
                IsInfeasible = true;
                return RestrictResult.Infeasible;
            }

            if (_variables.GetModifications().Any())
                finalResult = RestrictResult.Change;
            else
                return finalResult;
        }
    }

    public int ConstraintsRestricted;

    private RestrictResult RestrictOnce()
    {
        var result = RestrictResult.NoChange;

        var modifiedVariables = _variables.GetModifications()
            .Distinct()
            .ToList();
        _variables.ClearModifications();

        foreach (var variableIndex in modifiedVariables)
        foreach (var info in _variableToConstraint[variableIndex].Constraints) 
                info.ReconsiderCount++;

        var modifiedConstraints = _constraints
            .Where(info => info.ReconsiderCount > 0)
            .OrderByDescending(info => info.ReconsiderCount)
            .ToList();

        // Console.WriteLine($"Restricting {modifiedVariables.Count} modified variables and {modifiedConstraints.Count} constraints.");
        
        foreach (var constraint in modifiedConstraints)
        {
            ConstraintsRestricted++;
            var constraintResult = constraint.Restrict(_variables);
            if (constraintResult == RestrictResult.Infeasible)
                return RestrictResult.Infeasible;

            // Validate(this);

            if (constraintResult == RestrictResult.Change)
                result = RestrictResult.Change;
        }

        return result;
    }

    public IntegerProblem FindFeasible()
    {
        EliminateConstants();
        
        Console.WriteLine($"Solving {_variables.Count - 1} variables with {_constraints.Count} constraints.");

        return FindFeasible(this);
    }

    private static IntegerProblem FindFeasible(IntegerProblem problem)
    {
        problem.Restrict();
        if (problem.IsInfeasible || problem.IsSolved)
            return problem;

        var variableIndex = problem.GetSmallestVariable();
        if (variableIndex == -1)
            throw new InvalidOperationException("There is no unknown variable?");

        var variable = problem._variables[variableIndex];

        var solution = problem;
        
        // Larger values are more likely be decisive, probably
        for (int value = variable.Max; value >= variable.Min; value--)
        {
            Console.WriteLine($"Trying variable[{variableIndex}] = {value}");
            var variables = new VariableCollection(problem._variables);
            variables[variableIndex] = value;
            var clone = new IntegerProblem(variables, problem._variableToConstraint, problem._constraints, problem.Objective, problem.IsInfeasible);

            solution = FindFeasible(clone);
            if (!solution.IsInfeasible)
                return solution;
            
            Console.WriteLine("Infeasible!");
        }

        return solution;
    }

    private int GetSmallestVariable()
    {
        // By returning the variable with the least 'range', we have fewer options to consider
        // and thereby need fewer guesses to make progress.

        var (bestIndex, bestScore) = GetSmallestObjectiveVariable();

        // if (bestScore <= 1)
            // return bestIndex;

        for (int index = 1; index < _variables.Count; index++)
        {
            var variable = _variables[index];
            
            if (variable.IsConstant) continue;

            int score = variable.Size;
            score -= _variableToConstraint[index].Count;

            if (bestScore <= score) continue;

            bestScore = score;
            bestIndex = index;
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

            var score = size - Math.Abs(scale);
            score -= _variableToConstraint[index].Count;

            if (bestScore <= score) continue;

            // compared to GetSmallestVariable(), `diff` can go below 1, since we remove 
            bestScore = score;
            bestIndex = index;
        }

        return (bestIndex, bestScore);
    }

    public IntegerProblem OldMinimize()
    {
        EliminateConstants();
        
        Console.WriteLine($"Minimizing {_variables.Count - 1} variables with {_constraints.Count} constraints.");
        
        foreach (var (index, scale) in Objective.GetVariables())
        {
            if (_variableToConstraint[index].IsEmpty)
            {
                _variables[index] = scale > 0
                    ? _variables[index].Min
                    : _variables[index].Max;
            }
        }
        
        return OldMinimize(this);
    }

    private static IntegerProblem OldMinimize(IntegerProblem problem)
    {
        var bestSolution = problem;
        var bestObjective = double.MaxValue;

        var pq = new PriorityQueue<IntegerProblem, double>();
        pq.Enqueue(problem, problem.Objective.GetMin(problem._variables));

        while (pq.TryDequeue(out var attempt, out var minPossibleObjective))
        {
            if (bestObjective <= minPossibleObjective)
                return bestSolution;

            attempt.Restrict();
            if (attempt.IsInfeasible) continue;
            
            if (attempt.IsSolved)
            {
                var objective = attempt.GetObjectiveValue();
                Console.WriteLine("Found solution with objective " + objective);
                if (objective < bestObjective)
                {
                    bestObjective = objective;
                    bestSolution = attempt;
                }

                continue;
            }

            var variableIndex = attempt.GetSmallestVariable();
            var variable = attempt._variables[variableIndex];

            for (int value = variable.Max; value >= variable.Min; value--)
            {
                var newVariables = new VariableCollection(attempt._variables);
                newVariables[variableIndex] = value;

                var minObjective = attempt.Objective.GetMin(newVariables);
                // prevent the log(n) incurred by the addition, with n the amount of attempts in the queue
                // by not adding we also keep n lower, benefiting future additions
                // it's not much, but every little bit helps
                if (minObjective < bestObjective)
                {
                    var clone = new IntegerProblem(newVariables, attempt.Objective, attempt.IsInfeasible);
                    attempt._constraints
                        .Where(info => !info.IsComplete)
                        .Select(ConstraintInfo.New)
                        .ForEach(clone.AddConstraint);

                    pq.Enqueue(clone, minObjective);
                }
            }
        }

        return bestSolution;
    }

    public IntegerProblem Minimize()
    {
        EliminateConstants();
        
        Console.WriteLine($"{Environment.TickCount} Minimizing {_variables.Count - 1} variables with {_constraints.Count} constraints.");
        
        foreach (var (index, scale) in Objective.GetVariables())
        {
            if (_variableToConstraint[index].IsEmpty)
            {
                _variables[index] = scale > 0
                    ? _variables[index].Min
                    : _variables[index].Max;
            }
        }
        
        // Validate(this);

        return Minimize(this);
    }

    private static IntegerProblem Minimize(IntegerProblem problem)
    {
        var bestSolution = problem;
        var bestObjective = double.MaxValue;

        var pq = new PriorityQueue<IntegerProblem, double>();
        pq.Enqueue(problem, problem.Objective.GetMin(problem._variables));

        while (pq.TryDequeue(out var attempt, out var minPossibleObjective))
        {
            if (bestObjective <= minPossibleObjective)
                return bestSolution;
            
            attempt.Restrict();
            if (attempt.IsInfeasible) continue;

            if (attempt.IsSolved)
            {
                var objective = attempt.GetObjectiveValue();
                Console.WriteLine($"{Environment.TickCount} Found solution with objective {objective}");
                if (objective < bestObjective)
                {
                    bestObjective = objective;
                    bestSolution = attempt;
                }

                continue;
            }

            foreach (var option in attempt.GetConstraintOptions(out var constraint))
            {
                var newVariables = new VariableCollection(attempt._variables);
                foreach (var (index, value) in option) 
                    newVariables[index] = value;
                
                var minObjective = attempt.Objective.GetMin(newVariables);
                // prevent the log(n) incurred by the addition, with n the amount of attempts in the queue
                // by not adding we also keep n lower, benefiting future additions
                // it's not much, but every little bit helps
                if (minObjective < bestObjective)
                {
                    var clone = new IntegerProblem(newVariables, attempt.Objective, attempt.IsInfeasible);
                    attempt._constraints
                        .Where(info => !info.IsComplete)
                        .Select(ConstraintInfo.New)
                        .ForEach(clone.AddConstraint);

                    // problem.Validate(clone);
                    
                    pq.Enqueue(clone, minObjective);
                }
            }
        }

        return bestSolution;
    }

    public IEnumerable<(int variableIndex, int value)[]> GetConstraintOptions(out IConstraint? constraint)
    {
        var bestConstraint = _constraints
            .Where(c => !c.IsComplete)
            .MaxBy(c => c.DecompositionScore);

        constraint = bestConstraint?.Constraint;
        if (bestConstraint is { DecompositionScore: > 1 })
            return bestConstraint.GetValidOptions(_variables);
        constraint = null;
        
        var variableIndex = GetSmallestVariable();
        var variable = _variables[variableIndex];
        var result = new List<(int, int)[]>();
        for (int value = variable.Max; value >= variable.Min; value--)
            result.Add([(variableIndex, value)]);
        return result;
    }

    public bool Validate(IntegerProblem solution)
    {
        var result = true;
        for (var i = 0; i < _constraints.Count; i++)
        {
            var info = _constraints[i];
            var isValid = info.Constraint.IsValid(solution._variables);
            if (!isValid)
                Debugger.Break();

            result &= isValid;
        }

        return result;
    }

    public double GurobiSolve()
    {
        using var gurobiProblem = new GurobiProblem();
        _variables.ForEach(gurobiProblem.AddVariable);
        _constraints
            .Where(info => !info.IsComplete)
            .Select(info => info.Constraint)
            .ForEach(gurobiProblem.AddConstraint);
        gurobiProblem.SetObjective(Objective);
        return gurobiProblem.Solve();
    }


    private class VariableConstraintInfo(int variableIndex)
    {
        private readonly List<ConstraintInfo> _constraints = [];

        public static VariableConstraintInfo New(int variableIndex) => new(variableIndex);

        public EqualityConstraint? PrimaryConstraint
        {
            get
            {
                return _constraints
                    .Select(i => i.Constraint)
                    .Where(c => c.HasLeadingVariable(variableIndex))
                    .OfType<EqualityConstraint>()
                    .OrderBy(c => c.VariableCount)
                    .FirstOrDefault(c => c.GetScale(variableIndex) != 0);
            }
        }

        public int Count => _constraints.Count;

        public bool IsEmpty => _constraints.IsEmpty();

        public IEnumerable<ConstraintInfo> Constraints => _constraints;

        public void Add(ConstraintInfo constraint)
        {
            _constraints.Add(constraint);
        }

        public void Put(ConstraintInfo constraint)
        {
            if (!_constraints.Contains(constraint))
                _constraints.Add(constraint);
        }

        public void Reduce(List<VariableConstraintInfo> allVariables)
        {
            var primary = PrimaryConstraint;
            if (primary == null)
                return;

            var reducedPrimary = primary.ReduceScales(variableIndex);

            for (var i = _constraints.Count - 1; i >= 0; i--)
            {
                var targetRef = _constraints[i];
                if (targetRef.Constraint == primary)
                {
                    targetRef.Constraint = reducedPrimary;
                    continue;
                }

                var target = targetRef.Constraint;
                if (!target.HasLeadingVariable(variableIndex))
                    continue;
                
                var result = target.ReduceBy(reducedPrimary, variableIndex);
                if (result == null)
                    continue;

                _constraints.RemoveAt(i);

                targetRef.Constraint = result;
                foreach (var index in result.GetVariableIndices()) 
                    allVariables[index].Put(targetRef);
            }
        }
    }
    
    
    private class ConstraintInfo
    {
        private IConstraint _constraint;
        
        public bool IsComplete;
        public int ReconsiderCount;
        public int DecompositionScore;

        private ConstraintInfo(IConstraint constraint, int decompositionScore, int reconsiderCount)
        {
            _constraint = constraint;
            IsComplete = false;
            ReconsiderCount = reconsiderCount;
            DecompositionScore = decompositionScore;
        }

        public static ConstraintInfo New(ConstraintInfo source)
        {
            return new ConstraintInfo(source.Constraint, source.DecompositionScore, source.ReconsiderCount);
        }

        public static ConstraintInfo New(IConstraint constraint, VariableCollection variables)
        {
            var score = GetDecompositionScore(constraint, variables);

            return new ConstraintInfo(constraint, score, 1);
        }

        public IConstraint Constraint
        {
            get => _constraint;
            set
            {
                if (ReferenceEquals(_constraint, value))
                    return;
                
                _constraint = value;
                ReconsiderCount++;
            }
        }

        private static int GetDecompositionScore(IConstraint constraint, VariableCollection variables)
        {
            var varCount = constraint.VariableCount;
            if (varCount == 0)
                return 0;
            
            var optionCount = constraint.EstimateValidOptionsCount(variables);
            if (optionCount == 0)
                return 0;
            
            return varCount * varCount / optionCount;
        }

        public void EliminateConstants(VariableCollection variables)
        {
            _constraint = Constraint.EliminateConstants(variables);
            if (_constraint.VariableCount == 0)
                IsComplete = true;
            DecompositionScore = GetDecompositionScore(_constraint, variables);
        }

        public RestrictResult Restrict(VariableCollection variables)
        {
            if (IsComplete)
                return RestrictResult.Complete;
            
            ReconsiderCount = 0;
            var result = Constraint.Restrict(variables);
            if (result == RestrictResult.Complete)
                IsComplete = true;
            return result;
        }
        
        public IEnumerable<(int variableIndex, int value)[]> GetValidOptions(VariableCollection variables)
        {
            return Constraint.GetValidOptions(variables);
        }

        public override string ToString()
        {
            return _constraint.ToString();
        }
    }
}