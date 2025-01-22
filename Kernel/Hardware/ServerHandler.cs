using System.Net;
using System.Net.Sockets;
using System.Text;
using Kernel.Hardware.ClientHandlers;

namespace Kernel.Hardware;

internal static class ServerHandler
{
	private static readonly Socket Server;
	
	static ServerHandler()
	{
		var ipEndPoint = new IPEndPoint(IPAddress.Any, HardwareSettings.NetworkData.ServerPort);
		Server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		Server.Bind(ipEndPoint);
	}
	
	internal static async Task StartListening()
	{
		Server.Listen();
		
		var bListening = true;
		while (bListening)
		{
			try
			{
				Socket socket = await Server.AcceptAsync();
				_ = Task.Run(() => HandleConnection(socket));
			}
			catch
			{
				Server.Close();
				bListening = false;
			}
		}
	}

	private static void HandleConnection(Socket socket)
	{
		var buffer = new byte[1024];
		int received = socket.Receive(buffer);
		string message = Encoding.UTF8.GetString(buffer, 0, received);

		string answer = "ACK";
		switch (message)
		{
			case "computer":
				ComputerHandler.BindNew(new ComputerHandler(socket));
				break;
			
			case "esp32":
				SensorsHandler.BindNew(new SensorsHandler(socket));
				break;
			default:
				answer = "FIN";
				break;
		}
		socket.Send(Encoding.UTF8.GetBytes(answer));
		if(answer == "FIN") socket.Close();
	}

	internal static void ShutdownServer()
	{
		Server.Close();
	}
}