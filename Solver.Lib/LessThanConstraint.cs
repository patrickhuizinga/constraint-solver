namespace Solver.Lib;

public class LessThanConstraint(Expression expression) : IConstraint
{
    public Expression Expression => expression;

    public RestrictResult Restrict(VariableCollection variables)
    {
        return expression.RestrictToMaxZero(variables);
    }

    public IEnumerable<int> GetVariableIndices()
    {
        return expression.GetVariableIndices();
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
        
        if (sourceScale <= targetScale)
        {
            var factor = targetScale / sourceScale;
            targetExpression -= factor * sourceExpression;
            targetScale -= factor * sourceScale;
        }

        if (targetScale != 0 && sourceScale > targetScale)
        {
            targetExpression *= Math.Abs(sourceScale);
            sourceExpression *= targetScale * Math.Sign(sourceScale);

            targetExpression = sourceExpression - targetExpression;
        }

        return new LessThanConstraint(targetExpression);
    }

    public IConstraint EliminateConstants(VariableCollection variables)
    {
        var expression1 = expression.EliminateConstants(variables);
        if (ReferenceEquals(expression1, expression))
            return this;

        return new LessThanConstraint(expression1);
    }

    public int EstimateValidOptionsCount(VariableCollection variables)
    {
        var range = expression.GetRange(variables);
        if (!range.Contains(0))
            return 1;

        var count = 0;
        for (int i = range.Min; i <= 0; i++)
        {
            count += Combinations.Get(range.Size, range.Max - i); 
        }

        return count;
    }

    public IEnumerable<(int variableIndex, int value)[]> GetValidOptions(VariableCollection variables)
    {
        var scales = new List<(int abs, int sign)>();
        var ranges = new List<VariableType>();

        var min = expression.Constant;
        var first = new List<(int, int)>();
        
        foreach (var (index, scale) in expression.GetVariables())
        {
            var range = variables[index];
            if (scale == 0 || range.IsConstant)
                continue;
            
            var sign = Math.Sign(scale);
            var absScale = Math.Abs(scale);

            scales.Add((absScale, sign));
            ranges.Add(range);

            var firsValue = sign == 1 ? range.Min : range.Max;
            first.Add((index, firsValue));
            min += range.GetMin(scale);
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
        yield return first;
            
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
            for (int i = 0; i < size; i++)
            {
                if (sum > 0)
                    break;
            
                yield return first;

                first[varIndex].Item2 += sign;
                sum += absScale;
            }
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
        return range.Min <= 0;
    }

    public override string ToString()
    {
        return expression + " <= 0";
    }
}