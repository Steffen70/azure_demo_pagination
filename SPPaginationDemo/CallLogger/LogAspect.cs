using AspectInjector.Broker;
using System.Diagnostics;
using SPPaginationDemo.Extensions;

namespace SPPaginationDemo.CallLogger;

[Aspect(Scope.Global)]
public class LogAspect
{
    [Advice(Kind.Before, Targets = Target.Method | Target.Getter | Target.Constructor)]
    public void LogEnter([Argument(Source.Name)] string name, [Argument(Source.Type)] Type type)
    {
        var callingMethod = new StackTrace().GetFrames()[2].GetMethod();
        Console.WriteLine($"Call Logging: {callingMethod?.DeclaringType?.Name.NormalizeTypeName()}.{callingMethod?.Name.NormalizeTypeName()} => {type.Name.NormalizeTypeName()}.{name.NormalizeTypeName()}");
    }
}

[Injection(typeof(LogAspect))]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
public class LogAttribute : Attribute { }
