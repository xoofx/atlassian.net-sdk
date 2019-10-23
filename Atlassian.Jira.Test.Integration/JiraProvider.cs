using System.Collections;
using System.Collections.Generic;

namespace Atlassian.Jira.Test.Integration
{
    internal class JiraProvider : IEnumerable<object[]>
    {
        public const string HOST = "http://localhost:8080";

        public const string USERNAME = "admin";
        public const string PASSWORD = "admin";

        public const string OAUTHCONSUMERKEY = "JiraSdkConsumerKey";
        public const string OAUTHCONSUMERSECRET = "<RSAKeyValue><Modulus>odq47HoOGrM8b4FsbcaD+RpBCP1tNsKOcnH5wVd0XmgkEsiOaGiSUx1r9EEk9bBw2/Hq3DAXqbswOQOVt9WOPM27bjKDFJ2fkhFok//I3Wnsv/ZBpCHfyCT4dr9n8vd2HNVbDS4VqDNmoBbVs1Efkgw8ybcgsmGqqT7WZYmmSa8=</Modulus><Exponent>AQAB</Exponent><P>1HIXunp5C8dntdrNhOItLYYexBHDPSPCemdTLcoGIPrdGs+YgNHwpzjKAa89EToguijzxUigiZXbXLsimy+Whw==</P><Q>wwlrDSX0kV8b4jA5qDsoWN33h8BWotHq2YtajY2AyB7/MwmoBtWasQB1SxJcrILetbOqiTzJaZNdmQqg9RHVmQ==</Q><DP>abZYLlexEfZomepFqCDvwB5kAsaf8zVvGX9+uWM0x4ZtLWEtjrRo3pz4j/wGFCNrk5a7LmkkUTI7lJod70Cv0w==</DP><DQ>lxx/9eL3d36iIwDMW1ziaOApveM2/NX5yO2gjlYZdnQVtByCNDFhtkwtlKm4ZezL0ypOMiCHySXlegLzLI3R2Q==</DQ><InverseQ>mxXqH+teLS/8SgdBDi6cs5huMwXe7zAz33noZeiyi7Xm2ciyjvheCGFF201wBXehUemxMqmLGTCLWMBp0qsmnA==</InverseQ><D>VYeHgS9elK1ymloCOmBVDSXaiC2jsPRO4htop8rXK6xMo8BnwLTB3joF+iUSquJ6QUAto/2mA4NvkDFcxLCNYKziSj1JWIbfcc6gqPIKwtxyM3ZlSuJaG6GpNPh41SEhjtgMt2Cbf5Qy/prK1FkWFfOcvlOg+z2qGPQDXhS0QIE=</D></RSAKeyValue>";
        public const string OAUTHACCESSTOKEN = "ZGUlzyOnuzS929YgIXv6Yt0TiZ8KbUAG";
        public const string OAUTHTOKENSECRET = "EDeTxUt7QqDkoawenPY3QCaGeVGXa1BJ";

        private static Jira _jiraWithCredentials;
        private static Jira _jiraWithOAuth;

        private readonly List<object[]> _data;

        static JiraProvider()
        {
            _jiraWithCredentials = Jira.CreateRestClient(HOST, USERNAME, PASSWORD);
            _jiraWithOAuth = Jira.CreateOAuthRestClient(
                HOST,
                OAUTHCONSUMERKEY,
                OAUTHCONSUMERSECRET,
                OAUTHACCESSTOKEN,
                OAUTHTOKENSECRET);
        }

        public JiraProvider()
        {
            _data = new List<object[]>
            {
                new object[] { _jiraWithCredentials },
                new object[] { _jiraWithOAuth }
            };
        }

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
