using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public class Agreement
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
