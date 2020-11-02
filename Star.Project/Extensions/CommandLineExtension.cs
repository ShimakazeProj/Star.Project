using System.CommandLine;
using System.CommandLine.Invocation;

namespace Star.Project
{
    internal static class CommandLineExtension
    {
        public static Option<T> SetArgumentName<T>(this Option<T> @this, string name)
        {
            @this.Argument.Name = name;
            return @this;
        }
        public static Command SetHandler(this Command @this, ICommandHandler handler)
        {
            @this.Handler = handler;
            return @this;
        }
    }
}
