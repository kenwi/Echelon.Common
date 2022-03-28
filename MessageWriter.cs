public class MessageWriter : IMessageWriter
{
    public void Write(string message)
    {
        Console.WriteLine($"[{DateTime.Now}] {message.ReplaceLineEndings($"{Environment.NewLine}[{DateTime.Now}] ")}");
    }
}
