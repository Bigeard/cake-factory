namespace CakeMachine.Utils
{
    public class ThreadSafeRandomNumberGenerator
    {
        private readonly Random _random;

        public ThreadSafeRandomNumberGenerator()
        {
            _random = new Random();
        }

        private ThreadSafeRandomNumberGenerator(ThreadSafeRandomNumberGenerator parent)
        {
            lock (parent._random)
            {
                _random = new Random(parent._random.Next());
            }
        }

        public ThreadSafeRandomNumberGenerator Fork() => new (this);

        public double NextDouble()
        {
            lock (_random)
            {
                return _random.NextDouble();
            }
        }

        public bool NextBoolean(double chancesOfTrue)
        {
            lock (_random)
            {
                return _random.NextDouble() < chancesOfTrue;
            }
        }
    }
}
