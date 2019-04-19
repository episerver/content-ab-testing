﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Marketing.KPI.Common.Attributes;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(ITestDataCookieHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private IMarketingTestingWebRepository _testRepo;
        private IHttpContextHelper _httpContextHelper;
        private IEpiserverHelper _episerverHelper;
        private IAdminConfigTestSettingsHelper _adminConfigTestSettingsHelper;
        private ITestDataCookieMigrator _testDataCookieMigrator;

        internal readonly string COOKIE_PREFIX = "EPI-MAR-";

        [ExcludeFromCodeCoverage]
        public TestDataCookieHelper()
        {
            _adminConfigTestSettingsHelper = new AdminConfigTestSettingsHelper();
            _testRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _httpContextHelper = new HttpContextHelper();
            _testDataCookieMigrator = new TestDataCookieMigrator();
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        internal TestDataCookieHelper(IAdminConfigTestSettingsHelper adminConfigTestSettingsHelper, IMarketingTestingWebRepository testRepo, IHttpContextHelper contextHelper, IEpiserverHelper epiHelper, ITestDataCookieMigrator testDataCookieMigrator)
        {
            _adminConfigTestSettingsHelper = adminConfigTestSettingsHelper;
            _testRepo = testRepo;
            _httpContextHelper = contextHelper;
            _episerverHelper = epiHelper;
            _testDataCookieMigrator = testDataCookieMigrator;
        }

        /// <summary>
        /// Evaluates the supplied testdata cookie to determine if it is populated with valid test information
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        public bool HasTestData(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestId != Guid.Empty;
        }

        /// <summary>
        /// Evaluates the supplied testdata cookie to determine if the user has been set as a participant.
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        public bool IsTestParticipant(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestVariantId != Guid.Empty;
        }

        /// <summary>
        /// Saves the supplied test data as a cookie
        /// </summary>
        /// <param name="testData"></param>
        public void SaveTestDataToCookie(TestDataCookie testData)
        {
            var aTest = _testRepo.GetTestById(testData.TestId,true);
            int varIndex = -1;

            if (testData.TestVariantId != Guid.Empty)
            {
                varIndex = aTest.Variants.FindIndex(i => i.Id == testData.TestVariantId);
            }

            var cookieData = new HttpCookie(COOKIE_PREFIX + testData.TestContentId.ToString() + _adminConfigTestSettingsHelper.GetCookieDelimeter() + aTest.ContentLanguage)
            {
                ["start"] = aTest.StartDate.ToString(CultureInfo.InvariantCulture),
                ["vId"] = varIndex.ToString(),
                ["viewed"] = testData.Viewed.ToString(),
                ["converted"] = testData.Converted.ToString(),
                Expires = aTest.EndDate,
                HttpOnly = true

            };
            testData.KpiConversionDictionary.OrderBy(x => x.Key);
            for (var x = 0; x < testData.KpiConversionDictionary.Count; x++)
            {
                cookieData["k" + x] = testData.KpiConversionDictionary.ToList()[x].Value.ToString();
            }

            _httpContextHelper.AddCookie(cookieData);
        }

        /// <summary>
        /// Updates the current cookie
        /// </summary>
        /// <param name="testData"></param>
        public void UpdateTestDataCookie(TestDataCookie testData)
        {
            _httpContextHelper.RemoveCookie(COOKIE_PREFIX + testData.TestContentId.ToString() + _adminConfigTestSettingsHelper.GetCookieDelimeter() + _episerverHelper.GetContentCultureinfo().Name);
            SaveTestDataToCookie(testData);
        }

        /// <summary>
        /// Gets the current cookie data
        /// </summary>
        /// <param name="testContentId"></param>
        /// <returns></returns>
        public TestDataCookie GetTestDataFromCookie(string testContentId, string cultureName = null)
        {
            var cookieDelimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter();
            var retCookie = new TestDataCookie();
            var currentCulture = cultureName != null ? CultureInfo.GetCultureInfo(cultureName) : _episerverHelper.GetContentCultureinfo();
            var currentCulturename = cultureName != null ? cultureName : _episerverHelper.GetContentCultureinfo().Name;

            // old cookies use : for delimeter, need to update them to use new delimeter, we should remove this in a future release.
            UpdateOldCookie(testContentId, currentCulture, currentCulturename);

            var cookieKey = COOKIE_PREFIX + testContentId + cookieDelimeter + currentCulturename;
            var cookie = _httpContextHelper.HasCookie(cookieKey)
                ? _httpContextHelper.GetResponseCookie(cookieKey)
                : _httpContextHelper.GetRequestCookie(cookieKey);

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                if (cookie.Value.Contains("start"))
                {
                    Guid outguid;
                    int outint = 0;
                    retCookie.TestContentId = Guid.TryParse(cookie.Name.Substring(COOKIE_PREFIX.Length).Split(cookieDelimeter[0])[0], out outguid) ? outguid : Guid.Empty;

                    bool outval;
                    
                    retCookie.TestStart = DateTime.Parse(cookie["start"], CultureInfo.InvariantCulture);
                    retCookie.Viewed = bool.TryParse(cookie["viewed"], out outval) ? outval : false;
                    retCookie.Converted = bool.TryParse(cookie["converted"], out outval) ? outval : false;

                    var test = _testRepo.GetActiveTestsByOriginalItemId(retCookie.TestContentId,currentCulture).FirstOrDefault();
                    
                    if (test != null && retCookie.TestStart.ToString(CultureInfo.InvariantCulture) == test.StartDate.ToString(CultureInfo.InvariantCulture))
                    {
                        var index = int.TryParse(cookie["vId"], out outint) ? outint : -1;
                        retCookie.TestVariantId = index != -1 ? test.Variants[outint].Id : Guid.Empty;
                        retCookie.ShowVariant = index != -1 ? !test.Variants[outint].IsPublished : false;
                        retCookie.TestId = test.Id;

                        var orderedKpiInstances = test.KpiInstances.OrderBy(x => x.Id).ToList();
                        test.KpiInstances = orderedKpiInstances;

                        for (var x = 0; x < test.KpiInstances.Count; x++)
                        {
                            bool converted = false;
                            bool.TryParse(cookie["k" + x], out converted);
                            retCookie.KpiConversionDictionary.Add(test.KpiInstances[x].Id, converted);
                            retCookie.AlwaysEval = Attribute.IsDefined(test.KpiInstances[x].GetType(), typeof(AlwaysEvaluateAttribute));
                        }
                    }
                    else
                    {
                        ExpireTestDataCookie(retCookie);
                        retCookie = new TestDataCookie();
                    }
                }
                else
                {
                    retCookie = GetTestDataFromOldCookie(cookie);
                }
            }
            return retCookie;
        }

        /// <summary>
        /// previous verions of cookies used a colon : delimeter which causes issues with cookie based authentication
        /// the colon is also an illegal charagter for cookie names.  this changes it to an underscore _
        /// </summary>
        /// <param name="testContentId"></param>
        /// <param name="currentCulture"></param>
        /// <param name="currentCulturename"></param>
        internal void UpdateOldCookie(string testContentId, CultureInfo currentCulture, string currentCulturename)
        {
            var updatedTestDataCookie = _testDataCookieMigrator.UpdateOldCookie(COOKIE_PREFIX, testContentId,
                currentCulture, currentCulturename);

            if (updatedTestDataCookie != null)
            {
                SaveTestDataToCookie(updatedTestDataCookie);

                var oldCookieName = COOKIE_PREFIX + testContentId + ":" + currentCulturename;
                _httpContextHelper.RemoveCookie(oldCookieName);

                var expiredCookie = new HttpCookie(oldCookieName)
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(-1d)
                };
                _httpContextHelper.AddCookie(expiredCookie);
            }
        }

        /// <summary>
        /// This method converts cookies which contain a language indicator but are in the long key format to the shorthand format.
        /// This is to support installs which may be using the long hand key format so as not to inturrupt or break their tests.
        /// We can remove this after a couple of revs once it is no longer a concern.
        /// </summary>
        /// <param name="testContentId"></param>
        /// <param name="cultureName"></param>
        /// <returns></returns>
        internal TestDataCookie GetTestDataFromOldCookie(HttpCookie cookieToConvert)
        {
            var retCookie = new TestDataCookie();

            Guid outguid;
            CultureInfo culture = CultureInfo.GetCultureInfo(cookieToConvert.Name.Split(_adminConfigTestSettingsHelper.GetCookieDelimeter()[0])[1]);
            retCookie.TestId = Guid.TryParse(cookieToConvert["TestId"], out outguid) ? outguid : Guid.Empty;
            retCookie.TestContentId = Guid.TryParse(cookieToConvert["TestContentId"], out outguid) ? outguid : Guid.Empty;
            retCookie.TestVariantId = Guid.TryParse(cookieToConvert["TestVariantId"], out outguid) ? outguid : Guid.Empty;

            bool outval;
            retCookie.ShowVariant = bool.TryParse(cookieToConvert["ShowVariant"], out outval) ? outval : false;
            retCookie.Viewed = bool.TryParse(cookieToConvert["Viewed"], out outval) ? outval : false;
            retCookie.Converted = bool.TryParse(cookieToConvert["Converted"], out outval) ? outval : false;
            retCookie.AlwaysEval = bool.TryParse(cookieToConvert["AlwaysEval"], out outval) ? outval : false;

            var test = _testRepo.GetActiveTestsByOriginalItemId(retCookie.TestContentId, culture).FirstOrDefault();
            if (test != null)
            {
                retCookie.TestStart = test.StartDate;
                foreach (var kpi in test.KpiInstances)
                {
                    bool converted = false;
                    bool.TryParse(cookieToConvert[kpi.Id + "-Flag"], out converted);
                    retCookie.KpiConversionDictionary.Add(kpi.Id, converted);
                }
                UpdateTestDataCookie(retCookie);
            }
            else
            {
                ExpireTestDataCookie(retCookie);
                retCookie = new TestDataCookie();
            }            
            return retCookie;
        }

        /// <summary>
        /// Sets the cookie associated with the supplied testData to expire
        /// </summary>
        /// <param name="testData"></param>
        public void ExpireTestDataCookie(TestDataCookie testData)
        {
            var cookieDelimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter();
            var cultureName = _episerverHelper.GetContentCultureinfo().Name;
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + cookieDelimeter + cultureName;
            _httpContextHelper.RemoveCookie(cookieKey);
            HttpCookie expiredCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId + cookieDelimeter + cultureName);
            expiredCookie.HttpOnly = true;
            expiredCookie.Expires = DateTime.Now.AddDays(-1d);
            _httpContextHelper.AddCookie(expiredCookie);
        }

        /// <summary>
        /// Resets the cookie associated with the supplied testData to an empty Test Data cookie.
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public TestDataCookie ResetTestDataCookie(TestDataCookie testData)
        {
            var cookieDelimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter();
            var cultureName = _episerverHelper.GetContentCultureinfo().Name;
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + cookieDelimeter + cultureName;
            _httpContextHelper.RemoveCookie(cookieKey);
            var resetCookie = new HttpCookie(COOKIE_PREFIX + testData.TestContentId + cookieDelimeter + cultureName) { HttpOnly = true };
            _httpContextHelper.AddCookie(resetCookie);
            return new TestDataCookie();
        }

        /// <summary>
        /// Gets test cookie data from both Response and Request.
        /// Fetching response cookies gets current cookie data for cookies actively being processed
        /// while fetching request cookies gets cookie data for cookies which have not been touched.
        /// This ensure a complete set of current cookie data and prevents missed views or duplicated conversions.
        /// </summary>
        /// <returns></returns>
        public IList<TestDataCookie> GetTestDataFromCookies()
        {
            var delimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter()[0];

            //Get up to date cookies data for cookies which are actively being processed
            var aResponseCookieKeys = _httpContextHelper.GetResponseCookieKeys();
            List<TestDataCookie> tdcList = (from name in aResponseCookieKeys
                                            where name.Contains(COOKIE_PREFIX)
                                            select GetTestDataFromCookie(name.Split(delimeter)[0].Substring(COOKIE_PREFIX.Length))).ToList();

            //Get cookie data from cookies not recently updated.
            tdcList.AddRange(from name in _httpContextHelper.GetRequestCookieKeys()
                             where name.Contains(COOKIE_PREFIX) &&
                             !aResponseCookieKeys.Contains(name)
                             select GetTestDataFromCookie(name.Split(delimeter)[0].Substring(COOKIE_PREFIX.Length)));

            return tdcList;
        }
    }
}
