using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarShare.Data
{
  public  class Insurance
    {
        [Key]
        public int Id { get; set; }
        public  string PolicyNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UserId { get; set; }
    }
}
