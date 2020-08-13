using System;
using System.Threading;

namespace populated_ports
{
    // https://stackoverflow.com/questions/57615/how-to-add-a-timeout-to-console-readline
    internal class Reader
    {
        private static readonly AutoResetEvent GetInput;
        private static readonly AutoResetEvent GotInput;
        private static string _input;

        static Reader()
        {
            GetInput = new AutoResetEvent(false);
            GotInput = new AutoResetEvent(false);
            var inputThread = new Thread(Reader__) {IsBackground = true};
            inputThread.Start();
        }

        private static void Reader__()
        {
            while (true)
            {
                GetInput.WaitOne();
                _input = Console.ReadLine();
                GotInput.Set();
            }
        }

        // omit the parameter to read a line without a timeout
        public static string ReadLine(int timeOutMillisecs = Timeout.Infinite)
        {
            GetInput.Set();
            var success = GotInput.WaitOne(timeOutMillisecs);
            if (success)
                return _input;
            throw new TimeoutException("User did not provide input within the timelimit.");
        }

        public static bool TryReadLine(out string line, int timeOutMillisecs = Timeout.Infinite)
        {
            GetInput.Set();
            var success = GotInput.WaitOne(timeOutMillisecs);
            if (success)
                line = _input;
            else
                line = null;
            return success;
        }
    }
}