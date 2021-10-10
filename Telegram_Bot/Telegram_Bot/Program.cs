using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.IO;
using System.IO.Compression;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System.Drawing;

namespace Art_Telegram_Bot
{
    class Program
    {
        private static string token { get; set; } = "2014760085:AAGa8r6SU3no6NlvJBDAqz-9nj7UNW5WBlU";
        private static TelegramBotClient client;
        private static IWebDriver browser;
        private static string directory = Directory.GetCurrentDirectory(); 

        [Obsolete]
        static void Main(string[] args)
        {
            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            client.StopReceiving();
        }

        [Obsolete]
        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var msg = e.Message;
            var file = await client.GetFileAsync(msg.Photo.LastOrDefault().FileId);
            var path = directory + "/" + file.FilePath;
            using (var saveImageStream = new FileStream(path, FileMode.Create))
            {
                await client.DownloadFileAsync(file.FilePath, saveImageStream);
            }

            browser = new ChromeDriver();
            browser.Manage().Window.Maximize();
            browser.Navigate().GoToUrl("https://yandex.com/images/?lr=87");
            browser.FindElement(
                By.CssSelector("body > header > div > div.serp-header__under > div.serp-header__search2 > form > div.search2__input > span > span > div.input__cbir-button.input__button > button")).Click();
            IWebElement select_file = browser.FindElement(
                By.XPath("/html/body/header/div/div[3]/div[1]/div[2]/div[2]/div[3]/button[1]"));
            select_file.SendKeys(path);
        }

        private static void SendRequest(Telegram.Bot.Types.File file)
        {
            var request = WebRequest.Create("https://yandex.com/images/?lr=87");
            request.Method = "POST";
            request.ContentType = "image/jpeg";
            var gzipArray = Compress(file);
            var byteArray = new byte[gzipArray.Length];
            request.ContentLength = byteArray.Length;

            using (gzipArray)
            {
                gzipArray.Read(byteArray, 0, byteArray.Length);
            }

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            var response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            using (dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                var reader = new StreamReader(dataStream);
                // Read the content.
                var responseFromServer = reader.ReadToEnd();
                // Display the content.
                Console.WriteLine(responseFromServer);
            }

            // Close the response.
            response.Close();
        }

        private static GZipStream Compress(Telegram.Bot.Types.File file)
        {
            using (var originalFileStream = System.IO.File.OpenRead("C:/Users/HP/semester_project/Art_Telegram_Bot/Art_Telegram_Bot/bin/Debug/" + file.FilePath))
            {
                using (var compressedFileStream = System.IO.File.Create("C:/Users/HP/semester_project/Art_Telegram_Bot/Art_Telegram_Bot/bin/Debug/" + file.FilePath + "/" + file + ".gz"))
                {
                    using (var compressionStream = new GZipStream(compressedFileStream,
                           CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                        return compressionStream;
                    }
                }
            }
        }
    }
}
