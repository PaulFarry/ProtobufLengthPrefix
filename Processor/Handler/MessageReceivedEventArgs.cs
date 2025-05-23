using Google.Protobuf;

namespace Processor.Handler
{
	public class MessageReceivedEventArgs : EventArgs
	{
		public int MessageType { get; }
		public IMessage Message { get; }

		public MessageReceivedEventArgs(int messageType, IMessage message)
		{
			MessageType = messageType;
			Message = message;
		}
	}
}