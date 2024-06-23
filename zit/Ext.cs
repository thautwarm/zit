namespace Zit.Ext;

public static partial class Extensions
{
    public static R Pipe<T, R>(this T t, System.Func<T, R> f) => f(t);
    public static void Pipe<T>(this T t, System.Action<T> f) => f(t);
    public static T With<T>(this T t, System.Action<T> f)
    {
        f(t);
        return t;
    }
}