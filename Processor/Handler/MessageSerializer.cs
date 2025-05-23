using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Processor.Handler
{
	public class MessageSerializer
	{
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
			{ 1, MyMessage.Descriptor },
			{ 2, AnotherMessage.Descriptor },
			{ 3, VegetableMessage.Descriptor },
		};

		private Dictionary<MessageDescriptor, int> ReverseLookup;

		public MessageSerializer()
		{
			foreach (var t in typeof(MessageSerializer).Assembly.DefinedTypes.Where(x => x.ImplementedInterfaces.Contains(typeof(IMessage))))
			{
				Console.WriteLine(t.FullName);
			}

			ReverseLookup = new Dictionary<MessageDescriptor, int>();
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

		public static byte[] SerializeMessage(int messageType, IMessage message)
		{
			using var payloadStream = new MemoryStream();
			var s = message.Descriptor.Declaration;
			message.WriteTo(payloadStream);
			var payload = payloadStream.ToArray();

			using var finalStream = new MemoryStream();
			WriteVarint(finalStream, payload.Length);
			WriteVarint(finalStream, messageType);
			finalStream.Write(payload, 0, payload.Length);

			return finalStream.ToArray();
		}

		private static void WriteVarint(Stream stream, int value)
		{
			uint uValue = (uint)value;
			while (uValue >= 0x80)
			{
				stream.WriteByte((byte)(uValue | 0x80));
				uValue >>= 7;
			}
			stream.WriteByte((byte)uValue);
		}
	}
}