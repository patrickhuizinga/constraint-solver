using Solver.Lib;

namespace Solver.Test;

public class ConstraintTests
{
    [Test]
    public void LessThanRestrict()
    {
        var variables = new VariableCollection { 0..3, 0..3, 0..3, };

        Variable a = new(0), b = new(1), c = new(2);
        var constraint = 1 + a + 2*b + c <= 3;

        var result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));

        result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));
    }
 
    [Test]
    public void LessThanRestrict2()
    {
        var variables = new VariableCollection { {-3,0}, 0..1, 0..1, };

        Variable a = new(0), b = new(1), c = new(2);
        var constraint = a + b + c <= 3;

        var result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));

        result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));
    }
 
    [Test]
    public void EqualsRestrict()
    {
        var variables = new VariableCollection { 0..3, 0..3, 0..3, };

        Variable a = new(0), b = new(1), c = new(2);
        var constraint = a + 2*b + c == 10;

        var result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));

        result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));
    }

    [Test]
    public void EqualsRestrict2()
    {
        var variables = new VariableCollection { {-3,0}, 0..1, 0..1, };

        Variable a = new(0), b = new(1), c = new(2);
        var constraint = a + b + c == -2;

        var result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));

        result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));
    }

    [Test]
    public void EqualsRestrict3()
    {
        var variables = new VariableCollection { 0,1 };

        Variable a = new(0), b = new(1);
        var constraint = a - b == 0;

        var result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));

        result = constraint.Restrict(variables);
        Console.WriteLine(result);
        Console.WriteLine(String.Join(',', variables));
    }

    [Test]
    public void LessThanOptions()
    {
        var variables = new VariableCollection { 0..1, 0..1, 0..1, };

        Variable a = new(0), b = new(1), c = new(2);
        var constraint = -2*a + 2*b + c <= 0;

        foreach (var option in constraint.GetValidOptions(variables))
        {
            Console.WriteLine(String.Join(',', option));
        }
    }

    [Test]
    public void EqualsOptions()
    {
        var variables = new VariableCollection { 0..1, 0..1, 0..1, };

        Variable a = new(0), b = new(1), c = new(2);
        var constraint = -a + b + c == 0;

        foreach (var option in constraint.GetValidOptions(variables))
        {
            Console.WriteLine(String.Join(',', option));
        }
    }
}
