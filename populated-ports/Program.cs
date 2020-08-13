using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace populated_ports
{
    internal class Program
    {
        private static readonly List<int> PopulatedPorts = new List<int>
        {
            4200,
            5000,
            5001
        };

        private static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Listening for overlapping ports on ports " + string.Join(", ", PopulatedPorts) +
                                  "...");
                CheckForPopulatedPorts(PopulatedPorts);
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }
        }

        private static void CheckForPopulatedPorts(ICollection<int> populatedPorts)
        {
            var processesTakingMoreThanOnePort = RunningProcesses.Ports
                .Where(process => populatedPorts
                    .Contains(process.PortNumber))
                .GroupBy(x => x.PortNumber)
                .Where(processesGroup => processesGroup
                    .Count() > 1);

            foreach (var processesGroup in processesTakingMoreThanOnePort)
            {
                var overlappingSameProtocol = processesGroup
                    .GroupBy(x => x.Protocol)
                    .Select(n => new
                    {
                        Protocol = n.Key,
                        Count = n.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .First().Count;

                if (overlappingSameProtocol == 0) continue;
                Console.WriteLine("Port conflict found:");
                Console.WriteLine("====================");
                AlertUser();
                Console.WriteLine("Port " + processesGroup.Key + ":");
                foreach (var aProcess in processesGroup) Console.WriteLine(aProcess.ProcessPortDescription);

                Console.WriteLine("Would you like to close all above processes? Unsaved data will be lost (y/n):");
                Reader.TryReadLine(out var userResponse, (int) TimeSpan.FromSeconds(60).TotalMilliseconds);
                if (userResponse == null || !userResponse.Equals("y")) continue;

                Console.WriteLine("Should I close all docker containers that are using that port? (y/n):");
                Reader.TryReadLine(out var dockerResponse, (int) TimeSpan.FromSeconds(5).TotalMilliseconds);
                Console.WriteLine("Closing processes...");
                QuitProcesses(processesGroup, dockerResponse.Equals("y"));
            }
        }

        private static void QuitProcesses(IGrouping<int, ProcessPort> processesGroup, bool shouldQuitDocker = false)
        {
            foreach (var processToClose in from aProcess in processesGroup
                    .Where(process => !process.Name.Contains("com.docker.backend"))
                    .Select(process => process.ProcessId)
                let processToClose = Process.GetProcessById(aProcess)
                select processToClose)
            {
                processToClose.CloseMainWindow();
                processToClose.WaitForExit(5000);
                processToClose.Kill();
            }

            if (shouldQuitDocker && processesGroup.Any(x => x.Name.Contains("com.docker.backend")))
                QuitDockerContainersAsync();
        }

        private static void QuitDockerContainersAsync()
        {
            var client = new DockerClientConfiguration(
                    new Uri("npipe://./pipe/docker_engine"))
                .CreateClient();

            var containers = client.Containers.ListContainersAsync(new ContainersListParameters()).Result;

            containers.Select(container => new {container.ID, container.Ports})
                .Where(port => port.Ports
                    .Select(p => PopulatedPorts
                        .Contains(p.PublicPort))
                    .Any() || port.Ports
                    .Select(p => PopulatedPorts
                        .Contains(p.PrivatePort))
                    .Any())
                .Select(c => c.ID)
                .ToList()
                .ForEach(h => client.Containers
                    .StopContainerAsync(h, new ContainerStopParameters {WaitBeforeKillSeconds = 7})
                    .Wait());
        }

        private static void AlertUser()
        {
            Console.Beep();
            Console.Beep();
            Console.Beep();
        }
    }
}