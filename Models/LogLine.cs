namespace Echelon.Bot.Models
{
    public class LogLine
    {
        private readonly string line;

        public LogLine(string line)
        {
            this.line = line;
        }

        public DateTime Date
        {
            get
            {
                var date = line.Split("]").First().Trim('[');
                return DateTime.Parse(date);
            }
        }

        public string Text => line.Split("]").Skip(1).First().Trim();
    }
}

