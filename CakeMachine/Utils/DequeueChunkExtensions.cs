namespace CakeMachine.Utils
{
    internal static class DequeueChunkExtensions
    {
        public static IEnumerable<T> Dequeue<T>(this Queue<T> queue, uint nb)
        {
            for (var i = 0; i < nb; i++)
            {
                var canDequeue = queue.TryDequeue(out var element);
                if(!canDequeue) yield break;
                yield return element!;
            }
        }
    }
}
