using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public  class EmailSetting
    {
        [Key]
        public int Id { get; set; }
        public string FromEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Smtp { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UserId { get; set; }
    }
}
