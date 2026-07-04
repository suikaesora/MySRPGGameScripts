using UnityEngine;

namespace Unity2DGameTemplate
{
    public static class XPostSystem
    {
        /// <summary>
        /// ゲームURL無しでポスト
        /// </summary>
        public static void Post(string content, string[] hashTags)
        {
            string url = "";
            url += "https://x.com/intent/post?";
            url += "text=" + content;
            if (hashTags != null && hashTags.Length > 0)
            {
                url += "&hashtags=" + string.Join(',', hashTags);
            }
            Application.OpenURL(url);
        }

        /// <summary>
        /// ゲームURL付きでポスト
        /// </summary>
        public static void PostWithGameURL(string content, string[] hashTags)
        {
            string url = "";
            url += "https://x.com/intent/post?";
            url += "text=" + content;
            url += "&url=" + Application.absoluteURL;
            if (hashTags != null && hashTags.Length > 0)
            {
                url += "&hashtags=" + string.Join(',', hashTags);
            }
            Application.OpenURL(url);
        }
    }
}