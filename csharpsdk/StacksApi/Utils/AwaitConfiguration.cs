using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StacksForce.Utils
{
    public static class AwaitConfiguration
    {
        static public bool AllowConfigureAwaitFalse { get; set; } = true;

        public static ConfiguredTaskAwaitable ConfigureAwait(this Task task)
            => task.ConfigureAwait(!AllowConfigureAwaitFalse);

        public static ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task)
            => task.ConfigureAwait(!AllowConfigureAwaitFalse);

        public static ConfiguredValueTaskAwaitable<TResult> ConfigureAwait<TResult>(this ValueTask<TResult> task)
            => task.ConfigureAwait(!AllowConfigureAwaitFalse);
    }
}
