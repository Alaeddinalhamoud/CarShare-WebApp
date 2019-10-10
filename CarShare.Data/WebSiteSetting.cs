using System;
using System.ComponentModel.DataAnnotations;

namespace CarShare.Data
{
    public   class WebSiteSetting
    {
        [Key]
        public int Id { get; set; }
        public string AddressAPI { get; set; }
        public string DVLAAPI { get; set; }
        public string PaymentAPI { get; set; }
        public string MobileApiKey { get; set; }
        public string MobileApiSecret { get; set; }
        public string MobileWebsiteName { get; set; }
        public string UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
