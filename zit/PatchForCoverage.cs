using System.Diagnostics.CodeAnalysis;

namespace SystemX.Cmd
{

    [ExcludeFromCodeCoverage]
    public partial class CmdParser<T> where T : new() { }

}

namespace Zit
{

    [ExcludeFromCodeCoverage]
    public sealed partial class AutoClean { };

    namespace Ext
    {
        [ExcludeFromCodeCoverage]
        public static partial class Extensions { }
    }

    namespace Ops
    {
        [ExcludeFromCodeCoverage]
        partial struct AssureDirectory { }
    }
}