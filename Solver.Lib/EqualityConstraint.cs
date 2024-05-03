using System.Diagnostics;

namespace Solver.Lib;

public class EqualityConstraint : IConstraint
{
    private readonly SortedList<int, int> _left;
    private readonly int _right;

    public EqualityConstraint(Expression left, int right)
    {
        _right = right;
        _left = new SortedList<int, int>();
        AddVariables(left, +1, ref _right);
    }

    public EqualityConstraint(Expression left, Expression right)
    {
        _right = 0;
        _left = new SortedList<int, int>();
        AddVariables(left, +1, ref _right);
        AddVariables(right, -1, ref _right);

        for (int i = _left.Count - 1; i >= 0; i--)
        {
            var scale = _left.GetValueAtIndex(i);
            if (scale == 0)
                _left.RemoveAt(i);
        }
    }

    public EqualityConstraint(EqualityConstraint source)
    {
        _right = source._right;
        _left = new SortedList<int, int>(source._left);
    }

    private void AddVariables(Expression expression, int sign, ref int right)
    {
        switch (expression)
        {
            case Add2Expression add:
                AddVariables(add.First, sign, ref right);
                AddVariables(add.Second, sign, ref right);
                break;
            case ConstantExpression constant:
                right -= sign * constant.Value;
                break;
            case SumExpression sum:
                foreach (var child in sum.Elements) 
                    AddVariables(child, sign, ref right);
                break;
            case Variable variable:
                if (_left.TryGetValue(variable.Index, out int scale))
                    _left[variable.Index] = scale + sign;
                else
                    _left[variable.Index] = sign;
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(expression), expression, "Unsupported type of expression");
        }
    }

    public RestrictResult Restrict(IList<VariableType> variables)
    {
        int minSum = 0;
        int maxSum = 0;
        for (int i = _left.Count - 1; i >= 0; i--)
        {
            var scale = _left.GetValueAtIndex(i);
            if (scale == 0)
                continue;

            var index = _left.GetKeyAtIndex(i);
            var variable = variables[index];
            if (scale > 0)
            {
                minSum += scale * variable.Min;
                maxSum += scale * variable.Max;
            }
            else
            {
                // note: flipped min and max
                minSum -= scale * variable.Max;
                maxSum -= scale * variable.Min;
            }
        }

        if (_right < minSum || maxSum < _right)
            return RestrictResult.Infeasible;
        if (minSum == maxSum)
            return RestrictResult.NoChange;

        var minDiff = _right - minSum;
        var maxDiff = maxSum - _right;

        var result = RestrictResult.NoChange;
        for (int i = 0; i < _left.Count; i++)
        {
            var scale = _left.GetValueAtIndex(i);
            if (scale == 0) continue;
            
            var index = _left.GetKeyAtIndex(i);
            int elMinDiff, elMaxDiff;
            switch (scale)
            {
                case >= 1:
                    elMinDiff = minDiff / scale;
                    elMaxDiff = maxDiff / scale;
                    break;
                case <= -1:
                    // note: flipped min and max
                    elMinDiff = maxDiff / -scale;
                    elMaxDiff = minDiff / -scale;
                    break;
                default:
                    continue;
            }

            var maxResult = Variable.RestrictToMax(index, variables[index].Min + elMinDiff, variables);
            if (maxResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a higher maximum bound than the min possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }
            
            var minResult = Variable.RestrictToMin(index, variables[index].Max - elMaxDiff, variables);
            if (minResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a lower minimum bound than the max possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            if (minResult == RestrictResult.NoChange)
                minResult = maxResult;

            if (result == RestrictResult.NoChange) 
                result = minResult;
        }

        return result;
    }

    public int Range(IList<VariableType> variables)
    {
        int min = 0, max = 0;
        foreach (var index in _left.Keys)
        {
            min += variables[index].Min;
            max += variables[index].Max;
        }
        return max - min;
    }

    public IEnumerable<int> GetVariableIndices() => _left.Keys;

    public EqualityConstraint Clone()
    {
        return new EqualityConstraint(this);
    }
}