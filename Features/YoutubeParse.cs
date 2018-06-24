//snippet of code found somewhere around StackOverflow. Whoever is the guy who wrote this: Thank you, it saved me some huge work.
using System;
using System.Web;
using System.Net;

namespace EleunameBotConsole
{
    public class YoutubeParse
    {


        public static string GetTitle(string url)
        {

            var api = $"http://youtube.com/get_video_info?video_id={GetArgs(url, "v", '?')}";
            return GetArgs(new WebClient().DownloadString(api), "title", '&');

        }


        private static string GetArgs(string args, string key, char query)
        {
            var iqs = args.IndexOf(query);
            return iqs == -1
                ? string.Empty
                : HttpUtility.ParseQueryString(iqs < args.Length - 1
                    ? args.Substring(iqs + 1) : string.Empty)[key];
        }

    }
}
