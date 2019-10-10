namespace CarShareV1.Services
{
    public interface ISMSProvider
    {
         void SendSMS(string ClientMobileNumber, string textmsg);
    }
}
