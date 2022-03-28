using Echelon.Bot.Models;
using System.Collections.Concurrent;

namespace Echelon.Bot.Components
{
    public class QueueComponent
    {
        private readonly IMessageWriter messageWriter;
        private readonly string logFile = "log.txt";
        private readonly ConcurrentQueue<OutboundMessage> messages = new();
        private readonly ConcurrentQueue<OutboundMessage> outboundMessages = new();
        private readonly static ReaderWriterLockSlim readerWriterLockSlim = new();

        public QueueComponent(IMessageWriter messageWriter)
        {
            this.messageWriter = messageWriter;
        }

        public void QueueMessage(OutboundMessage message)
        {
            if (IsPrinted(message.Text))
            {
                messageWriter.Write("Message already sent");
                return;
            }
            messages.Enqueue(message);
        }

        bool IsPrinted(string? link)
        {
            readerWriterLockSlim.EnterReadLock();
            var fileContent = File.ReadAllText(logFile);
            readerWriterLockSlim.ExitReadLock();
            return fileContent.Contains(link!);
        }

        public async Task WriteToFile()
        {
            readerWriterLockSlim.EnterWriteLock();
            using var writer = File.AppendText(logFile);
            while (messages.TryDequeue(out var message))
            {
                await writer.WriteAsync($"[{DateTime.Now}] {message.Text} {Environment.NewLine}");                
                outboundMessages.Enqueue(message);
                messageWriter.Write($"Writing to file: {message.Text}");
            }
            writer.Flush();
            readerWriterLockSlim.ExitWriteLock();
        }

        public ConcurrentQueue<OutboundMessage> GetOutboundMessages() => outboundMessages;
    }
}

