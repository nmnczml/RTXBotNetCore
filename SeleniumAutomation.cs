using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using System.Threading;

namespace AmazonBuyerCore
{
    public class SeleniumAutomation
    {
        private readonly IWebDriver webDriver;

        public SeleniumAutomation(IWebDriver webDriver)
        {
            this.webDriver = webDriver;
        }

        public bool addItemToChart(IWebDriver siteDriver)
        {


            //    //span[contains(@class,'a-size-medium a-color-price')] ---Currently unavailable. or price in here

            ReadOnlyCollection<IWebElement> currentlyUnavailable = siteDriver.FindElements(By.XPath("//span[contains(@class,'a-size-medium a-color-price')]"));

            if (currentlyUnavailable.Count > 0) // there may be price info also
            {
                IWebElement cUDetails = currentlyUnavailable[0];
                if (cUDetails.Text.ToLower().Equals("currently unavailable."))
                {
                    MainClass.Logger.Info("Currentyly unavailable.");
                    return false;
                }

            }


            //    //input[@id='add-to-cart-button']  -- add to chart directly
            ReadOnlyCollection<IWebElement> isThereDirectAddToChartButtons = siteDriver.FindElements(By.XPath("//input[@id='add-to-cart-button']"));

            if (isThereDirectAddToChartButtons.Count > 0) // there may be price info also
            {
                IWebElement addToChartButton = isThereDirectAddToChartButtons[0];
                addToChartButton.Click();
                MainClass.Logger.Info("Clicked add to chart");
                return true;
            }


            //    (//span[@data-action='show-all-offers-display']//a)[2] -- available from these sellers link
            ReadOnlyCollection<IWebElement> availableFromTheseSellersLinks = siteDriver.FindElements(By.XPath("(//span[@data-action='show-all-offers-display']//a)[2]"));


            if (availableFromTheseSellersLinks.Count > 0)
            {
                IWebElement availableFromTheseSellersLink = availableFromTheseSellersLinks[0];
                availableFromTheseSellersLink.Click();
                MainClass.Logger.Info("Clicked available sellers");

            }


            //check for amazon brand

            // //div[@id='aod-offer-soldBy']/div[@class='a-fixed-left-grid']/div[@class='a-fixed-left-grid-inner']/div[@class='a-fixed-left-grid-col a-col-right']  -- sold by array
            ReadOnlyCollection<IWebElement> availableSellerArray = siteDriver.FindElements(By.XPath("//div[@id='aod-offer']"));

            //div[@id='aod-offer'][1]

            //div[@id='aod-offer'][1]//*[contains(@class,'a-button-input')] --BUTTON
            for(int i = 0;i<availableFromTheseSellersLinks.Count;i++)
            {

                string cnt = Convert.ToString(i + 1);

                string soldByHtml = siteDriver.FindElement(By.XPath("//div[@id='aod-offer']["+cnt+"]/*[contains(@id,'aod-offer-soldBy')]")).Text;


                if(soldByHtml.ToLower().Contains("amazon"))
                {
                    MainClass.Logger.Info("found amazon.com seller");
                    siteDriver.FindElement(By.XPath("//div[@id='aod-offer'][" + cnt + "]//*[contains(@class,'a-button-input')]")).Click();
                    return true;
                }

            }


            ////input[@name='submit.addToCart'] --buy buttons



            MainClass.Logger.Info("Nothing found");

            return false;        
        }

        public void Start(string username, string password, string storeName)
        {
           
            webDriver.Url = storeName;

            Thread.Sleep(3000);


            MainClass.Logger.Info("logging in");

            try
            {
                //cookie accept
                ClickAndWaitForPageToLoad(webDriver, By.Id("sp-cc-accept"));
                Thread.Sleep(2000);
            }
            catch 
            {
                //there may not be cookie query;
            }
           

            ClickAndWaitForPageToLoad(webDriver, By.Id("nav-link-accountList"));

            // Kullanıcı adı
            var input = webDriver.FindElement(By.Id("ap_email"));
            input.SendKeys(username);
            ClickAndWaitForPageToLoad(webDriver, By.Id("continue"));

            // Parola
            input = webDriver.FindElement(By.Id("ap_password"));
            input.SendKeys(password);
            webDriver.FindElement(By.Name("rememberMe")).Click();
            ClickAndWaitForPageToLoad(webDriver, By.Id("signInSubmit"));

            Thread.Sleep(15000);

            MainClass.Logger.Info("logged in");


            string[] itemURIs = File.ReadAllLines("./ProductList.txt");


            while (true)
            {

                foreach (string uri in itemURIs)
                {


                    if (uri.StartsWith("--"))
                    {
                        MainClass.Logger.Info(uri);
                        continue;
                    }

                    Random rnd = new Random();
                    int dice = rnd.Next(0, 3);
                    Thread.Sleep(dice * 500);
                    webDriver.Url = uri;

                    ReadOnlyCollection<IWebElement> areThereAnyDogs = webDriver.FindElements(By.XPath("//div[@class='nav-footer-line'] | //img[@alt='Dogs of Amazon']"));

                    if (areThereAnyDogs.Count > 0) // there may be price info also
                    {
                        if (areThereAnyDogs[0].TagName.Equals("img"))
                        {
                            MainClass.Logger.Info("Saw dogs, continue...");

                            rnd = new Random();
                            dice = rnd.Next(0, 10);
                            Thread.Sleep(dice * 500);
                            continue;
                        }
                    }



                    bool addedToChart = addItemToChart(webDriver);

                    if (addedToChart)
                        buyItem(webDriver);
                 }

            }
            
          
 
        }

        private void buyItem(IWebDriver driver)
        {
            //proceed check out
            ClickAndWaitForPageToLoad(driver, By.XPath("//a[@id='hlb-ptc-btn-native']"));

            //deliver to this address.
            ClickAndWaitForPageToLoad(driver, By.XPath("//form[@class='a-nostyle'][1]/div[@class='a-spacing-base address-book']//*[normalize-space(text()) = 'Deliver to this address']"));


            //click continue button shipping options
            ReadOnlyCollection<IWebElement> continueButtons = driver.FindElements(By.XPath("//input[@class='a-button-text']"));

            foreach (IWebElement element in continueButtons)
            {
                element.Click();
                break;
            }


            //click continue button payment options
            ReadOnlyCollection<IWebElement> continueButtonsPayments = driver.FindElements(By.XPath("//input[@name='ppw-widgetEvent:SetPaymentPlanSelectContinueEvent']"));

            foreach (IWebElement element in continueButtonsPayments)
            {
                element.Click();
                break;
            }


            //place your order button

            ReadOnlyCollection<IWebElement> placeYourOrder = driver.FindElements(By.XPath("//input[@class='a-button-text place-your-order-button']"));

            foreach (IWebElement element in continueButtonsPayments)
            {
                element.Click();
                break;
            }





        }

        private void ClickAndWaitForPageToLoad(IWebDriver driver,  By elementLocator, int timeout = 10)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                var elements = driver.FindElements(elementLocator);
                if (elements.Count == 0)
                {
                    MainClass.Logger.Error("No elements " + elementLocator + " ClickAndWaitForPageToLoad");
                    throw new NoSuchElementException(
                        "No elements " + elementLocator + " ClickAndWaitForPageToLoad");
                }
                var element = elements.FirstOrDefault(e => e.Displayed);
                element.Click();
               // wait.Until(ExpectedConditions.StalenessOf(element));
            }
            catch (NoSuchElementException)
            {
                MainClass.Logger.Error("Element with locator: '" + elementLocator + "' was not found.");
                
                throw;
            }
        }
    }
}