namespace Solver.Lib;

public class BinaryVariable : Variable
{

    public override int Min => 0;

    public override int Max => 1;
    
    public override int GetMin(Dictionary<Variable, Variable> variables) => variables[this].Min;
    
    public override int GetMax(Dictionary<Variable, Variable> variables) => variables[this].Max;
    
    public override RestrictResult RestrictToMin(int minValue, Dictionary<Variable, Variable> variables)
    {
        var oldVal = variables[this];
        var newVal = oldVal.TryRestrictToMin(minValue);
        if (newVal == null)
            return RestrictResult.Infeasible;
        if (newVal == oldVal)
            return RestrictResult.NoChange;

        variables[this] = newVal;
        return RestrictResult.Change;
    }

    public override RestrictResult RestrictToMax(int maxValue, Dictionary<Variable, Variable> variables)
    {
        var oldVal = variables[this];
        var newVal = oldVal.TryRestrictToMax(maxValue);
        if (newVal == null)
            return RestrictResult.Infeasible;
        if (newVal == oldVal)
            return RestrictResult.NoChange;

        variables[this] = newVal;
        return RestrictResult.Change;
    }
    
    public override Variable? TryRestrictToMin(int minValue)
    {
        if (minValue <= 0)
            return this;
        if (minValue == 1)
            return new ConstantVariable(1);
        
        return null;
    }

    public override Variable? TryRestrictToMax(int maxValue)
    {
        if (1 <= maxValue)
            return this;
        if (maxValue == 0)
            return new ConstantVariable(0);
        
        return null;
    }

    public override string ToString()
    {
        return "[0,1]";
    }
}