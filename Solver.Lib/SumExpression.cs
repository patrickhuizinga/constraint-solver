using System.Diagnostics;

namespace Solver.Lib;

public sealed class SumExpression : Expression
{
    private readonly Expression[] _elements;

    public SumExpression(params Expression[] elements)
    {
        _elements = elements;
    }

    public override SumExpression Add(Expression addition)
    {
        if (addition is SumExpression sum)
        {
            var elements = new Expression[_elements.Length + sum._elements.Length];
            Array.Copy(_elements, elements, _elements.Length);
            Array.Copy(sum._elements, 0, elements, _elements.Length, sum._elements.Length);
            return new SumExpression(elements);
        }
        else
        {
            var elements = new Expression[_elements.Length + 1];
            Array.Copy(_elements, elements, _elements.Length);
            elements[_elements.Length] = addition;
            return new SumExpression(elements);
        }
    }

    public override int GetMin(Dictionary<Variable, Variable> variables) => _elements.Sum(e => e.GetMin(variables));

    public override int GetMax(Dictionary<Variable, Variable> variables) => _elements.Sum(e => e.GetMax(variables));
    
    public override RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables)
    {
        var maxValues = new int[_elements.Length];
        int maxSum = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            maxValues[i] = _elements[i].GetMax(variables);
            maxSum += maxValues[i];
        }

        if (maxSum < minValue)
            return RestrictResult.Infeasible;

        var diff = maxSum - minValue;

        var result = RestrictResult.NoChange;
        for (int i = 0; i < _elements.Length; i++)
        {
            var elResult = _elements[i].RestrictToMin(maxValues[i] - diff, variables);
            if (elResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a lower minimum bound than the max possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            if (result == RestrictResult.NoChange) 
                result = elResult;
        }

        return result;
    }

    public override RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables)
    {
        var minValues = new int[_elements.Length];
        int minSum = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            minValues[i] = _elements[i].GetMin(variables);
            minSum += minValues[i];
        }

        if (maxValue < minSum)
            return RestrictResult.Infeasible;

        var diff = maxValue - minSum;

        var result = RestrictResult.NoChange;
        for (int i = 0; i < _elements.Length; i++)
        {
            var elResult = _elements[i].RestrictToMax(minValues[i] + diff, variables);
            if (elResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a higher maximum bound than the min possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            if (result == RestrictResult.NoChange)
                result = elResult;
        }

        return result;
    }

    public override RestrictResult Exclude(int value, Dictionary<Variable, Variable> variables)
    {
        var minValues = new int[_elements.Length];
        var maxValues = new int[_elements.Length];
        int minSum = 0, maxSum = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            minValues[i] = _elements[i].GetMin(variables);
            minSum += minValues[i];
            maxValues[i] = _elements[i].GetMax(variables);
            maxSum += maxValues[i];

            if (value < minSum || maxSum < value)
                return RestrictResult.NoChange;
        }

        if (minSum == maxSum)
        {
            return minSum == value
                ? RestrictResult.Infeasible
                : RestrictResult.NoChange;
        }

        if (maxSum == value)
        {
            var maxValue = value - 1;
            var diff = maxValue - minSum;

            var result = RestrictResult.NoChange;
            for (int i = 0; i < _elements.Length; i++)
            {
                var elMaxValue = minValues[i] + diff;
                if (maxValues[i] <= elMaxValue)
                    continue;

                var elResult = _elements[i].RestrictToMax(elMaxValue, variables);
                if (elResult == RestrictResult.Infeasible)
                {
                    // it should always be possible to set a higher maximum bound than the min possible value!
                    Debugger.Break();
                    return RestrictResult.Infeasible;
                }

                if (result == RestrictResult.NoChange)
                    result = elResult;
            }

            return result;
        }

        if (minSum == value)
        {
            var minValue = value + 1;
            var diff = maxSum - minValue;

            var result = RestrictResult.NoChange;
            for (int i = 0; i < _elements.Length; i++)
            {
                var elMinValue = maxValues[i] - diff;
                if (elMinValue <= minValues[i])
                    continue;
                
                var elResult = _elements[i].RestrictToMin(elMinValue, variables);
                if (elResult == RestrictResult.Infeasible)
                {
                    // it should always be possible to set a lower minimum bound than the max possible value!
                    Debugger.Break();
                    return RestrictResult.Infeasible;
                }

                if (result == RestrictResult.NoChange) 
                    result = elResult;
            }

            return result;
        }

        return RestrictResult.NoChange;
    }

    public override IEnumerable<Variable> GetVariables()
    {
        return _elements.SelectMany(e => e.GetVariables());
    }
}