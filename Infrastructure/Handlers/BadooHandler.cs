﻿using Infrastructure.Selectors.Navigation;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Infrastructure.Handlers
{
    public class BadooHandler : HandlerBase
    {
        private readonly IWebDriver _webDriver;
        private readonly BadooNavigationSelectors _selectors;

        public BadooHandler(
                   IWebDriver webDriver,
                   BadooNavigationSelectors selectors) : base(webDriver, selectors)
        {
            _webDriver = webDriver ??
                throw new ArgumentNullException(nameof(webDriver));

            _selectors = selectors ??
                throw new ArgumentNullException(nameof(selectors));
        }

        #region Public Methods

        public override void SearchFromEncounters()
        {
            WaitSeconds(7);

            try
            {
                while (true)
                {
                    OpenProfile();

                    try
                    {
                        WaitSeconds(1);

                        CheckProfile(ProfileConditions);

                        ClickLikeBtn();
                    }
                    catch (NoSuchElementException exc)
                    {
                        ClickDislikeBtn();
                    }

                    WaitSeconds(1);
                }
            }
            catch (ElementClickInterceptedException)
            {
                try
                {
                    WaitSeconds(1);

                    ClosePopupWindow();
                }
                catch (NoSuchElementException)
                {

                }
            }
        }

        public override void SearchFromPeopleNearby()
        {
            WaitSeconds(5);

            var urls = GetUrlsFromPages(2);

            foreach (var url in urls)
            {
                GoToUrl(url);

                try
                {
                    WaitSeconds(2);

                    CheckProfile(ProfileConditions);

                    WaitSeconds(2);

                    ClickLikeBtn();

                    WaitSeconds(2);

                    //ClosePopupWindow();
                    //WaitSeconds(2);
                    //AddToFavourites();
                    //WaitSeconds(3);
                }
                catch (NoSuchElementException)
                {
                    ClickDislikeBtn();

                    WaitSeconds(3);
                }
            }
        }

        public override void SetLocalization()
        {
            WaitSeconds(7);

            var text = _webDriver.FindElement(_selectors.FeaturedPeople)
                                 .Text;
            switch (text)
            {
                case "Самые известные":
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
                    break;

                case "Популярні":
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("uk-UA");
                    break;
            }

            WaitSeconds(3);
        }

        #endregion

        #region Protected Methods

        protected override void ClickLikeBtn()
        {
            WaitSeconds(1);

            Click(_selectors.LikeBtn);

            Click(_selectors.CloseNotificationsLink);
        }

        protected override void ClickDislikeBtn()
        {
            Click(_selectors.DislikeBtn);
        }

        protected override void OpenProfile()
        {
            try
            {
                WaitSeconds(1);

                Click(_selectors.UserNameLink, 10);
            }
            catch (ElementNotInteractableException)
            {

            }
        }

        #endregion 

        #region Private Methods

        private void ClosePopupWindow()
        {
            Click(_selectors.ClosePopupLink);
        }

        private void AddToFavourites()
        {
            try
            {
                Click(_selectors.AddToFavouritesBtn);
            }
            catch (ElementNotInteractableException)
            {
                Click(_selectors.ProfileHeaderDropDownList);

                WaitSeconds(2);

                try
                {
                    JSClick(_selectors.AddToFavouritesClassName);
                }
                catch
                {
                    ClickDislikeBtn();

                    WaitSeconds(3);
                }
            }
        }

        private IEnumerable<string> GetUrlsFromPages(int pagesCount)
        {
            var hrefsFromAllRequestedPages = new List<string>();

            for (int i = 1; i <= pagesCount; i++)
            {
                GoToUrl($"https://badoo.com/search?filter=online&page={i}");

                WaitSeconds(6);

                var hrefsFromSinglePage = ExtractHrefsFromAnchors(_selectors.UserCardLink);

                hrefsFromAllRequestedPages.AddRange(hrefsFromSinglePage);
            }

            return hrefsFromAllRequestedPages.Distinct();
        }

        #endregion
    }
}
