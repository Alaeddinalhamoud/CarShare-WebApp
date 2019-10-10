using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CarShareV1.Data
{
    public class GooglereCaptchaService
    {   
        private ReCAPTCHASettings _settings;
        public GooglereCaptchaService(IOptions<ReCAPTCHASettings> settings)
        {
            _settings = settings.Value;
        }
        public virtual async Task<GoogleResponse> VerifyreCaptcha(string _Token)
        {
            GooglereCaptchaData _MyData = new GooglereCaptchaData()
            {
                response = _Token,
                secret = _settings.ReCAPTCHA_Secret_Key
            };
            HttpClient client = new HttpClient();

            var _response = await client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={_MyData.secret}&response={_MyData.response}");
            var captchaResponse = JsonConvert.DeserializeObject<GoogleResponse>(_response);
            return captchaResponse;
        }
    }

    public class GooglereCaptchaData
    {
        public string response { get; set; }
        public string secret { get; set; }
    }

    public class GoogleResponse
    {
        public bool success { get; set; }
        public double score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
