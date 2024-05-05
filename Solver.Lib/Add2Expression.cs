using System.Diagnostics;

namespace Solver.Lib;

public sealed class Add2Expression : Expression
{
    public int FirstVariableIndex { get; }
    public int FirstScale { get; }
    public int SecondVariableIndex { get; }
    public int SecondScale { get; }

    public override int Constant { get; }

    public Add2Expression(int firstVariableIndex, int firstScale, int secondVariableIndex, int secondScale, int constant)
    {
        FirstVariableIndex = firstVariableIndex;
        FirstScale = firstScale;
        SecondVariableIndex = secondVariableIndex;
        SecondScale = secondScale;
        Constant = constant;
    }

    public override Expression Add(Expression addition, int scale)
    {
        if (scale == 0)
            return this;

        switch (addition)
        {
            case ConstantExpression cv:
                return Add(scale * cv.Value);
            
            case Add1Expression add1:
                if (add1.VariableIndex == FirstVariableIndex)
                {
                    return new Add2Expression(
                        FirstVariableIndex, 
                        FirstScale + scale * add1.Scale,
                        SecondVariableIndex,
                        SecondScale,
                        Constant + scale * add1.Constant);
                }
                if (add1.VariableIndex == SecondVariableIndex)
                {
                    return new Add2Expression(
                        FirstVariableIndex,
                        FirstScale,
                        SecondVariableIndex,
                        SecondScale + scale * add1.Scale,
                        Constant + scale * add1.Constant);
                }

                goto default;
            case Add2Expression add2:
                if (add2.FirstVariableIndex == FirstVariableIndex && add2.SecondVariableIndex == SecondVariableIndex)
                {
                    return new Add2Expression(
                        FirstVariableIndex,
                        FirstScale + scale * add2.FirstScale,
                        SecondVariableIndex,
                        SecondScale + scale * add2.SecondScale,
                        Constant + scale * add2.Constant);
                }
                if (add2.SecondVariableIndex == FirstVariableIndex && add2.FirstVariableIndex == SecondVariableIndex)
                {
                    return new Add2Expression(
                        FirstVariableIndex,
                        FirstScale + scale * add2.SecondScale,
                        SecondVariableIndex,
                        SecondScale + scale * add2.FirstScale,
                        Constant + scale * add2.Constant);
                }
                
                goto default;
            default:
                return new SumExpression(this, addition, scale);
        }
    }

    public override Expression Add(int addition)
    {
        if (addition == 0)
            return this;
        
        return new Add2Expression(
            FirstVariableIndex, FirstScale,
            SecondVariableIndex, SecondScale,
            Constant + addition);
    }

    public override Expression Add(Variable addition, int scale)
    {
        if (scale == 0)
            return this;

        if (addition.Index == FirstVariableIndex)
        {
            return new Add2Expression(FirstVariableIndex, FirstScale + scale, SecondVariableIndex, SecondScale, Constant);
        }
        if (addition.Index == SecondVariableIndex)
        {
            return new Add2Expression(FirstVariableIndex, FirstScale, SecondVariableIndex, SecondScale + scale, Constant);
        }
    
        return new SumExpression(this, addition, scale);
    }

    public override int GetMin(VariableCollection variables) =>
        Constant + variables[FirstVariableIndex].GetMin(FirstScale) + variables[SecondVariableIndex].GetMin(SecondScale);

    public override int GetMax(VariableCollection variables) =>
        Constant + variables[FirstVariableIndex].GetMax(FirstScale) + variables[SecondVariableIndex].GetMax(SecondScale);

    public override RestrictResult RestrictToMaxZero(VariableCollection variables)
    {
        int firstMin = variables[FirstVariableIndex].GetMin(FirstScale);
        int secondMin = variables[SecondVariableIndex].GetMin(SecondScale);
        int minSum = Constant + firstMin + secondMin;

        if (0 < minSum)
            return RestrictResult.Infeasible;

        RestrictResult firstResult;
        switch (FirstScale)
        {
            case 0:
                firstResult = RestrictResult.NoChange;
                break;
            case > 0:
            {
                var elMaxValue = (-Constant - secondMin) / FirstScale;
                firstResult = Variable.RestrictToMax(FirstVariableIndex, elMaxValue, variables);
                break;
            }
            default:
            {
                var elMinValue = (-Constant - secondMin) / FirstScale;
                firstResult = Variable.RestrictToMin(FirstVariableIndex, elMinValue, variables);
                break;
            }
        }

        RestrictResult secondResult;
        switch (SecondScale)
        {
            case 0:
                return firstResult;
            case > 0:
            {
                var elMaxValue = (-Constant - firstMin) / SecondScale;
                secondResult = Variable.RestrictToMax(SecondVariableIndex, elMaxValue, variables);
                break;
            }
            default:
            {
                var elMinValue = (-Constant - firstMin) / SecondScale;
                secondResult = Variable.RestrictToMin(SecondVariableIndex, elMinValue, variables);
                break;
            }
        }

        if (firstResult == RestrictResult.Infeasible || secondResult == RestrictResult.Infeasible)
        {
            // it should always be possible to set a lower maximum bound than the min possible value!
            Debugger.Break();
            return RestrictResult.Infeasible;
        }

        return firstResult == RestrictResult.NoChange ? secondResult : firstResult;
    }

    public override IEnumerable<int> GetVariableIndices()
    {
        yield return FirstVariableIndex;
        yield return SecondVariableIndex;
    }

    public override IEnumerable<KeyValuePair<int, int>> GetVariables()
    {
        yield return KeyValuePair.Create(FirstVariableIndex, FirstScale);
        yield return KeyValuePair.Create(SecondVariableIndex, SecondScale);
        
    }
}