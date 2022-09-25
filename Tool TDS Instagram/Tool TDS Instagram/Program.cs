using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace Tool_TDS_Instagram
{
    internal class Program
    {
        [Serializable]
        public class TDSLoginException : Exception
        {
            public TDSLoginException() : base() { }
            public TDSLoginException(string message) : base(message) { }
            public TDSLoginException(string message, Exception inner) : base(message, inner) { }
        }
        [Serializable]
        public class TDSJobException : Exception
        {
            public TDSJobException() : base() { }
            public TDSJobException(string message) : base(message) { }
            public TDSJobException(string message, Exception inner) : base(message, inner) { }
        }
        [Serializable]
        public class InstagramLoginException : Exception
        {
            public InstagramLoginException() : base() { }
            public InstagramLoginException(string message) : base(message) { }
            public InstagramLoginException(string message, Exception inner) : base(message, inner) { }
        }
        public class TDSTypeJob
        {
            public static string Like { get { return "instagram_like"; } }
            public static string Follow { get { return "instagram_follow"; } }
            public static string Comment { get { return "instagram_comment"; } }
            public static string LikeCMT { get { return "instagram_likecmt"; } }
        }
        public class TDSTypeJobCache
        {
            public static string Like { get { return "INS_LIKE_CACHE"; } }
            public static string Follow { get { return "INS_FOLLOW_CACHE"; } }
            public static string Comment { get { return "INS_COMMENT_CACHE"; } }
            public static string LikeCMT { get { return "INS_LIKECMT_CACHE"; } }
        }
        public struct TDSJob
        {
            public string Id { get; set; }
            public string Link { get; set; }
            public string Cmt { get; set; }
        }
        public class TDS
        {
            private string token { get; set; }
            private Request request = new Request();
            public TDS(string token = null)
            {
                if (!request.Get($"https://traodoisub.com/api/?fields=profile&access_token={token}").Replace(" ", "").Contains("\"success\":200")) throw new TDSLoginException("token wrong");
                this.token = token;
            }
            public bool SetNick(string id)
            {
                return request.Get($"https://traodoisub.com/api/?fields=instagram_run&id={id}&access_token={token}").Replace(" ", "").Contains("\"success\":200");
            }
            public string PostXu(string id, string type)
            {
                return Regex.Match(request.Get($"https://traodoisub.com/api/coin/?type={type}&id={id}&access_token={token}").Replace(" ", ""), "\"pending\":(.*?),").Groups[1].Value;
            }
            public List<TDSJob> GetJobs(string type)
            {
                string response = request.Get($"https://traodoisub.com/api/?fields={type}&access_token={token}").Replace(" ", "").Replace("\r", "").Replace("\n", "");
                List<TDSJob> tDSJobs = new List<TDSJob>();
                foreach (Match match in Regex.Matches(response, "\"id\":\"(.*?)\",\"link\":\"(.*?)\""))
                {
                    tDSJobs.Add(new TDSJob() { Id = match.Groups[1].Value, Link = match.Groups[2].Value.Replace("\\/", "/") });
                }
                return tDSJobs;
            }
        }
        public struct Headers
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public class Request
        {
            private List<Headers> headers = new List<Headers>();
            public string UserAgent { get; set; }
            public string Cookie { get; set; }
            public WebProxy WebProxy { get; set; }
            public Request()
            {
                WebProxy = null;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.DefaultConnectionLimit = 256;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;
            }
            public void AddHeader(string name, string value)
            {
                headers.Add(new Headers() { Name = name, Value = value });
            }
            public void DeleteHeader(string name)
            {
                headers.Remove(headers.Where(x => x.Name == name).First());
            }
            public void ClearHeader()
            {
                headers.Clear();
            }
            public string Post(string uri, string postData, string contentType = "application/x-www-form-urlencoded")
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                if (!string.IsNullOrEmpty(Cookie))
                {
                    request.Headers["Cookie"] = Cookie;
                }
                if (!string.IsNullOrEmpty(UserAgent))
                {
                    request.UserAgent = UserAgent;
                }
                foreach (var header in headers)
                {
                    request.Headers[header.Name] = header.Value;
                }
                if (WebProxy != null)
                {
                    request.Proxy = WebProxy;
                }
                request.Method = "POST";
                var data = Encoding.UTF8.GetBytes(postData);
                request.ContentType = contentType;
                request.ContentLength = data.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            public string Get(string uri)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Method = "GET";
                    if (!string.IsNullOrEmpty(Cookie))
                    {
                        request.Headers["Cookie"] = Cookie;
                    }
                    if (!string.IsNullOrEmpty(UserAgent))
                    {
                        request.UserAgent = UserAgent;
                    }
                    foreach (var header in headers)
                    {
                        request.Headers[header.Name] = header.Value;
                    }
                    if (WebProxy != null)
                    {
                        request.Proxy = WebProxy;
                    }
                    var response = (HttpWebResponse)request.GetResponse();
                    return new StreamReader(response.GetResponseStream()).ReadToEnd();
                }
                catch (WebException ex)
                {
                    var response = (HttpWebResponse)ex.Response;
                    if (response != null)
                    {
                        return new StreamReader(response.GetResponseStream()).ReadToEnd();
                    }
                }
                return "";
            }
            public string Post(string uri, string contentType = "application/x-www-form-urlencoded")
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Method = "POST";
                    if (!string.IsNullOrEmpty(Cookie))
                    {
                        request.Headers["Cookie"] = Cookie;
                    }
                    if (!string.IsNullOrEmpty(UserAgent))
                    {
                        request.UserAgent = UserAgent;
                    }
                    foreach (var header in headers)
                    {
                        request.Headers[header.Name] = header.Value;
                    }
                    if (WebProxy != null)
                    {
                        request.Proxy = WebProxy;
                    }
                    request.ContentType = contentType;
                    request.ContentLength = 0;
                    var response = (HttpWebResponse)request.GetResponse();
                    return new StreamReader(response.GetResponseStream()).ReadToEnd();
                }
                catch (WebException ex)
                {
                    var response = (HttpWebResponse)ex.Response;
                    if (response != null)
                    {
                        return new StreamReader(response.GetResponseStream()).ReadToEnd();
                    }    
                }
                return "";
            }
        }
        public class Instagram
        {
            private string username { get; set; }
            public string Username { get { return username; } }
            private Request request = new Request();
            public Instagram(string cookie)
            {
                request.Cookie = cookie;
                request.AddHeader("x-ig-app-id", "936619743392459");
                try
                {
                    string username = Regex.Match(request.Get("https://i.instagram.com/api/v1/accounts/edit/web_form_data/"), "\"username\":\"(.*?)\"").Groups[1].Value;
                    if (string.IsNullOrEmpty(username)) throw new InstagramLoginException("cookie die");
                    string get = request.Get("https://www.instagram.com/data/shared_data/");
                    string csrf_token = Regex.Match(get, "\"csrf_token\":\"(.*?)\"").Groups[1].Value;
                    if (!cookie.EndsWith(";")) cookie += ";";
                    if (!cookie.Contains("csrftoken=")) cookie += "csrftoken=" + csrf_token + ";";
                    this.username = username;
                    request.Cookie = cookie;
                    request.AddHeader("x-csrftoken", csrf_token);
                    request.AddHeader("sec-fetch-site", "same-site");
                    request.AddHeader("x-instagram-ajax", "1006267176");
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";
                }
                catch
                {
                    throw new InstagramLoginException("cookie die");
                }
            }
            public bool Like(string id)
            {
                try
                {
                    return request.Post($"https://i.instagram.com/api/v1/web/likes/{id}/like/").Contains("\"status\":\"ok\"");
                }
                catch { }
                return false;
            }
            public bool Follow(string id)
            {
                try
                {
                    return request.Post($"https://i.instagram.com/api/v1/web/friendships/{id}/follow/").Contains("\"status\":\"ok\"");
                }
                catch { }
                return false;
            }
            public bool LikeCMT(string id)
            {
                try
                {
                    return request.Post($"https://i.instagram.com/api/v1/web/comments/like/{id}/", "status=ok").Contains("\"status\":\"ok\"");
                }
                catch { }
                return false;
            }
            public bool CMT(string id, string cmt)
            {
                try
                {
                    return request.Post($"https://i.instagram.com/api/v1/web/comments/{id}/add/", $"comment_text={HttpUtility.UrlEncode(cmt)}").Contains("\"status\":\"ok\"");
                }
                catch { }
                return false;
            }
        }
        public static void Wait(int s, string text)
        {
            for (var i = s; i > 0; i--)
            {
                Console.Write("\r");
                Console.Write(text.Replace("{seconds}", i.ToString()));
                Thread.Sleep(1000);
                if (i == 1) Console.Write("\r");
            }
        }
        public static void WaitDelay(int seconds)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Wait(seconds, "Đang delay job, còn {seconds} giây để chuyển job...");
        }
        static void Main(string[] args)
        {
            Console.Title = "Tool TDS Instagram";
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            if (!File.Exists("config.json"))
            {
                File.WriteAllText("config.json", "{\"token\":\"\",\"cookie\":\"\",\"job\":\"like,likecmt,cmt,follow\",\"like_delay\":60,\"follow_delay\":120,\"cmt_delay\":30,\"like_cmt_delay\":30}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Vui lòng setup file config.json!");
                goto DONE;
            }
            var cookie = Regex.Match(File.ReadAllText("config.json"), "\"cookie\":\"(.*?)\"").Groups[1].Value;
            var token = Regex.Match(File.ReadAllText("config.json"), "\"token\":\"(.*?)\"").Groups[1].Value;
            var job = Regex.Match(File.ReadAllText("config.json"), "\"job\":\"(.*?)\"").Groups[1].Value;
            int like_delay = int.Parse(Regex.Match(File.ReadAllText("config.json"), "\"like_delay\":(.*?),").Groups[1].Value);
            int follow_delay = int.Parse(Regex.Match(File.ReadAllText("config.json"), "\"follow_delay\":(.*?),").Groups[1].Value);
            int cmt_delay = int.Parse(Regex.Match(File.ReadAllText("config.json"), "\"cmt_delay\":(.*?),").Groups[1].Value);
            int like_cmt_delay = int.Parse(Regex.Match(File.ReadAllText("config.json"), "\"like_cmt_delay\":(.*?)}").Groups[1].Value);
            try
            {
                Instagram instagram = new Instagram(cookie);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Đăng nhập instagram thành công!");
                TDS tds = new TDS(token);
                Console.WriteLine("Đăng nhập tds thành công!");
                if (!tds.SetNick(instagram.Username))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Không thể set nick username!");
                    goto DONE;
                }
                while (true)
                {
                    if (!job.Contains(","))
                    {
                        if (job.ToLower() == "like")
                        {
                            var jobs = tds.GetJobs(TDSTypeJob.Like);
                            if (jobs.Count > 0)
                            {
                                var job_random = jobs[rand.Next(0, jobs.Count)];
                                if (instagram.Like(job_random.Id.Split('_')[0]))
                                {
                                    string result = tds.PostXu(job_random.Id, TDSTypeJobCache.Like);
                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                        Console.WriteLine($"[{job_random.Id}] Đã gửi duyệt nhiệm vụ like, số xu đợi duyệt: {result}");
                                        WaitDelay(like_delay);
                                    }
                                }
                            }
                        }
                        else if (job.ToLower() == "likecmt")
                        {
                            var jobs = tds.GetJobs(TDSTypeJob.LikeCMT);
                            if (jobs.Count > 0)
                            {
                                var job_random = jobs[rand.Next(0, jobs.Count)];
                                if (instagram.LikeCMT(job_random.Id.Split('_')[0]))
                                {
                                    string result = tds.PostXu(job_random.Id, TDSTypeJobCache.LikeCMT);
                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                        Console.WriteLine($"[{job_random.Id}] Đã gửi duyệt nhiệm vụ likecmt, số xu đợi duyệt: {result}");
                                        WaitDelay(like_cmt_delay);
                                    }
                                }
                            }
                        }
                        else if (job.ToLower() == "follow")
                        {
                            var jobs = tds.GetJobs(TDSTypeJob.Follow);
                            if (jobs.Count > 0)
                            {
                                var job_random = jobs[rand.Next(0, jobs.Count)];
                                if (instagram.Follow(job_random.Id.Split('_')[0]))
                                {
                                    string result = tds.PostXu(job_random.Id, TDSTypeJobCache.Follow);
                                    if (!string.IsNullOrEmpty(result))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                        Console.WriteLine($"[{job_random.Id}] Đã gửi duyệt nhiệm vụ follow, số xu đợi duyệt: {result}");
                                        WaitDelay(follow_delay);
                                    }
                                }
                            }
                        }
                    }   
                    else
                    {
                        var job_ = job.Split(',');
                        foreach (var item in job_)
                        {
                            if (item.ToLower() == "like")
                            {
                                var jobs = tds.GetJobs(TDSTypeJob.Like);
                                if (jobs.Count > 0)
                                {
                                    var job_random = jobs[rand.Next(0, jobs.Count)];
                                    if (instagram.Like(job_random.Id.Split('_')[0]))
                                    {
                                        string result = tds.PostXu(job_random.Id, TDSTypeJobCache.Like);
                                        if (!string.IsNullOrEmpty(result))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                            Console.WriteLine($"[{job_random.Id}] Đã gửi duyệt nhiệm vụ like, số xu đợi duyệt: {result}");
                                            WaitDelay(like_delay);
                                        }
                                    }
                                }
                            }
                            else if (item.ToLower() == "likecmt")
                            {
                                var jobs = tds.GetJobs(TDSTypeJob.LikeCMT);
                                if (jobs.Count > 0)
                                {
                                    var job_random = jobs[rand.Next(0, jobs.Count)];
                                    if (instagram.LikeCMT(job_random.Id.Split('_')[0]))
                                    {
                                        string result = tds.PostXu(job_random.Id, TDSTypeJobCache.LikeCMT);
                                        if (!string.IsNullOrEmpty(result))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                            Console.WriteLine($"[{job_random.Id}] Đã gửi duyệt nhiệm vụ likecmt, số xu đợi duyệt: {result}");
                                            WaitDelay(like_cmt_delay);
                                        }
                                    }
                                }
                            }
                            else if (item.ToLower() == "follow")
                            {
                                var jobs = tds.GetJobs(TDSTypeJob.Follow);
                                if (jobs.Count > 0)
                                {
                                    var job_random = jobs[rand.Next(0, jobs.Count)];
                                    if (instagram.Follow(job_random.Id.Split('_')[0]))
                                    {
                                        string result = tds.PostXu(job_random.Id, TDSTypeJobCache.Follow);
                                        if (!string.IsNullOrEmpty(result))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                            Console.WriteLine($"[{job_random.Id}] Đã gửi duyệt nhiệm vụ follow, số xu đợi duyệt: {result}");
                                            WaitDelay(follow_delay);
                                        }
                                    }
                                }
                            }
                        }    
                    }    
                }  
            }
            catch (TDSLoginException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Token tds không chính xác!");
            }
            catch (InstagramLoginException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Cookie instagram không chính xác!");
            }
        DONE:;
            Console.ReadLine();
        }
        public static Random rand = new Random();
    }
}
