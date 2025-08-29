using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifier.Base
{
    
    //public class BooleanWrapper
    //{
    //    private bool value;

    //    private Func<bool> predicate;


    //    public bool Value { get { return this.value; } set { this.value = value; } }
    //    public BooleanWrapper(bool value) { this.Value = value; }
    //}

    //not a ref type !
    //public static class BoolExtensions
    //{
    //    public static void UntilTrue(this Boolean value, CancellationToken cancellationToken = default) { while (!value) if (cancellationToken != default && cancellationToken.IsCancellationRequested) break; }
    //    public static bool UntilTrue(this Boolean value, TimeSpan allotedDuration, bool throwOnDurationExeeded = false, CancellationToken cancellationToken = default)
    //    {
    //        var startTicks = DateTime.UtcNow.Ticks;  

    //        while (!value)
    //        {
    //            if (cancellationToken != default && cancellationToken.IsCancellationRequested) return false;

    //            if (DateTime.UtcNow.Ticks - startTicks > allotedDuration.Ticks) { if (throwOnDurationExeeded) throw new Exception($"{nameof(UntilTrue)} duration of {allotedDuration.Ticks} ticks exceeded"); }
    //            else return false;
    //        }

    //        return true;
    //    }
    //}

    public class Utils
    {
        //inefficient 
        public static string GetAssemblyNameFromAssemblyQualifiedTypeName(string assemblyQualifiedTypeName)
        {
            var parts = assemblyQualifiedTypeName.Split('.');
            return string.Join('.', new ArraySegment<string>(parts, 0, parts.Length - 1));
        }
    }
}
