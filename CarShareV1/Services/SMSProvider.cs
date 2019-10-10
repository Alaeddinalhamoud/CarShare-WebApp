using CarShareV1.Data;
using Nexmo.Api;
using System.Linq;

namespace CarShareV1.Services
{

    public class SMSProvider: ISMSProvider
    { 

    private readonly ApplicationDbContext _context;
    public SMSProvider(ApplicationDbContext context)
    {
        _context = context;
    }


    public void SendSMS(string ClientMobileNumber, string textmsg)
    {
        var _Setting = _context.WebSiteSettings.FirstOrDefault();
        string key = _Setting.MobileApiKey;
        string secret = _Setting.MobileApiSecret;
        string name = _Setting.MobileWebsiteName;

        var client = new Client(creds: new Nexmo.Api.Request.Credentials
        {
            ApiKey = key,
            ApiSecret = secret
        });
        var results = client.SMS.Send(request: new SMS.SMSRequest
        {
            from = name,
            to = ClientMobileNumber,
            text = textmsg
        });
    }

         
    }
}
