using Gurobi;
using Solver.Lib;

namespace Solver.Gurobi;

public static class ModelExtensions
{
    public static GRBVar AddVar(this GRBModel model, VariableType type, string name = "x")
    {
        if (type is { Min: 0, Max: 1 })
            return model.AddVar(0, 1, 0, GRB.BINARY, name);
        
        return model.AddVar(type.Min, type.Max, 0, GRB.INTEGER, name);
    }
    
    public static GRBVar[] AddVars(this GRBModel model, VariableType type, int length, string name = "x")
    {
        if (type is { Min: 0, Max: 1 })
            return model.AddVars(length, GRB.BINARY);
        
        var result = new GRBVar[length];

        for (int i = 0; i < length; i++) 
            result[i] = model.AddVar(type.Min, type.Max, 0, GRB.INTEGER, $"{name}[{i}]");
        
        return result;
    }
    
    public static GRBVar[,] AddVars(this GRBModel model, VariableType type, int lengthI, int lengthJ, string name = "x")
    {
        var result = new GRBVar[lengthI, lengthJ];

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            result[i, j] = model.AddVar(type, $"{name}[{i},{j}]");
        
        return result;
    }
    
    public static GRBVar[,,] AddVars(this GRBModel model, VariableType type, int lengthI, int lengthJ, int lengthK, string name = "x")
    {
        var result = new GRBVar[lengthI, lengthJ, lengthK];

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        for (int k = 0; k < lengthK; k++)
            result[i, j, k] = model.AddVar(type, $"{name}[{i},{j},{k}]");
        
        return result;
    }

    public static GRBVar[] AddVars(this GRBModel model, IEnumerable<VariableType> variables, string name = "x")
    {
        if (!variables.TryGetNonEnumeratedCount(out var count))
        {
            variables = variables.ToList();
            count = variables.Count();
        }
        
        var result = new GRBVar[count];

        int i = 0;
        foreach (var variable in variables) 
            result[i++] = model.AddVar(variable, $"{name}[{i}]");

        return result;
    }

    public static GRBVar AddBinaryVar(this GRBModel model, string name = "x")
    {
        return model.AddVar(0, 1, 0, GRB.BINARY, name);
    }

    public static GRBVar[] AddBinaryVars(this GRBModel model, int length, string name = "x")
    {
        var result = new GRBVar[length];

        for (int i = 0; i < length; i++) 
            result[i] = model.AddBinaryVar($"{name}[{i}]");

        return result;
    }

    public static GRBVar[,] AddBinaryVars(this GRBModel model, int lengthI, int lengthJ, string name = "x")
    {
        var result = new GRBVar[lengthI, lengthJ];

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            result[i, j] = model.AddBinaryVar($"{name}[{i},{j}]");

        return result;
    }

    public static GRBVar[,,] AddBinaryVars(this GRBModel model, int lengthI, int lengthJ, int lengthK, string name = "x")
    {
        var result = new GRBVar[lengthI, lengthJ, lengthK];

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        for (int k = 0; k < lengthK; k++)
            result[i, j, k] = model.AddBinaryVar($"{name}[{i},{j},{k}]");

        return result;
    }

    public static GRBVar?[] AddBinaryVars(this GRBModel model, bool[] template, string name = "x")
    {
        var length = template.Dim();
        var result = new GRBVar?[length];

        for (int i = 0; i < length; i++)
        {
            if (template[i])
                result[i] = model.AddBinaryVar($"{name}[{i}]");
            else
                result[i] = null;
        }

        return result;
    }

    public static GRBVar?[,] AddBinaryVars(this GRBModel model, bool[,] template, string name = "x")
    {
        var (lengthI, lengthJ) = template.Dim();
        var result = new GRBVar?[lengthI, lengthJ];

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        {
            if (template[i, j])
                result[i, j] = model.AddBinaryVar($"{name}[{i},{j}]");
            else
                result[i, j] = null;
        }
        
        return result;
    }

    public static GRBVar?[,,] AddBinaryVars(this GRBModel model, bool[,,] template, string name = "x")
    {
        var (lengthI, lengthJ, lengthK) = template.Dim();
        var result = new GRBVar?[lengthI, lengthJ, lengthK];

        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
        for (int k = 0; k < lengthK; k++)
        {
            if (template[i, j, k])
                result[i, j, k] = model.AddBinaryVar($"{name}[{i},{j},{k}]");
            else
                result[i, j, k] = null;
        }

        return result;
    }

    public static GRBConstr[] AddConstrs(this GRBModel model, GRBLinExpr[] left, char sense, GRBLinExpr right, string name = "constr")
    {
        var result = new GRBConstr[left.Length];
        
        for (int i = 0; i < left.Length; i++)
            result[i] = model.AddConstr(left[i], sense, right, $"{name}[{i}]");

        return result;
    }

    public static GRBConstr[,] AddConstrs(this GRBModel model, GRBLinExpr[,] left, char sense, GRBLinExpr right, string name = "constr")
    {
        var (lengthI, lengthJ) = left.Dim();
        var result = new GRBConstr[lengthI, lengthJ];
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            result[i, j] = model.AddConstr(left[i, j], sense, right, $"{name}[{i},{j}]");

        return result;
    }

    public static GRBConstr[] AddConstrs(this GRBModel model, GRBLinExpr[] left, char sense, GRBLinExpr[] right, string name = "constr")
    {
        var result = new GRBConstr[left.Length];
        
        for (int i = 0; i < left.Length; i++)
            result[i] = model.AddConstr(left[i], sense, right[i], $"{name}[{i}]");

        return result;
    }

    public static GRBConstr[,] AddConstrs(this GRBModel model, GRBLinExpr[,] left, char sense, GRBLinExpr[,] right, string name = "constr")
    {
        var (lengthI, lengthJ) = left.Dim();
        var result = new GRBConstr[lengthI, lengthJ];
        
        for (int i = 0; i < lengthI; i++)
        for (int j = 0; j < lengthJ; j++)
            result[i, j] = model.AddConstr(left[i, j], sense, right[i, j], $"{name}[{i},{j}]");

        return result;
    }
}