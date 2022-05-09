using System;

namespace LindormStudio
{
    public class LindormExecuteSqlException : Exception
    {
        public int Code { get; set; }
        public string SqlState { get; set; } = string.Empty;
        public new string Message { get; set; } = string.Empty;
        public override string ToString()
        {
            return $"{Message}\n\nCode: {Code}\nSqlState: {SqlState}";
        }
    }
}
