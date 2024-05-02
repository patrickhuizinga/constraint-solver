using System.Diagnostics;

namespace Solver.Lib;

public sealed class SumExpression : Expression
{
    internal readonly Expression[] Elements;

    private SumExpression(params Expression[] elements)
    {
        Elements = elements;
    }

    public static Expression Create(Expression element)
    {
        return element;
    }

    public static Expression Create(Expression first, Expression second)
    {
        return new Add2Expression(first, second);
    }

    public static Expression Create(Expression first, Expression second, Expression third)
    {
        return new SumExpression(first, second, third);
    }

    public static Expression Create(Expression first, Expression second, Expression third, Expression fourth)
    {
        return new SumExpression(first, second, third, fourth);
    }

    public static Expression Create(params Expression[] elements)
    {
        return elements.Length switch
        {
            0 => new ConstantExpression(0),
            1 => elements[0],
            2 => new Add2Expression(elements[0], elements[1]),
            _ => new SumExpression(elements)
        };
    }

    public override SumExpression Add(Expression addition)
    {
        if (addition is SumExpression sum)
        {
            var elements = new Expression[Elements.Length + sum.Elements.Length];
            Array.Copy(Elements, elements, Elements.Length);
            Array.Copy(sum.Elements, 0, elements, Elements.Length, sum.Elements.Length);
            return new SumExpression(elements);
        }
        else if (addition is Add2Expression add)
        {
            var elements = new Expression[Elements.Length + 2];
            Array.Copy(Elements, elements, Elements.Length);
            elements[^2] = add.First;
            elements[^1] = add.Second;
            return new SumExpression(elements);
            
        }
        else
        {
            var elements = new Expression[Elements.Length + 1];
            Array.Copy(Elements, elements, Elements.Length);
            elements[Elements.Length] = addition;
            return new SumExpression(elements);
        }
    }

    public override SumExpression Add(int addition)
    {
        for (int i = 0; i < Elements.Length; i++)
        {
            if (Elements[i] is ConstantExpression ce)
            {
                var elements = new Expression[Elements.Length];
                Array.Copy(Elements, elements, Elements.Length);
                elements[i] = new ConstantExpression(ce.Value + addition);
                return new SumExpression(elements);
            }
        }

        {
            var elements = new Expression[Elements.Length + 1];
            Array.Copy(Elements, elements, Elements.Length);
            elements[Elements.Length] = new ConstantExpression(addition);
            return new SumExpression(elements);
        }
    }

    public override int GetMin(List<VariableType> variables) => Elements.Sum(e => e.GetMin(variables));

    public override int GetMax(List<VariableType> variables) => Elements.Sum(e => e.GetMax(variables));
    
    public override RestrictResult RestrictToMin(int minValue, List<VariableType> variables)
    {
        var maxValues = new int[Elements.Length];
        int maxSum = 0;
        for (int i = 0; i < Elements.Length; i++)
        {
            maxValues[i] = Elements[i].GetMax(variables);
            maxSum += maxValues[i];
        }

        if (maxSum < minValue)
            return RestrictResult.Infeasible;

        var diff = maxSum - minValue;

        var result = RestrictResult.NoChange;
        for (int i = 0; i < Elements.Length; i++)
        {
            var elResult = Elements[i].RestrictToMin(maxValues[i] - diff, variables);
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

    public override RestrictResult RestrictToMax(int maxValue, List<VariableType> variables)
    {
        var minValues = new int[Elements.Length];
        int minSum = 0;
        for (int i = 0; i < Elements.Length; i++)
        {
            minValues[i] = Elements[i].GetMin(variables);
            minSum += minValues[i];
        }

        if (maxValue < minSum)
            return RestrictResult.Infeasible;

        var diff = maxValue - minSum;

        var result = RestrictResult.NoChange;
        for (int i = 0; i < Elements.Length; i++)
        {
            var elResult = Elements[i].RestrictToMax(minValues[i] + diff, variables);
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

    public override RestrictResult Exclude(int value, List<VariableType> variables)
    {
        var minValues = new int[Elements.Length];
        var maxValues = new int[Elements.Length];
        int minSum = 0, maxSum = 0;
        for (int i = 0; i < Elements.Length; i++)
        {
            minValues[i] = Elements[i].GetMin(variables);
            minSum += minValues[i];
            maxValues[i] = Elements[i].GetMax(variables);
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
            for (int i = 0; i < Elements.Length; i++)
            {
                var elMaxValue = minValues[i] + diff;
                if (maxValues[i] <= elMaxValue)
                    continue;

                var elResult = Elements[i].RestrictToMax(elMaxValue, variables);
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
            for (int i = 0; i < Elements.Length; i++)
            {
                var elMinValue = maxValues[i] - diff;
                if (elMinValue <= minValues[i])
                    continue;
                
                var elResult = Elements[i].RestrictToMin(elMinValue, variables);
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

    public override IEnumerable<int> GetVariableIndices()
    {
        return Elements.SelectMany(e => e.GetVariableIndices());
    }
}