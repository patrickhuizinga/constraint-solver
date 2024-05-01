using System.Diagnostics;

namespace Solver.Lib;

public class SumExpression : IExpression
{
    private readonly IExpression[] _elements;

    public SumExpression(params IExpression[] elements)
    {
        _elements = elements;
    }

    public int GetMin(Dictionary<Variable, Variable> variables) => _elements.Sum(e => e.GetMin(variables));

    public int GetMax(Dictionary<Variable, Variable> variables) => _elements.Sum(e => e.GetMax(variables));
    
    public RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables)
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

    public RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables)
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

    public IEnumerable<Variable> GetVariables()
    {
        return _elements.SelectMany(e => e.GetVariables());
    }
}