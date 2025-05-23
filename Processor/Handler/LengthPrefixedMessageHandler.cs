using Google.Protobuf;

namespace Processor.Handler
{
	public class LengthPrefixedMessageHandler
	{
		private readonly MemoryStream _buffer = new();
		private readonly MessageSerializer serialiser;

		public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

		public LengthPrefixedMessageHandler(MessageSerializer serialiser)
		{
			this.serialiser = serialiser!;
		}

		public void ReceiveData(byte[] data)
		{
			_buffer.Write(data, 0, data.Length);
			_buffer.Position = 0;

			while (TryReadMessage(out var messageType, out var message))
			{
				MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageType, message!));
			}

			var remaining = _buffer.Length - _buffer.Position;
			if (remaining > 0)
			{
				var leftover = new byte[remaining];
				_buffer.Read(leftover, 0, (int)remaining);
				_buffer.SetLength(0);
				_buffer.Write(leftover, 0, leftover.Length);
			}
			else
			{
				_buffer.SetLength(0);
			}
		}

		private bool TryReadMessage(out int messageType, out IMessage? message)
		{
			messageType = 0;
			message = null;

			long startPos = _buffer.Position;
			if (!TryReadVarint(_buffer, out int length)) return false;
			if (!TryReadVarint(_buffer, out messageType)) return false;

			if (_buffer.Length - _buffer.Position < length)
			{
				_buffer.Position = startPos;
				return false;
			}

			var messageDescriptor = serialiser.GetDescriptor(messageType) ?? throw new InvalidOperationException($"Unknown message type: {messageType}");

			var payload = new byte[length];
			_buffer.Read(payload, 0, length);
			using var payloadStream = new MemoryStream(payload);
			message = messageDescriptor.Parser.ParseFrom(payloadStream);
			return true;
		}

		private static bool TryReadVarint(Stream stream, out int value)
		{
			value = 0;
			int shift = 0;
			int b;

			while (shift < 32)
			{
				int read = stream.ReadByte();
				if (read == -1) return false;

				b = read;
				value |= (b & 0x7F) << shift;
				if ((b & 0x80) == 0) return true;

				shift += 7;
			}

			return false;
		}
	}
}
