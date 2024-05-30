namespace Solver.Lib;

public class EqualityConstraint(Expression expression) : IConstraint
{
    public Expression Expression => expression;

    public RestrictResult Restrict(VariableCollection variables)
    {
        return expression.RestrictToEqualZero(variables);
    }

    public IEnumerable<int> GetVariableIndices() => expression.GetVariableIndices();

    public int VariableCount => expression.GetVariables().Count();

    public int GetScale(int variableIndex) => expression.GetScale(variableIndex);

    public EqualityConstraint ReduceScales(int primaryVariableIndex)
    {
        var primaryScale = GetScale(primaryVariableIndex);
        switch (primaryScale)
        {
            case 1:
                return this;
            case -1:
                return new EqualityConstraint(-expression);
        }

        var factor = Math.Abs(expression.Constant);
        foreach (var (_, scale) in expression.GetVariables())
        {
            if (factor == 1 || factor == -1) break;

            factor = factor == 0
                ? Math.Abs(scale)
                : Gcd(factor, Math.Abs(scale));
        }

        if (factor == 0)
            return this;
        if (primaryScale < 0)
            factor = -factor;
        else if (factor == 1)
            return this;

        var newVariables = expression.GetVariables()
            .Select(pair => (new Variable(pair.Key), pair.Value / factor));
        var newConstant = expression.Constant / factor;
        
        return new EqualityConstraint(new SumExpression(newVariables, newConstant));
    }

    private int Gcd(int first, int second)
    {
        if (first < second)
            (first, second) = (second, first);

        do
        {
            (first, second) = (second, first % second);
        } while (second != 0);

        return first;
    }

    public IConstraint ReduceBy(EqualityConstraint source, int variableIndex)
    {
        var sourceScale = source.GetScale(variableIndex);
        if (sourceScale == 0)
            throw new ArgumentException("The source does not contain the variable", nameof(variableIndex));

        var targetScale = expression.GetScale(variableIndex);
        if (targetScale == 0)
            return this;

        var sourceExpression = source.Expression;
        var targetExpression = expression;

        while (targetScale != 0)
        {
            if (sourceScale == targetScale)
            {
                return new EqualityConstraint(targetExpression - sourceExpression);
            }

            if (Math.Abs(targetScale) < Math.Abs(sourceScale))
            {
                (targetScale, sourceScale) = (sourceScale, targetScale);
                (targetExpression, sourceExpression) = (sourceExpression, targetExpression);
            }
            
            // sourceScale.Abs <= targetScale.Abs
            
            var factor = targetScale / sourceScale;
            targetExpression -= factor * sourceExpression;
            targetScale -= factor * sourceScale;
        }

        return new EqualityConstraint(targetExpression);
    }

    public IConstraint EliminateConstants(VariableCollection variables)
    {
        var result = expression.EliminateConstants(variables);
        if (ReferenceEquals(result, expression))
            return this;

        return new EqualityConstraint(result);
    }

    public int EstimateValidOptionsCount(VariableCollection variables)
    {
        var range = expression.GetRange(variables);
        if (!range.Contains(0))
            return 1;
        
        return Combinations.Get(range.Size, range.Max);
    }

    public IEnumerable<(int variableIndex, int value)[]> GetValidOptions(VariableCollection variables)
    {
        var scales = new List<(int abs, int sign)>();
        var ranges = new List<VariableType>();

        var min = expression.Constant;
        var first = new List<(int, int)>();
        
        foreach (var (index, scale) in expression.GetVariables())
        {
            if (scale == 0)
                continue;
            
            var range = variables[index];
            
            min += range.GetMin(scale);

            if (range.IsConstant)
                continue;

            var sign = Math.Sign(scale);
            var absScale = Math.Abs(scale);
            
            scales.Add((absScale, sign));
            ranges.Add(range);

            var firsValue = sign == 1 ? range.Min : range.Max;
            first.Add((index, firsValue));
        }

        if (min > 0)
            throw new InvalidOperationException("Infeasible constraint has no options.");

        (int, int)[] result = first.ToArray();
        
        // spacial case for when the equality holds.
        if (min == 0)
        {
            return Enumerable.Repeat(result, 1);
        }

        // special case for a hot-one encoded set of booleans.
        if (min == -1)
        {
            return GetHotOneOptions(result, scales);
        }

        return GetValidOptions(scales, ranges, result, 0, min);
    }

    private static IEnumerable<(int variableIndex, int value)[]> GetHotOneOptions(
        (int, int)[] first, List<(int abs, int sign)> scales)
    {       
        for (int i = 0; i < first.Length; i++)
        {
            var (absScale, sign) = scales[i];
            if (absScale != 1) continue;
            
            // super nasty and depends on the array being immediately consumed by the iterating method
            first[i].Item2 += sign;
            yield return first;
            first[i].Item2 -= sign;
        }
    } 

    private IEnumerable<(int variableIndex, int value)[]> GetValidOptions(
        List<(int abs, int sign)> scales, List<VariableType> ranges, (int, int)[] first, int varIndex, int sum)
    {
        var oldValue = first[varIndex];
        
        var (absScale, sign) = scales[varIndex];
        var size = ranges[varIndex].Size + 1;
        
        if (varIndex == scales.Count - 1)
        {
            // note: sum is negative and increasing to 0
            if (sum % absScale != 0)
                yield break;
            
            var requiredSize = -sum / absScale;
            if (requiredSize > size)
                yield break;

            first[varIndex].Item2 += sign * requiredSize;
            yield return first;
            first[varIndex] = oldValue;
            yield break;
        }

        for (int i = 0; i < size; i++)
        {
            if (sum > 0)
                break;

            var results = GetValidOptions(scales, ranges, first, varIndex + 1, sum);

            foreach (var result in results)
                yield return result;

            first[varIndex].Item2 += sign;
            sum += absScale;
        }

        first[varIndex] = oldValue;
    }

    public bool IsValid(VariableCollection variables)
    {
        var range = expression.GetRange(variables);
        return range.Contains(0);
    }

    public override string ToString()
    {
        return expression + " = 0";
    }
}