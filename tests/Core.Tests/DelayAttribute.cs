using System.Reflection;
using Xunit.Sdk;

public class DelayAttribute : BeforeAfterTestAttribute
{
    private readonly int _milliseconds;

    public DelayAttribute(int milliseconds)
    {
        _milliseconds = milliseconds;
    }

    public override void After(MethodInfo methodUnderTest)
    {
        Thread.Sleep(_milliseconds);
    }
}
