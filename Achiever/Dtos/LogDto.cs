using System;

namespace Achiever.Dtos
{
    public class LogDto
    {
        public int id { get; set; }
        public int aId { get; set; }
        public string desc { get; set; }
        public DateTime timestamp { get; set; }
        public bool doubled { get; set; }
        public int count { get; set; }
        public int count2 { get; set; }
        public int penalty { get; set; }
    }
}