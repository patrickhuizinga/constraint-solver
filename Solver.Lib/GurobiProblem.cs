using Gurobi;

namespace Solver.Lib;

public class GurobiProblem : IDisposable
{
    private readonly GRBModel _model;
    private readonly Dictionary<int, GRBVar> _variables = new();

    public GurobiProblem()
    {
        var env = new GRBEnv();
        env.Start();
        _model = new GRBModel(env);
    }

    public GurobiProblem(GRBEnv env)
    {
        _model = new GRBModel(env);
    }

    public void AddVariable(int index, VariableType type)
    {
        GRBVar grbVar;
        if (type.Min == 0 && type.Max == 1)
            grbVar = _model.AddVar(type.Min, type.Max, 0, GRB.BINARY, "x");
        else
            grbVar = _model.AddVar(type.Min, type.Max, 0, GRB.INTEGER, "x");
        
        _variables.Add(index, grbVar);
    }

    public void AddConstraint(IConstraint constraint)
    {
        switch (constraint)
        {
            case EqualityConstraint ec:
            {
                var linExpr = new GRBLinExpr(constraint.Expression.Constant);
                foreach (var (index, scale) in ec.Expression.GetVariables())
                    linExpr.AddTerm(scale, _variables[index]);
                _model.AddConstr(linExpr, GRB.EQUAL, new GRBLinExpr(0), "");
                break;
            }
            case LessThanConstraint lc:
            {
                var linExpr = new GRBLinExpr(constraint.Expression.Constant);
                foreach (var (index, scale) in lc.Expression.GetVariables())
                    linExpr.AddTerm(scale, _variables[index]);
                _model.AddConstr(linExpr, GRB.LESS_EQUAL, new GRBLinExpr(0), "");
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(constraint), "type of constraint not supported.");
        }
    }

    public void SetObjective(DoubleExpression objective)
    {
        var linExpr = new GRBLinExpr(objective.Constant);
        foreach (var (index, scale) in objective.GetVariables()) 
            linExpr.AddTerm(scale, _variables[index]);
        
        _model.SetObjective(linExpr);
    }

    public double Solve()
    {
        _model.Optimize();
        if (_model.Status == GRB.Status.OPTIMAL)
            return _model.ObjVal;
        Console.WriteLine("Status was : " + _model.Status);
        return 0;
    }

    public void Dispose()
    {
        _model.Dispose();
    }
}