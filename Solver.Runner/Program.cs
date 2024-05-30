// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using Gurobi;

namespace Solver.Runner;

public static class Program
{
    private static readonly List<string> Models = [];
    private static readonly List<int> N = [];
    private static readonly List<int> K = [];
    private static readonly List<double> Densities = [];
    private static readonly List<int> Threads = [];
    private static readonly List<double> Altruists = [];
    private static readonly List<int> L = [];
    private static readonly SortedSet<bool> RealWeights = [];
    private static readonly TimeSpan GurobiTimeLimit = TimeSpan.FromMinutes(30);

    public static void Main(string[] args)
    {
        Action<string> add = arg => Models.Add(arg);
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "n":
                    add = arg => N.Add(int.Parse(arg));
                    break;
                case "k":
                    add = arg => K.Add(int.Parse(arg));
                    break;
                case "d":
                    add = arg => Densities.Add(double.Parse(arg, CultureInfo.InvariantCulture));
                    break;
                case "t":
                    add = arg => Threads.Add(int.Parse(arg));
                    break;
                case "a":
                    add = arg => Altruists.Add(double.Parse(arg, CultureInfo.InvariantCulture));
                    break;
                case "l":
                    add = arg => L.Add(int.Parse(arg));
                    break;
                case "w":
                    add = arg => RealWeights.Add(bool.Parse(arg));

                    if (args.Length == i + 1 || !bool.TryParse(args[i + 1], out _))
                        RealWeights.Add(true);
                    break;
                default:
                    add(args[i]);
                    break;
            }
        }

        if (Models.Count == 0)
            throw new ArgumentException("Should provide at least 1 model to run");
        
        if (N.Count == 0) N.Add(10);
        if (K.Count == 0) K.Add(3);
        if (Densities.Count == 0) Densities.Add(0.2);
        if (Threads.Count == 0) Threads.Add(1);
        if (Altruists.Count == 0) Altruists.Add(L.Count == 0 ? 0.0 : 0.1);
        if (L.Count == 0) L.Add(int.MaxValue);
        if (RealWeights.Count == 0) RealWeights.Add(false);
        
        if (Models.Count > 1 || N.Count > 1 || K.Count > 1 || Densities.Count > 1 || Altruists.Count > 1 || L.Count > 1 || Threads.Count > 1 || RealWeights.Count > 1)
        {
            StartChildProcesses();
        }
        else
        {
            RunModel();
        }
    }

    private static void RunModel()
    {
        var modelName = Models[0];
        var n = N[0];
        var k = K[0];
        var density = Densities[0];
        var realWeights = RealWeights.Contains(true);
        var thread = Threads[0];

        var model = GetModel(modelName, k);

        using var env = new GRBEnv();
        env.LogToConsole = 0;
        // each run uses a single thread.
        env.Threads = 1;
        env.MIPGap = 0;
        env.TimeLimit = GurobiTimeLimit.TotalSeconds;
        env.Start();
        
        // run the model {threads} times in parallel
        var runs =
            from seed in ParallelEnumerable.Range(42, thread)
            let compat = ModelHelper.CreateCompatibility(n, density, realWeights, seed)
            let result = model.Run(env, compat.A, compat.w)
            select (seed, result);

        using var writer = new StreamWriter("output.dat", append: true);

        foreach (var run in runs)
            LogResult(writer, run.seed, run.result);
    }

    private static IKepModel GetModel(string model, int k)
    {
        return model switch
        {
            "edge" => new ExtendedEdgeModel(k),
            "mtz" => new MtzModel(k),
            // "arcCycle" => new MtzModel(k),
            // "arcCycleRowGen" => new MtzModel(k),
            // "arcPathRowGen" => new MtzModel(k),
            // "edgeAlt" => new MtzModel(k),
            // "arcAlt" => new MtzModel(k),
            // "arcAltRowGen" => new MtzModel(k),
            // "mtzAltSep" => new MtzModel(k),
            // "mtzAltInt" => new MtzModel(k),
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, "unknown model")
        };
    }

    private static void LogResult(TextWriter writer, int seed, ModelResult result)
    {
        writer.WriteLine(
            "{0,-16} {1,3} {2,2} {3,2} {4,2} {5,2} {6,3} {7,1} {8,4} {9,4} {10,10} {11,10}",
            Models[0],
            N[0],
            K[0],
            (int)(100 * Densities[0]),
            (int)(100 * Altruists[0]),
            L[0] > 99 ? 99 : L[0],
            seed,
            RealWeights.Contains(true) ? 1 : 0,
            (int)result.SetupTime.TotalSeconds,
            (int)result.RunningTime.TotalSeconds,
            MaxLength(result.Objective, 10),
            MaxLength(result.ObjectiveGap, 10));
    }

    private static string MaxLength(double number, int maxLength)
    {
        string output = "";
        int decimalPlaces = maxLength - 2; //because every decimal contains at least "0."
        bool isError = true;

        while (isError && decimalPlaces >= 0)
        {
            output = Math.Round(number, decimalPlaces).ToString(NumberFormatInfo.InvariantInfo);
            isError = output.Length > maxLength;
            decimalPlaces--;
        }

        if (isError)
        {
            throw new FormatException($"{number} can't be represented in {maxLength} characters");
        }
        
        return output;
    }

    private static void StartChildProcesses()
    {
        foreach (var model in Models)
        foreach (var n in N)
        foreach (var k in K)
        foreach (var d in Densities)
        foreach (var t in Threads)
        foreach (var w in RealWeights)
        {
            if (Altruists.Count == 0 || Altruists[0] == 0)
            {
                StartChildProcess(model, "n", n, "k", k, "d", d, "t", t, "w", w);
                continue;
            }

            foreach (var a in Altruists)
            foreach (var l in L)
            {
                StartChildProcess(model, "N", n, "K", k, "d", d, "t", t, "w", w, "a", a, "L", l);
            }
        }
    }

    private static void StartChildProcess(params object[] args)
    {
        Console.WriteLine($"{DateTime.Now:u}  Start  {String.Join(" ", args)}");
        var info = new ProcessStartInfo(Environment.ProcessPath);
        foreach (var arg in args)
        {
            if (arg is IFormattable f)
                info.ArgumentList.Add(f.ToString(null, CultureInfo.InvariantCulture));
            else
                info.ArgumentList.Add(arg.ToString()!);
        }

        // hide standard output and capture standard error
        info.UseShellExecute = false;
        info.RedirectStandardOutput = true;
        info.RedirectStandardError = true;

        var proc = new Process { StartInfo = info };

        try
        {
            proc.Start();
            proc.WaitForExit();

            var stderr = proc.StandardError.ReadToEnd();
            if (String.IsNullOrWhiteSpace(stderr))
                return;
            
            using var errorFile = new StreamWriter("error.dat", append: true);
            errorFile.WriteLine(String.Join(" ", args));
            errorFile.WriteLine(stderr);
            errorFile.WriteLine();
        }
        catch (Exception e)
        {
            using var errorFile = new StreamWriter("error.dat", append: true);
            errorFile.WriteLine(String.Join(" ", args));
            errorFile.WriteLine(e);
            errorFile.WriteLine();
        }
    }
}