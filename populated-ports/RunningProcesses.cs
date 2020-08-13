using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace populated_ports
{
    // https://github.com/2gis/Winium.Desktop/pull/71/commits/3118da30abcc703b1866487ca6c848277131459c
    /// <summary>
    ///     Static class that returns the list of processes and the ports those processes use.
    /// </summary>
    public static class RunningProcesses
    {
        /// <summary>
        ///     A list of ProcesesPorts that contain the mapping of processes and the ports that the process uses.
        /// </summary>
        public static List<ProcessPort> Ports => GetNetStatPorts();


        /// <summary>
        ///     This method distills the output from netstat -a -n -o into a list of ProcessPorts that provide a mapping between
        ///     the process (name and id) and the ports that the process is using.
        /// </summary>
        /// <returns></returns>
        private static List<ProcessPort> GetNetStatPorts()
        {
            var processPorts = new List<ProcessPort>();

            try
            {
                using var proc = new Process();
                var startInfo = new ProcessStartInfo();
                startInfo.FileName = "netstat.exe";
                startInfo.Arguments = "-a -n -o";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;

                proc.StartInfo = startInfo;
                proc.Start();

                var standardOutput = proc.StandardOutput;
                var standardError = proc.StandardError;

                var netStatContent = standardOutput.ReadToEnd() + standardError.ReadToEnd();
                var netStatExitStatus = proc.ExitCode.ToString();

                if (netStatExitStatus != "0")
                    Console.WriteLine("NetStat command failed.   This may require elevated permissions.");

                var netStatRows = Regex.Split(netStatContent, "\r\n");

                foreach (var netStatRow in netStatRows)
                {
                    var tokens = Regex.Split(netStatRow, "\\s+");
                    if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
                    {
                        var ipAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                        try
                        {
                            processPorts.Add(new ProcessPort(
                                tokens[1] == "UDP"
                                    ? GetProcessName(Convert.ToInt16(tokens[4]))
                                    : GetProcessName(Convert.ToInt16(tokens[5])),
                                tokens[1] == "UDP" ? Convert.ToInt16(tokens[4]) : Convert.ToInt16(tokens[5]),
                                ipAddress.Contains("1.1.1.1")
                                    ? $"{tokens[1]}v6"
                                    : $"{tokens[1]}v4",
                                Convert.ToUInt32(ipAddress.Split(':')[1])
                            ));
                        }
                        catch
                        {
                            Console.WriteLine(
                                "Could not convert the following NetStat row to a Process to Port mapping.");
                            Console.WriteLine(netStatRow);
                        }
                    }
                    else
                    {
                        if (!netStatRow.Trim().StartsWith("Proto") && !netStatRow.Trim().StartsWith("Active") &&
                            !string.IsNullOrWhiteSpace(netStatRow))
                        {
                            Console.WriteLine("Unrecognized NetStat row to a Process to Port mapping.");
                            Console.WriteLine(netStatRow);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return processPorts;
        }

        /// <summary>
        ///     Private method that handles pulling the process name (if one exists) from the process id.
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        private static string GetProcessName(int processId)
        {
            var procName = "UNKNOWN";

            try
            {
                procName = Process.GetProcessById(processId).ProcessName;
            }
            catch
            {
                // ignored
            }

            return procName;
        }
    }
}