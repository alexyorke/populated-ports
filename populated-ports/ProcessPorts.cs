using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace populated_ports
{
    // https://github.com/2gis/Winium.Desktop/pull/71/commits/3118da30abcc703b1866487ca6c848277131459c
    /// <summary>
    /// Static class that returns the list of processes and the ports those processes use.
    /// </summary>
    public static class ProcessPorts
    {
        /// <summary>
        /// A list of ProcesesPorts that contain the mapping of processes and the ports that the process uses.
        /// </summary>
        public static List<ProcessPort> ProcessPortMap
        {
            get
            {
                return GetNetStatPorts();
            }
        }


        /// <summary>
        /// This method distills the output from netstat -a -n -o into a list of ProcessPorts that provide a mapping between
        /// the process (name and id) and the ports that the process is using.
        /// </summary>
        /// <returns></returns>
        private static List<ProcessPort> GetNetStatPorts()
        {
            List<ProcessPort> ProcessPorts = new List<ProcessPort>();

            try
            {
                using Process Proc = new Process();
                ProcessStartInfo StartInfo = new ProcessStartInfo();
                StartInfo.FileName = "netstat.exe";
                StartInfo.Arguments = "-a -n -o";
                StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                StartInfo.UseShellExecute = false;
                StartInfo.RedirectStandardInput = true;
                StartInfo.RedirectStandardOutput = true;
                StartInfo.RedirectStandardError = true;

                Proc.StartInfo = StartInfo;
                Proc.Start();

                StreamReader StandardOutput = Proc.StandardOutput;
                StreamReader StandardError = Proc.StandardError;

                string NetStatContent = StandardOutput.ReadToEnd() + StandardError.ReadToEnd();
                string NetStatExitStatus = Proc.ExitCode.ToString();

                if (NetStatExitStatus != "0")
                {
                    Console.WriteLine("NetStat command failed.   This may require elevated permissions.");
                }

                string[] NetStatRows = Regex.Split(NetStatContent, "\r\n");

                foreach (string NetStatRow in NetStatRows)
                {
                    string[] Tokens = Regex.Split(NetStatRow, "\\s+");
                    if (Tokens.Length > 4 && (Tokens[1].Equals("UDP") || Tokens[1].Equals("TCP")))
                    {
                        string IpAddress = Regex.Replace(Tokens[2], @"\[(.*?)\]", "1.1.1.1");
                        try
                        {
                            ProcessPorts.Add(new ProcessPort(
                                Tokens[1] == "UDP" ? GetProcessName(Convert.ToInt16(Tokens[4])) : GetProcessName(Convert.ToInt16(Tokens[5])),
                                Tokens[1] == "UDP" ? Convert.ToInt16(Tokens[4]) : Convert.ToInt16(Tokens[5]),
                                IpAddress.Contains("1.1.1.1") ? String.Format("{0}v6", Tokens[1]) : String.Format("{0}v4", Tokens[1]),
                                Convert.ToInt32(IpAddress.Split(':')[1])
                            ));
                        }
                        catch
                        {
                            Console.WriteLine("Could not convert the following NetStat row to a Process to Port mapping.");
                            Console.WriteLine(NetStatRow);
                        }
                    }
                    else
                    {
                        if (!NetStatRow.Trim().StartsWith("Proto") && !NetStatRow.Trim().StartsWith("Active") && !String.IsNullOrWhiteSpace(NetStatRow))
                        {
                            Console.WriteLine("Unrecognized NetStat row to a Process to Port mapping.");
                            Console.WriteLine(NetStatRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ProcessPorts;
        }

        /// <summary>
        /// Private method that handles pulling the process name (if one exists) from the process id.
        /// </summary>
        /// <param name="ProcessId"></param>
        /// <returns></returns>
        private static string GetProcessName(int ProcessId)
        {
            string procName = "UNKNOWN";

            try
            {
                procName = Process.GetProcessById(ProcessId).ProcessName;
            }
            catch { }

            return procName;
        }
    }
}
