using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using System.Timers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ImageCollector
{
    [TestClass]
    public class Program
    {
        public static void Main()
        {

            string userName = Environment.UserName;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Hi {userName}, Let's start collecting images :)");
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write("What to look for? (Example: car):");
            var searchText = Console.ReadLine();
            while (String.IsNullOrEmpty(searchText))
            {
                Console.Write("What to look for? (Example: car):");
                searchText = Console.ReadLine();
            }

            Console.Write("How many photos to save? (Example: 100):");
            int imagesToSave = Int32.Parse(Console.ReadLine().ToString());
            while (String.IsNullOrEmpty(searchText))
            {
                Console.Write("What to look for? (Example: car):");
                searchText = Console.ReadLine();
            }

            WebResults(searchText, imagesToSave);



        }

        public static void WebResults(string searchText, int imageToSave)
        {
            string currentPath = Directory.GetCurrentDirectory();
            var pathToSave = Path.Combine(currentPath, searchText);
            if (!Directory.Exists(Path.Combine(currentPath, searchText)))
            {
                Directory.CreateDirectory(Path.Combine(currentPath, searchText));

            }

            IWebDriver driver = new ChromeDriver(".\\");
            driver.Manage().Window.Maximize();
            var options = new ChromeOptions();
            options.AddArgument("no-sandbox");
            Console.Clear();
            driver.Navigate().GoToUrl($"https://www.google.com/search?q={searchText}&source=lnms&tbm=isch");
            Console.Clear();

            Console.WriteLine("Open Browser...");

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (timer.Elapsed.TotalSeconds < 5)
            {
                Console.WriteLine("Loading images....");
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollTo(0,document.body.scrollHeight);", "");
                Console.Clear();

            }

            timer.Stop();
            var masterDivs = driver.FindElement(By.Id("islrg"));

            if (masterDivs != null)
            {
                var childDivs = masterDivs.FindElement(By.CssSelector("div[class = 'islrc']")).FindElements(By.TagName("div"));

                foreach (var div in childDivs)
                {
                    string dataId;
                    try
                    {
                        dataId = div.GetAttribute("data-id");
                    }
                    catch(Exception ex)
                    {
                        continue;
                    }
                    
                    if (dataId != null)
                    {
                        driver.Navigate().GoToUrl($"{driver.Url}#imgrc={dataId}");

                        try
                        {


                            var imgsExtand = driver.FindElements(By.CssSelector("img[class = 'n3VNCb']"));

                            var img = imgsExtand[1];

                                if (img.GetAttribute("class") == "n3VNCb" && img.GetAttribute("src").Contains("http"))
                                {
                                    string src = img.GetAttribute("src");

                                    Console.WriteLine(img.GetAttribute("src"));
                                    using (WebClient client = new WebClient())
                                    {

                                        Random rnd = new Random();

                                        client.DownloadFile(new Uri(src), $"{pathToSave}\\{(rnd.Next(1, 50000)) + ".jpg"}");

                                        var closeBtn = driver.FindElement(By.CssSelector("a[aria-label = 'Close']"));
                                        closeBtn.Click();

                                        int fCount = Directory.GetFiles(pathToSave, "*", SearchOption.AllDirectories).Length;

                                        if (fCount >= imageToSave)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;

                                            Console.WriteLine($"Collecting {fCount} images successfully completed, Press Enter key to exit");

                                            var read = Console.ReadLine();

                                            Process.Start($"{pathToSave}");

                                            Environment.Exit(0);
                                        }
                                        continue;


                                    }
                                }
                                else if (img.GetAttribute("class") == "n3VNCb" && !img.GetAttribute("src").Contains("http"))
                                {

                                    pathToSave = Path.Combine(currentPath, searchText);
                                    string base64 = img.GetAttribute("src").Split(',')[1];
                                    byte[] bytes = Convert.FromBase64String(base64);
                                    using (Image imageStream = Image.FromStream(new MemoryStream(bytes)))
                                    {
                                        Random rnd = new Random();
                                        imageStream.Save($"{pathToSave}\\{(rnd.Next(1, 50000)) + ".jpg"}", ImageFormat.Jpeg);

                                        int fCount = Directory.GetFiles(pathToSave, "*", SearchOption.AllDirectories).Length;

                                        if (fCount >= imageToSave)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Green;

                                            Console.WriteLine($"Collecting {fCount} images successfully completed, Press Enter key to exit");

                                            var read = Console.ReadLine();

                                            Process.Start($"{pathToSave}");

                                            Environment.Exit(0);
                                        }
                                    }
                                }
                                else
                                {
                                    var closeBtn = driver.FindElement(By.CssSelector("a[aria-label = 'Close']"));
                                    closeBtn.Click();
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (driver.Url.Contains("imgrc"))
                                {
                                    driver.Navigate().Back();
                                    continue;
                                }
                            }
                       

                    }
                    else
                    {
                        if (driver.Url.Contains("imgrc"))
                        {
                            driver.Navigate().Back();
                        }

                    }

                }
            }

            Process.Start($"{pathToSave}");

        }
    }
}
