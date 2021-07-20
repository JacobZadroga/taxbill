using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace taxbill
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--log-level=3");
            Console.WriteLine("v1.1");
            //options.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            IWebDriver driver = new ChromeDriver(service, options);
            driver.Manage().Window.Minimize();
            int countofgemsnt = File.ReadLines(Directory.GetCurrentDirectory() + "\\gemsnt.txt").Count();
            string[] gemsnt = new string[countofgemsnt];
            try
            {
                StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "\\gemsnt.txt");
                string line = sr.ReadLine();
                while(line != null)
                {
                    gemsnt[countofgemsnt - 1] = line;
                    countofgemsnt--;
                    line = sr.ReadLine();
                }
                sr.Close();

            } catch(Exception e)
            {
                Console.WriteLine("Exception Error : " + e.Message);
                driver.Quit();
                Console.ReadLine();
                return;
            }

            

            string town = ""; 
            Console.Write("Town: ");
            town = Console.ReadLine().ToLower();
            if(town.Equals("jz_listgemsnt"))
            {
                foreach(string s in gemsnt)
                {
                    Console.WriteLine(s);
                }
                driver.Quit();
                Console.ReadLine();
                return;
            } else if(town.Equals("jz_addgemsnt"))
            {
                StreamWriter sw = File.AppendText(Directory.GetCurrentDirectory() + "\\gemsnt.txt");
                Console.Write("Town to Add: ");
                string tta = Console.ReadLine();
                
                sw.WriteLine(tta);
                sw.Close();
                driver.Quit();
                Console.ReadLine();
                return;
            }
            while(true)
            {
                string bg = townGuess(town);
                if (!bg.ToLower().Equals(town))
                {
                    Console.Write("Did you mean " + bg + "?: ");
                    string ans = Console.ReadLine();
                    if (ans.ToLower().Equals("y"))
                    {
                        town = bg.ToLower();
                        break;
                    }
                    else
                    {
                        Console.Write("Town: ");
                        town = Console.ReadLine().ToLower();
                    }
                } else
                {
                    break;
                }
            }
            

            foreach(string twn in gemsnt)
            {
                if(twn.Equals(town))
                {
                    gemsnttaxbill(driver, town);
                    Console.ReadLine();
                    driver.Quit();
                    return;
                }
            }
            mytaxbill(driver, town);

            Console.ReadLine();
            driver.Quit();
        }

        public static void mytaxbill(IWebDriver driver, string town)
        {
            string website = "https://www.mytaxbill.org/inet/bill/home.do?town=";
            string propnum = "", propname = "";

            
            bool success = false;
            int fails = 0;
            do
            {
                if (fails > 0)
                {
                    Console.WriteLine("Not Found. Try Again.");
                }


                Console.Write("Street Number: ");
                propnum = Console.ReadLine().ToLower();

                Console.Write("Street Name (No Type): ");
                propname = Console.ReadLine().ToLower();

                Console.WriteLine("Waiting for page to load.");
                driver.Navigate().GoToUrl(website + town);
                WaitForPageLoad(By.XPath("//select[@name='actionType']//option[@value='Property Location']"), 20, driver);
                

                bool popup = false;
                Thread.Sleep(2000);
                //Console.Write("test");
                popup = !(driver.FindElements(By.XPath("//button[@id='btnclose' and @data-dismiss='modal']")).Count == 0);
                if (popup)
                {
                    driver.FindElement(By.XPath("//button[@id='btnclose' and @data-dismiss='modal']")).Click();
                }


                driver.FindElement(By.XPath("//select[@name='actionType']//option[@value='Property Location']")).Click();
                driver.FindElement(By.XPath("//input[@type='radio' and @id='recordType3']")).Click();
                driver.FindElement(By.XPath("//div[@id='location']//input[@name='propertyNumber']")).SendKeys(propnum);
                driver.FindElement(By.XPath("//div[@id='location']//input[@name='propertyName']")).SendKeys(propname);
                driver.FindElement(By.XPath("//div[@id='location']//input[@class='button']")).Click();

                success = (driver.FindElements(By.XPath("//div[@class='success']")).Count == 0);
                if (fails >= 3)
                {
                    Console.WriteLine("Too Many Failed Attmempts. Try Manually...");
                    driver.Quit();
                    Console.ReadLine();
                    return;
                }
                fails++;
            } while (!success);

            driver.FindElement(By.XPath("//b[contains(text(), '2019-')]/../../../..//a[@title='Download PDF']")).Click();
            Console.WriteLine(driver.FindElement(By.XPath("//b[contains(text(), '2019-')]/../../../..")).Text);

            driver.Manage().Window.Maximize();
        }

        public static void gemsnttaxbill(IWebDriver driver, string town)
        {
            string website = "https://gemsnt.com/";
            driver.Navigate().GoToUrl(website + town + "-webtax/");
            driver.FindElement(By.XPath("//input[@name='agree']")).Click();

            Console.Write("Last Name: ");
            string lastname = Console.ReadLine();

            Console.Write("Address (Do not include street type): ");
            string addy = Console.ReadLine();


            driver.FindElement(By.XPath("//input[@name='search']")).SendKeys(lastname);
            driver.FindElement(By.XPath("//input[@name='Search']")).Click();

            var taxbills = driver.FindElements(By.XPath("//tr[@class='d1' or @class='d0']"));

            foreach(IWebElement wb in taxbills)
            {
                string stuff = wb.Text;
                
                if (stuff.IndexOf("2019") != -1 && stuff.ToLower().IndexOf(addy.ToLower()) != -1)
                {
                    
                    wb.FindElement(By.XPath(".//form[@title='View']")).Click();
                    break;
                }
            }
            Console.ReadLine();
            driver.Quit();
            return;
        }

        public static double stringSimilarity(string act, string att)
        {
            double percentage = 0;
            int minimum = min(act.Length, att.Length);
            double correctadd = 100.0 / act.Length;
            for (int i = 0; i < minimum; i++)
            {
                string around;
                //Console.WriteLine(act.Substring(i, 1) + " | " + att.Substring(i, 1));
                if (i == 0)
                {
                    around = act.Substring(i, 2);
                }
                else if (i == act.Length - 1)
                {
                    around = act.Substring(i - 1, 2);
                }
                else
                {
                    around = act.Substring(i - 1, 3);
                }
                if (act.Substring(i, 1).ToLower().Equals(att.Substring(i, 1).ToLower()))
                {
                    percentage += correctadd;
                }
                else if (around.IndexOf(att.Substring(i, 1)) != -1)
                {
                    percentage += (correctadd / 2);
                }
            }
            return System.Math.Round(percentage, 2);
        }

        public static int min(int x, int y)
        {
            if (x < y) return x;
            return y;
        }

        public static string townGuess(string att)
        {
            double bestpercentage = -1;
            string besttown = "";
            StreamReader sr = new StreamReader(Directory.GetCurrentDirectory() + "\\towns.txt");
            string line = sr.ReadLine();
            while (line != null)
            {
                line.Replace("\n", "").Replace("\r", "");
                double curPer = stringSimilarity(line, att);
                if (curPer > bestpercentage)
                {
                    bestpercentage = curPer;
                    besttown = line;
                }
                line = sr.ReadLine();
            }
            sr.Close();
            return besttown;
        }

        public static bool WaitForPageLoad(By element, int timeout, IWebDriver driver)
        {
            bool didLoad = false;
            try
            {
                var w = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                w.Until(ExpectedConditions.ElementExists(element));
                didLoad = true;
                Console.WriteLine("Page Loaded.");
            }
            catch(NoSuchElementException)
            {
                Console.WriteLine("Timed Out!");
            }
            return didLoad;
        }
    }
}
