using System.Diagnostics;

namespace Solver.Lib;

public sealed class Add2Expression : Expression
{
    public Expression First { get; }
    public Expression Second { get; }

    public Add2Expression(Expression first, Expression second)
    {
        First = first;
        Second = second;
    }

    public override Expression Add(Expression addition)
    {
        switch (addition)
        {
            case SumExpression sum:
                var elements = new Expression[2 + sum.Elements.Length];
                elements[0] = First;
                elements[1] = Second;
                Array.Copy(sum.Elements, 0, elements, 2, sum.Elements.Length);
                return SumExpression.Create(elements);
            case Add2Expression add:
                return SumExpression.Create(First, Second, add.First, add.Second);
            case ConstantExpression cv:
                return Add(cv.Value);
            default:
                return SumExpression.Create(First, Second, addition);
        }
    }

    public override Expression Add(int addition)
    {
        if (First is ConstantExpression cv1)
        {
            return new Add2Expression(new ConstantExpression(cv1.Value + addition), Second);
        }

        if (Second is ConstantExpression cv2)
        {
            return new Add2Expression(First, new ConstantExpression(cv2.Value + addition));
        }

        return SumExpression.Create(First, Second, addition);
    }

    public override int GetMin(IList<VariableType> variables) =>
        First.GetMin(variables) + Second.GetMin(variables);

    public override int GetMax(IList<VariableType> variables) =>
        First.GetMax(variables) + Second.GetMax(variables);

    public override RestrictResult RestrictToMin(int minValue, IList<VariableType> variables)
    {
        int firstMax = First.GetMax(variables);
        int secondMax = Second.GetMax(variables);
        int maxSum = firstMax + secondMax;

        if (maxSum < minValue)
            return RestrictResult.Infeasible;

        var diff = maxSum - minValue;

        var firstResult = First.RestrictToMin(firstMax - diff, variables);
        if (firstResult == RestrictResult.Infeasible)
        {
            // it should always be possible to set a lower minimum bound than the max possible value!
            Debugger.Break();
            return RestrictResult.Infeasible;
        }

        var secondResult = Second.RestrictToMin(secondMax - diff, variables);
        if (secondResult == RestrictResult.Infeasible)
        {
            // it should always be possible to set a lower minimum bound than the max possible value!
            Debugger.Break();
            return RestrictResult.Infeasible;
        }

        return firstResult == RestrictResult.NoChange ? secondResult : firstResult;
    }

    public override RestrictResult RestrictToMax(int maxValue, IList<VariableType> variables)
    {
        int firstMin = First.GetMin(variables);
        int secondMin = Second.GetMin(variables);
        int minSum = firstMin + secondMin;

        if (maxValue < minSum)
            return RestrictResult.Infeasible;

        var diff = maxValue - minSum;

        var firstResult = First.RestrictToMax(firstMin + diff, variables);
        if (firstResult == RestrictResult.Infeasible)
        {
            // it should always be possible to set a higher maximum bound than the min possible value!
            Debugger.Break();
            return RestrictResult.Infeasible;
        }

        var secondResult = Second.RestrictToMax(secondMin + diff, variables);
        if (secondResult == RestrictResult.Infeasible)
        {
            // it should always be possible to set a higher maximum bound than the min possible value!
            Debugger.Break();
            return RestrictResult.Infeasible;
        }

        return firstResult == RestrictResult.NoChange ? secondResult : firstResult;
    }

    public override RestrictResult Exclude(int value, IList<VariableType> variables)
    {
        int firstMin = First.GetMin(variables);
        int secondMin = Second.GetMin(variables);
        int minSum = firstMin + secondMin;

        int firstMax = First.GetMax(variables);
        int secondMax = Second.GetMax(variables);
        int maxSum = firstMax + secondMax;

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

            var firstResult = First.RestrictToMax(firstMin + diff, variables);
            if (firstResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a higher maximum bound than the min possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            var secondResult = Second.RestrictToMax(secondMin + diff, variables);
            if (secondResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a higher maximum bound than the min possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            return firstResult == RestrictResult.NoChange ? secondResult : firstResult;
        }

        if (minSum == value)
        {
            var minValue = value + 1;
            var diff = maxSum - minValue;

            var firstResult = First.RestrictToMin(firstMax - diff, variables);
            if (firstResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a lower minimum bound than the max possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            var secondResult = Second.RestrictToMin(secondMax - diff, variables);
            if (secondResult == RestrictResult.Infeasible)
            {
                // it should always be possible to set a lower minimum bound than the max possible value!
                Debugger.Break();
                return RestrictResult.Infeasible;
            }

            return firstResult == RestrictResult.NoChange ? secondResult : firstResult;
        }

        return RestrictResult.NoChange;
    }

    public override IEnumerable<int> GetVariableIndices()
    {
        return Enumerable.Concat(
            First.GetVariableIndices(),
            Second.GetVariableIndices());
    }
}