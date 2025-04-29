using System.Diagnostics;
using System.Runtime.InteropServices;

namespace McpToolkit.Client;

internal static class ProcessHelper
{
    public static void KillTree(this Process process)
    {
        var timeout = TimeSpan.FromSeconds(30);
        var pid = process.Id;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            RunProcessAndWaitForExit(
                "taskkill",
                $"/T /F /PID {pid}",
                timeout,
                out var _);
        }
        else
        {
            var children = new HashSet<int>();
            GetAllChildIdsUnix(pid, children, timeout);
            foreach (var childId in children)
            {
                KillProcessUnix(childId, timeout);
            }
            KillProcessUnix(pid, timeout);
        }

        process.WaitForExit((int)timeout.TotalMilliseconds);
    }

    static void GetAllChildIdsUnix(int parentId, ISet<int> children, TimeSpan timeout)
    {
        var exitcode = RunProcessAndWaitForExit(
            "pgrep",
            $"-P {parentId}",
            timeout,
            out var stdout);

        if (exitcode == 0 && !string.IsNullOrEmpty(stdout))
        {
            using var reader = new StringReader(stdout);
            while (true)
            {
                var text = reader.ReadLine();
                if (text == null)
                    return;

                if (int.TryParse(text, out var id))
                {
                    children.Add(id);
                    GetAllChildIdsUnix(id, children, timeout);
                }
            }
        }
    }

    static void KillProcessUnix(int processId, TimeSpan timeout)
    {
        RunProcessAndWaitForExit(
            "kill",
            $"-TERM {processId}",
            timeout,
            out var _);
    }

    static int RunProcessAndWaitForExit(string fileName, string arguments, TimeSpan timeout, out string? stdout)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        stdout = null;

        var process = Process.Start(startInfo);
        if (process == null) return -1;

        if (process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            stdout = process.StandardOutput.ReadToEnd();
        }
        else
        {
            process.Kill();
        }

        return process.ExitCode;
    }
}