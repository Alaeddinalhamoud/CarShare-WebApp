using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }
        public string BranchName { get; set; }      
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UserId { get; set; }
    }
}
