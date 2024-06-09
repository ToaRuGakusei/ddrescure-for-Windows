using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ddrescue_for_Windows
{
    public sealed class LineNotify
    {
        public async void PushMessage(string TOKEN,string message)
        {
            var ACCESS_TOKEN = TOKEN;

            using (var c = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string> { { "message", message } });

                //ヘッダにアクセストークンを追加
                c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ACCESS_TOKEN);

                var result = await c.PostAsync("https://notify-api.line.me/api/notify", content);
            }
        }
    }
}
