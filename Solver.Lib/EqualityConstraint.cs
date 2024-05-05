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

    public EqualityConstraint(Variable left, int right)
    {
        _right = right;
        _left = new SortedList<int, int> { { left.Index, 1 } };
    }

    public EqualityConstraint(Variable left, Variable right)
    {
        _right = 0;
        _left = new SortedList<int, int>();
        if (left.Index == right.Index)
            return;

        _left = new SortedList<int, int> { { left.Index, 1 }, { right.Index, -1 } };
    }

    public EqualityConstraint(EqualityConstraint source)
    {
        _right = source._right;
        _left = new SortedList<int, int>(source._left);
    }

    private void AddVariables(Expression expression, int sign, ref int right)
    {
        right -= sign * expression.Constant;
        
        switch (expression)
        {
            case ConstantExpression:
                break;
            case Add1Expression add1:
                AddVariable(add1.VariableIndex, add1.Scale);
                break;
            case Add2Expression add2:
                AddVariable(add2.FirstVariableIndex, add2.FirstScale);
                AddVariable(add2.SecondVariableIndex, add2.SecondScale);
                break;
            case SumExpression sum:
                foreach (var (index, scale) in sum.GetVariables())
                    AddVariable(index, scale);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(expression), expression, "Unsupported type of expression");
        }
    }

    private void AddVariable(int index, int scale)
    {
        var i = _left.IndexOfKey(index);

        if (i == -1)
            _left[index] = scale;
        else
            _left.SetValueAtIndex(i, _left.GetValueAtIndex(i) + scale);
    }

    public RestrictResult Restrict(VariableCollection variables)
    {
        int minSum = 0;
        int maxSum = 0;
        foreach(var (index, scale) in _left)
        {
            if (scale == 0) continue;
            
            var variable = variables[index];
            minSum += variable.GetMin(scale);
            maxSum += variable.GetMax(scale);
        }

        if (_right < minSum || maxSum < _right)
            return RestrictResult.Infeasible;
        if (minSum == maxSum)
            return RestrictResult.NoChange;

        var minDiff = _right - minSum;
        var maxDiff = maxSum - _right;

        var result = RestrictResult.NoChange;
        foreach(var (index, scale) in _left)
        {
            if (scale == 0) continue;
            
            int elMinDiff, elMaxDiff;
            switch (scale)
            {
                case > 0:
                    elMinDiff = minDiff / scale;
                    elMaxDiff = maxDiff / scale;
                    break;
                case < 0:
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

    public IEnumerable<int> GetVariableIndices() => _left.Keys;

    public EqualityConstraint Clone()
    {
        return new EqualityConstraint(this);
    }
}