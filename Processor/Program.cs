using Google.Protobuf;
using Processor.Handler;
using Processor.Protobuf.MessageTypes;

namespace Processor
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var serialiser = new MessageSerializer
			{
				IncludeDiagnostics = true
			};

			var buffer = new MemoryStream();

			var messageList = new List<IMessage>
			{
				new NoOperation{ },
				new MyMessage { Id = 123, Name = "Test" },
				new MyMessage { Id = 0, Name = "!" },
				new AnotherMessage { Id = 222, Name = "thg", Value = 321 },
				new VegetableMessage { Id = 12345, Value = 32767},
				new VulcanMessage { Activated = true , Value = 12394785},
				new VulcanMessage { Activated = false , Value = 8}
			};

			Console.WriteLine($"Sending");
			Console.WriteLine($"=======");
			foreach (var message in messageList)
			{
				byte[] serialized = serialiser.Serialise(message);
				buffer.Write(serialized, 0, serialized.Length);
			}

			Console.WriteLine();
			buffer.Position = 0;

			Console.WriteLine($"Reading");
			Console.WriteLine($"=======");
			Console.WriteLine($"[Length + Type + Payload]");

			ReadData(buffer, serialiser);

			//using var client = new TcpClient("localhost", 5000);
			//using var stream = client.GetStream();
			//stream.Write(serialized, 0, serialized.Length);

			Console.WriteLine("Message sent.");
		}

		public static void ReadData(Stream incoming, MessageSerializer serialiser)
		{
			var handler = new LengthPrefixedMessageHandler(serialiser);
			handler.MessageReceived += (sender, e) =>
			{
				Console.WriteLine($"{e.MessageType}");
				Console.WriteLine($"{e.Message.Descriptor.ClrType}: {e.Message}");
			};

			//var listener = new TcpListener(IPAddress.Any, 5000);
			//listener.Start();
			//Console.WriteLine("Listening for connections...");

			while (true)
			{
				//using var client = listener.AcceptTcpClient();
				//using var stream = client.GetStream();
				var stream = incoming;
				var buffer = new byte[1024];
				int bytesRead;

				while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					handler.ReceiveData(buffer[..bytesRead]);
				}
			}
		}
	}
}