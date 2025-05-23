using Google.Protobuf;
using Google.Protobuf.Reflection;
using Processor.Protobuf.MessageTypes;

namespace Processor.Handler
{
	public class MessageSerializer
	{
		public bool IncludeDiagnostics { get; set; }

		/// <summary>
		/// Contains the mapping for the types.
		/// So that the types themsevles don't have unnecessary data, we define them once here.
		/// </summary>
		/// <remarks>
		/// Once a value has been defined it CANNOT be changed or deleted unless you handle all storage of that value
		/// 
		/// </remarks>
		private Dictionary<int, MessageDescriptor> DescriptorLookup = new()
		{
			{ 0, NoOperation.Descriptor }, //
			{ 1, MyMessage.Descriptor },
			{ 2, AnotherMessage.Descriptor },
			{ 3, VegetableMessage.Descriptor },
			{ 4, VulcanMessage.Descriptor },
		};

		private Dictionary<MessageDescriptor, int> ReverseLookup;

		public MessageSerializer()
		{

			//foreach (var t in typeof(MessageSerializer).Assembly.DefinedTypes.Where(x => x.ImplementedInterfaces.Contains(typeof(IMessage))))
			//{
			//	Console.WriteLine(t.FullName);
			//}

			ReverseLookup = [];
			foreach (var kvp in DescriptorLookup)
			{
				ReverseLookup.Add(kvp.Value, kvp.Key);
			}
		}

		public MessageDescriptor GetDescriptor(int type)
		{
			return DescriptorLookup[type];
		}

		public int DetermineType(IMessage message)
		{
			if (ReverseLookup.TryGetValue(message.Descriptor, out var type))
			{
				Console.WriteLine(type);
				return type;
			}
			return -1;
		}

		public byte[] Serialise(IMessage message)
		{
			var t = DetermineType(message);

			return SerializeMessage(t, message);
		}

		public byte[] SerializeMessage(int messageType, IMessage message)
		{
			using var payloadStream = new MemoryStream();
			message.WriteTo(payloadStream);
			var payload = payloadStream.ToArray();

			using var finalStream = new MemoryStream();
			var lengthBytes = WriteVarint(finalStream, payload.Length);
			var typeBytes = WriteVarint(finalStream, messageType);
			if (payload.Length > 0)
			{
				finalStream.Write(payload, 0, payload.Length);
			}
			if (IncludeDiagnostics)
			{
				var totalb = lengthBytes + typeBytes + payload.Length;
				Console.WriteLine($"{message.Descriptor.Name}: {message} | Size(bytes) {totalb} = [{lengthBytes} + {typeBytes} + {payload.Length}] ");
			}
			return finalStream.ToArray();
		}

		private static int WriteVarint(Stream stream, int value)
		{
			var bytesWritten = 0;
			uint uValue = (uint)value;
			while (uValue >= 0x80)
			{
				stream.WriteByte((byte)(uValue | 0x80));
				bytesWritten++;
				uValue >>= 7;
			}
			stream.WriteByte((byte)uValue);
			bytesWritten++;
			return bytesWritten;
		}
	}
}