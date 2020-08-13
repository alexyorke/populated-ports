namespace populated_ports
{
    // https://stackoverflow.com/questions/1675077/how-do-i-get-process-name-of-an-open-port-in-c
    /// <summary>
    ///     A mapping for processes to ports and ports to processes that are being used in the system.
    /// </summary>
    public class ProcessPort
    {
        /// <summary>
        ///     Internal constructor to initialize the mapping of process to port.
        /// </summary>
        /// <param name="name">Name of process to be </param>
        /// <param name="processId"></param>
        /// <param name="protocol"></param>
        /// <param name="portNumber"></param>
        internal ProcessPort(string name, int processId, string protocol, int portNumber)
        {
            this.Name = name;
            this.ProcessId = processId;
            this.Protocol = protocol;
            this.PortNumber = portNumber;
        }

        public string ProcessPortDescription =>
            $"{Name} ({Protocol} port {PortNumber} pid {ProcessId})";

        public string Name { get; }

        public int ProcessId { get; }

        public string Protocol { get; }

        public int PortNumber { get; }
    }
}