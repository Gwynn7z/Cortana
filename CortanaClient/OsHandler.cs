using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CortanaClient;

internal enum Os
{
	Linux,
	Windows
}

internal struct OsMacro
{
	private const string BashPath = "/bin/bash";
	private const string CmdPath = "cmd.exe";
	private const string BashArg = "-c";
	private const string CmdArg = "/C";

	public static string GetPath(Os operatingSystem)
	{
		return operatingSystem == Os.Linux ? BashPath : CmdPath;
        
	}
	public static string GetArg(Os operatingSystem)
	{
		return operatingSystem == Os.Linux ? BashArg : CmdArg; 
	}
}

internal static class OsHandler
{
	private static readonly Os OperatingSystem;

	static OsHandler()
	{
		if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) OperatingSystem = Os.Linux;
		else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) OperatingSystem = Os.Windows;
		else throw new Exception("Unsupported Operating System");
	}

	internal static string DecodeCommand(string command, string arg = "")
	{
		bool onLinux = OperatingSystem == Os.Linux;
		return command switch
		{
			"shutdown" => onLinux ? "poweroff" : "shutdown /s",
			"reboot" => onLinux ? "reboot" : "shutdown /r",
			"notify" => onLinux ? $"notify-send -u low -a Cortana \'{arg}\'" : $"notify-send \"Cortana\" \"{arg}\"",
			"cmd" => arg,
			_ => ""
		};
	}

	internal static void ExecuteCommand(string arg)
	{
		string path = OsMacro.GetPath(OperatingSystem);
		string param = OsMacro.GetArg(OperatingSystem);

		var process = new Process()
		{
			StartInfo = new ProcessStartInfo
			{
				FileName = path,
				Arguments = $"{param} \"{arg}\"",
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			}
		};
		process.OutputDataReceived += (sender, args) => ComputerClient.Write(args.Data ?? "Empty Response");
		process.Start();
	}
}