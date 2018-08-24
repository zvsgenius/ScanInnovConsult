using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;

namespace ScanInnov
{
    class Program
    {
        static string baseLink = "http://tl.innov.ru/consult/";
        static string protocol = "";
        static string domainName = "";
        static string folder = "";

        static void Main(string[] args)
        {            
            List<string> links = new List<string>();
            int index = 0;
            int temp = 1;

            if (!ProcessingBaseLink())
            {
                Console.WriteLine("Error id:1\nbase link is not correct");
                Console.ReadLine();
                return;
            }

            bool firstStart = VerifyFirstStart(ref links, ref index);

            if(links == null)
            {
                Console.WriteLine("Error id:2\nЗапуск программы вызвал ошибку. Программа завершает свою работу.");
                return;
            }

            if (firstStart)
            {
                links.Add(baseLink);

                #region Add Links

                links.Add(protocol + domainName + "/consult/consultants/");
                links.Add(protocol + domainName + "/consult/index.php?category=6");
                links.Add(protocol + domainName + "/consult/index.php?category=7");
                links.Add(protocol + domainName + "/consult/index.php?category=3");
                links.Add(protocol + domainName + "/consult/index.php?category=8");
                links.Add(protocol + domainName + "/consult/index.php?category=4");
                links.Add(protocol + domainName + "/consult/index.php?category=1");
                links.Add(protocol + domainName + "/consult/index.php?category=5");
                links.Add(protocol + domainName + "/consult/index.php?category=2");

                #endregion Add Link
            }


            for (int i = index; i < links.Count; i++)
            {
                temp++;

                if (temp == 17000)
                {
                    StopTheProgram(ref links, i);
                    Console.ReadLine();
                    return;
                }

                Console.Write(i + " ");
                Console.WriteLine(links[i]);

                string page = DonloadPage(links[i]);

                if (page == null)
                    continue;

                page = DeletingComments(ref page);

                if (page == null)
                    continue;

                SearchLinks(ref page, ref links, links[i]);
            }

            Console.WriteLine("end");
            Console.ReadLine();
        }

        private static void StopTheProgram(ref List<string> links, int i)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Logs";

            try
            {
                using (StreamWriter sw = new StreamWriter((path + "\\" +i)))
                {
                    foreach (string line in links)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error id:3\nThe file could not be write:");
                Console.WriteLine(e.Message);
                Console.WriteLine("Промежуточный результат не записан");
                return;
            }
            Console.WriteLine("Промежуточный результат записан");
        }

        private static bool VerifyFirstStart(ref List<string> links, ref int index)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Logs";
            bool flag = false;
            List<int> indexes = new List<int>();
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                    flag = true;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error id:4\nСоздание папки Logs вызвало ошибку:\n" + e.Message);
                links = null;
                return true;
            }

            if (!flag)
            {
                try
                {
                    string[] files = Directory.GetFiles(path);

                    foreach (string file in files)
                    {
                        indexes.Add(Int32.Parse(file.Substring(file.LastIndexOf('\\') + 1)));
                    }

                    foreach (int ind in indexes)
                    {
                        if (ind > index)
                            index = ind;
                    }

                    using (StreamReader sr = new StreamReader(path + "\\" + index))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            links.Add(line);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error id:5\nThe process opened files is failed: {0}", e.ToString());
                    links = null;
                    return true;
                }
            }



            return flag;
        }

        private static bool ProcessingBaseLink()
        {
            int beginDomainName = 0;

            if (baseLink.IndexOf("http://", StringComparison.Ordinal) == 0)
            {
                protocol = "http://";
                beginDomainName = 7;
            }
            else if(baseLink.IndexOf("https://", StringComparison.Ordinal) == 0)
            {
                protocol = "https://";
                beginDomainName = 8;
            }
            else
                return false;

            int endDomainName = baseLink.IndexOf("/", beginDomainName, StringComparison.Ordinal);

            if (endDomainName == -1)
                return false;

            domainName = baseLink.Substring(beginDomainName, endDomainName - beginDomainName);

            if (domainName == "")
                return false;

            int beginFolderName = endDomainName + 1;

            folder = baseLink.Substring(beginFolderName - 1);

            if (folder == "")
                return false;

            return true;
        }

        private static string DeletingComments(ref string page)
        {
            int startComment = 0;
            int endComment = 0;
            LinkedList<Comment> comments = new LinkedList<Comment>();

            while (true)
            {
                startComment = page.IndexOf("<!--", startComment, StringComparison.Ordinal);

                if (startComment == -1)
                    break;

                endComment = page.IndexOf("-->", startComment + 4, StringComparison.Ordinal) + 3;

                if (endComment == -1)
                    break;

                comments.AddFirst(new Comment(startComment, endComment - startComment));

                startComment++;
            }

            if (comments.Count == 0)
                return null;

            StringBuilder textChange = new StringBuilder(page);

            foreach(Comment comment in comments)
            {
                textChange.Remove(comment.begin, comment.length);
            }

            return textChange.ToString();
        }

        private static string DonloadPage(string link)
        {          
            string text = null;

                string path = CreateFolders(link);
                if (path == null)
                    return null;

                string newPath = CreateFile(link, path);
                if (newPath == null)
                    return null;

            try
            {
                using (StreamReader sr = new StreamReader(newPath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        text += line;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error id:6\nОшибка чтения файла: " + newPath +"\n" + e.Message);
                return null;
            }

            return text;
        }

        private static string CreateFolders(string link)
        {
            int indexDomainName = link.IndexOf(domainName, StringComparison.Ordinal);

            if (indexDomainName == -1)
                return null;

            string path = AppDomain.CurrentDomain.BaseDirectory + domainName;

            int amountFolders = 0;
            int indexFolder = indexDomainName + 1;
            int lastIndexFolder = 0;

            if (link.IndexOf("?", StringComparison.Ordinal) == -1)
            {
                while (true)
                {
                    lastIndexFolder = indexFolder;
                    indexFolder = link.IndexOf("/", indexFolder, StringComparison.Ordinal);

                    if (indexFolder == -1)
                        break;

                    if (amountFolders != 0)
                        path += "\\" + link.Substring(lastIndexFolder, indexFolder - lastIndexFolder);

                    indexFolder++;

                    try
                    {
                        if (!Directory.Exists(path))
                        {
                            DirectoryInfo di = Directory.CreateDirectory(path);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error id:7\nСоздание папки вызвало ошибку:\n" + e.Message);
                        return null;
                    }

                    amountFolders++;
                }
            }
            else
            {
                path += "\\consult";
                try
                {
                    if (!Directory.Exists(path))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error id:7.0\nСоздание папки вызвало ошибку:\n" + e.Message);
                    return null;
                }

                if (link.IndexOf("index.php?curr_page=", StringComparison.Ordinal) != -1)
                {
                    path += "\\curr_page";
                }
                else if(link.IndexOf("index.php?consultant=", StringComparison.Ordinal) != -1)
                {
                    path += "\\consultant";
                }
                else if (link.IndexOf("index.php?category=", StringComparison.Ordinal) != -1)
                {
                    path += "\\category";
                }
                else
                {
                    return null;
                }

                try
                {
                    if (!Directory.Exists(path))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(path);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error id:7.1\nСоздание папки вызвало ошибку:\n" + e.Message);
                    return null;
                }
            }

            return path;
        }

        private static string CreateFile(string link, string path)
        {
            WebRequest req = WebRequest.Create(link);

            HttpWebRequest request = (HttpWebRequest)req;
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.117 Safari/537.36";

            WebResponse resp;

            try
            {
                resp = request.GetResponse();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error id:8\nerror downloading the page\n" + link + "\n" + e.Message);
                return null;
            }

            Stream istrm = resp.GetResponseStream();

            string newPath = path;

            if (link.IndexOf("?", StringComparison.Ordinal) == -1)
            {
                if (link[link.Length - 1] == '/')
                    newPath += "\\index.html";
                else
                    newPath += "\\" + link.Substring(link.LastIndexOf('/') + 1);
            }
            else
            {
                int indexAND = link.IndexOf("&", StringComparison.Ordinal);
                if (indexAND == -1)
                {
                    newPath += "\\" + link.Substring(link.LastIndexOf('=') + 1);
                }
                else
                {
                    int lastIndex1 = link.LastIndexOf('=');
                    int lastIndex2 = link.LastIndexOf('=', indexAND);
                    newPath += "\\" + link.Substring(lastIndex2 + 1, indexAND - lastIndex2 -1) + "-" + link.Substring(lastIndex1 + 1);
                }
            }

            try
            {
                using (FileStream fileStream = File.Create(newPath))
                {
                    istrm.CopyTo(fileStream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error id:9\nСтраница:" + link + "Не загружена. Ошибка:\n" + e.Message);
                return null;
            }

            resp.Close();
            return newPath;
        }

        private static void SearchLinks(ref string page, ref List<string> links, string parentLink)
        {
            int startTeg = 0;
            int startHref = 0;
            int startLink = 0;
            int endLink = 0;
            int lenghtLink = 0;
            string link = null;

            while (true)
            {
                startTeg = page.IndexOf("<a", startTeg, StringComparison.Ordinal);

                if (startTeg == -1)
                    break;

                startTeg++;

                startHref = page.IndexOf("href=\"", startTeg, StringComparison.Ordinal);

                if (startHref == -1)
                    break;

                startLink = startHref + 6;

                endLink = page.IndexOf("\"", startLink, StringComparison.Ordinal);

                if (endLink == -1)
                    break;

                lenghtLink = endLink - startLink;

                link = page.Substring(startLink, lenghtLink);

                if (link == null)
                    continue;

                link = link.ToLower();

                link = LinkAnalysis(ref link, parentLink);

                if (link == null)
                    continue;

                if(FindDuplicates(link, ref links))
                    continue;

                if (link != null)
                    links.Add(link);

            }

        }

        private static bool FindDuplicates(string link, ref List<string> links)
        {
            foreach(string tempLink in links)
            {
                if (string.CompareOrdinal(link, tempLink) == 0)
                    return true;                
            }

            return false;
        }

        private static string LinkAnalysis(ref string link, string parentLink)
        {
            char firstCharacter = link[0];

            string tempLink = null;

            switch (firstCharacter)
            {
                case 'h':
                    {
                        if (link.IndexOf(protocol + domainName + folder, StringComparison.Ordinal) != 0)
                            return null;
                        tempLink = link;
                    }
                    break;
                case '/':
                    {
                        if (link.IndexOf(folder, StringComparison.Ordinal) != 0)
                            return null;
                        tempLink = protocol + domainName + link;
                    }
                    break;
                case '"': return null;
                case '.':
                    {
                        if (link.IndexOf("../", StringComparison.Ordinal) == 0)
                        {
                            int lastIndexSlash = parentLink.LastIndexOf("/", StringComparison.Ordinal);
                            int tempLinkEnd = parentLink.LastIndexOf("/", lastIndexSlash - 1, StringComparison.Ordinal);
                            tempLink = parentLink.Substring(0, tempLinkEnd + 1) + link.Substring(3);
                        }
                        else if (link.IndexOf("./", StringComparison.Ordinal) == 0)
                        {
                            string part1Link = parentLink.Substring(0, parentLink.LastIndexOf("/", StringComparison.Ordinal) + 1);
                            tempLink = part1Link + link.Substring(2);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
            }
            if(tempLink == null)
            {

                string part1Link = parentLink.Substring(0, parentLink.LastIndexOf("/", StringComparison.Ordinal) + 1);
                tempLink = part1Link + link;
            }


            string checkLink = tempLink.Replace(protocol + domainName + folder, "");

            string[] forbiddenSymbols = { "\\", ":", "*", "<", ">", "\"", "//", "|" };

            foreach(string forbiddenSymbol in forbiddenSymbols)
            {
                if (checkLink.IndexOf(forbiddenSymbol, StringComparison.Ordinal) != -1)
                    return null;
            }

            return tempLink;
        }
    }

}
