namespace Zit.Test;

class Program
{
    static void Main()
    {
        Assert.Run(typeof(Program).Assembly);
    }

    [Assert.Case]
    static void Examle()
    {
    }
}