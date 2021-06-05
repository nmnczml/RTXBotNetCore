using System;
using System.Configuration;
using HtmlAgilityPack;
using log4net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AmazonBuyerCore
{
    class MainClass
    {
        public static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {


            Logger.Info("Crawler started");
 
            string storeURI = "https://www.amazon"+ConfigurationManager.AppSettings["store"];

            
            string userName = ConfigurationManager.AppSettings["userName"];
            string password = ConfigurationManager.AppSettings["password"];

            var webDriver = LaunchBrowser();
            try
            {
                var seleniumAutomation = new SeleniumAutomation(webDriver);

                seleniumAutomation.Start(userName, password, storeURI);
                
            }
            catch (Exception ex)
            {
                Logger.Error("Error while logging in",ex);
                 
            }
            finally
            {

            }
 
        }

        static IWebDriver LaunchBrowser()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36");


            var driver = new ChromeDriver(Environment.CurrentDirectory, options);
            return driver;
        }

    }
}
