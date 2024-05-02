namespace Solver.Lib;

public class BinaryType : VariableType
{

    public override int Min => 0;

    public override int Max => 1;
    
    public override VariableType? TryRestrictToMin(int minValue)
    {
        if (minValue <= 0)
            return this;
        if (minValue == 1)
            return new ConstantType(1);
        
        return null;
    }

    public override VariableType? TryRestrictToMax(int maxValue)
    {
        if (1 <= maxValue)
            return this;
        if (maxValue == 0)
            return new ConstantType(0);
        
        return null;
    }

    public override VariableType TryExclude(int value)
    {
        return value switch
        {
            0 => new ConstantType(1),
            1 => new ConstantType(0),
            _ => this
        };
    }

    public override string ToString()
    {
        return "[0,1]";
    }
}