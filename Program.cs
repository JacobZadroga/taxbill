using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace taxbill
{
    class Program
    {
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--log-level=3");
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            IWebDriver driver = new ChromeDriver(service, options);
            driver.Manage().Window.Minimize();
            string website = "https://www.mytaxbill.org/inet/bill/home.do?town=";

            string town = "", propnum = "", propname = "";
            bool success = false;
            int fails = 0;
            do
            {
                if(fails > 0)
                {
                    Console.WriteLine("Not Found. Try Again.");
                }
                Console.Write("Town: ");
                town = Console.ReadLine().ToLower();

                Console.Write("Street Number: ");
                propnum = Console.ReadLine().ToLower();

                Console.Write("Street Name: ");
                propname = Console.ReadLine().ToLower();


                driver.Navigate().GoToUrl(website + town);

                driver.FindElement(By.XPath("//select[@name='actionType']//option[@value='Property Location']")).Click();
                driver.FindElement(By.XPath("//input[@type='radio' and @id='recordType3']")).Click();
                driver.FindElement(By.XPath("//div[@id='location']//input[@name='propertyNumber']")).SendKeys(propnum);
                driver.FindElement(By.XPath("//div[@id='location']//input[@name='propertyName']")).SendKeys(propname);
                driver.FindElement(By.XPath("//div[@id='location']//input[@class='button']")).Click();

                success = (driver.FindElements(By.XPath("//div[@class='success']")).Count == 0);
                if(fails >= 3)
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

            Console.ReadLine();
            driver.Quit();
        }
    }
}
