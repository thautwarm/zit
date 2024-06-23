using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Pastel;

[ExcludeFromCodeCoverage]
public static class Assert
{
    static int s_Total = 0;
    static int s_Passed = 0;


    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class Case : System.Attribute
    {
        public int Order { get; set; }
    }
    static string _nulls<T>(T value) => value is null ? "null" : (value.ToString() ?? "null");
    static string _y<T>(T value) => _nulls(value).ToString().Pastel(ConsoleColor.Yellow);
    static string _r<T>(T value) => _nulls(value).ToString().Pastel(ConsoleColor.Magenta);
    static string _g<T>(T value) => _nulls(value).ToString().Pastel(ConsoleColor.Green);
    static string _b<T>(T value) => _nulls(value).ToString().Pastel(ConsoleColor.Cyan);

    public static string CallerFilePath([System.Runtime.CompilerServices.CallerFilePath] string filePath = "") => filePath;

    public static void Cond(
        bool cond,
        [System.Runtime.CompilerServices.CallerMemberName] string membName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        if (!cond)
        {
            Console.WriteLine($"Failed: {filePath}({lineNumber}): {membName}");
        }
    }

    public static void Equal<T>(
        T expected,
        T actual,
        [System.Runtime.CompilerServices.CallerMemberName] string membName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
    {
        bool cond = false;

        if (expected is null)
        {
            cond = actual is null;
        }
        else
        {
            try
            {
                cond = expected.Equals(actual);
            }
            catch (Exception)
            {
                var msg = _r("Exception thrown while comparing values: ")
                          + _y(filePath)
                          + ":" + _y(lineNumber)
                          + " at "
                          + _y(membName + "()");
                throw;
            }
        }
        if (!cond)
        {
            Console.WriteLine(_r("Failed   :") + $" {filePath}:{lineNumber} at {membName}()");
            Console.WriteLine("Expected :" + $" {_y(expected)}");
            Console.WriteLine("Actual   :" + $" {_r(actual)}");
        }
    }

    static string GetFullName(this MethodInfo m)
    {
        var t = m.DeclaringType;
        if (t is not null)
            return t.FullName + "." + m.Name;
        return m.Name;
    }

    public static void Run(Assembly asm)
    {
        List<string> unPassed = new List<string>();
        var cases = asm.GetTypes()
            .SelectMany(a => a.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            // check the signature of the method: void f();
            .Where(a => a.GetCustomAttribute<Case>() != null)
            .OrderBy(a => a.GetCustomAttribute<Case>()?.Order);


        foreach (var c in cases)
        {
            if (c.GetParameters().Length != 0)
            {
                Console.WriteLine(_y("Warning: ") + _y("skip the test case ") + _r(c.GetFullName()) + _y(" because the definition method has parameters"));
                continue;
            }
            else
            {
                bool passed = false;
                try
                {
                    Console.WriteLine(_b("[" + c.GetFullName() + "] ") + "starting...");
                    Action action = (Action)Delegate.CreateDelegate (typeof(Action), c);
                    action();
                    passed = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(_r("Exception thrown: ") + _y(e.StackTrace));
                }
                if (passed)
                {
                    s_Passed++;
                    Console.WriteLine(_b("[" + c.GetFullName() + "] ") + _g("passed"));
                }
                else
                {
                    Console.WriteLine(_b("[" + c.GetFullName() + "] ") + _r("failed"));
                    unPassed.Add(c.GetFullName());
                }
                s_Total++;
            }
        }
        Console.WriteLine(_b("[*] ") + "succeeded in " + _g(s_Passed) + "/" + s_Total + " cases in toal");
        if (s_Total == s_Passed)
        {
            Console.WriteLine(_g($"All passed"));
        }
        else
        {
            Console.WriteLine(_r("Failed ") + _r(s_Total - s_Passed) + "/" + s_Total + " cases");
            Console.WriteLine(_y("Failed cases:"));
            for (int i = 0; i < unPassed.Count; i++)
            {
                Console.WriteLine("  " + unPassed[i]);
            }
        }
    }
}