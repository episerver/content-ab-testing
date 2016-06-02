﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.Exceptions;

namespace EPiServer.Marketing.Testing.Web
{
    internal class TestHandler : ITestHandler
    {
        internal List<ContentReference> ProcessedContentList;
        private readonly ITestingContextHelper _contextHelper = new TestingContextHelper();
        private readonly ITestDataCookieHelper _testDataCookieHelper = new TestDataCookieHelper();

        private ITestManager _testManager;
        private bool? _swapDisabled;
        public bool? SwapDisabled
        {
            set { _swapDisabled = value; }

            get
            {
                if (_swapDisabled == null)
                {
                    return _contextHelper.IsInSystemFolder();

                }
                return _swapDisabled;
            }
        }

        [ExcludeFromCodeCoverage]
        public TestHandler()
        {

        }

        [ExcludeFromCodeCoverage]
        internal TestHandler(ITestManager testManager, ITestDataCookieHelper cookieHelper, List<ContentReference> processedList, ITestingContextHelper contextHelper)
        {
            _testDataCookieHelper = cookieHelper;
            ProcessedContentList = processedList;
            _testManager = testManager;
            _contextHelper = contextHelper;
        }

        [ExcludeFromCodeCoverage]
        public void Initialize()
        {
            _testManager = new TestManager();
            ProcessedContentList = new List<ContentReference>();
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedContent += LoadedContent;
        }

        private void EvaluateKpis(ContentEventArgs e)
        {
            var cdl = _testDataCookieHelper.getTestDataFromCookies();
            foreach (var testdata in cdl)
            {
                // for every test cookie we have, check for the converted and the viewed flag
                if (!testdata.Converted && testdata.Viewed)
                {
                    try
                    {
                        var test = _testManager.Get(testdata.TestId);

                        // optimization : create the list of kpis that have not evaluated 
                        // to true and then evaluate them
                        var kpis = new List<IKpi>();
                        foreach (var kpi in test.KpiInstances)
                        {
                            var converted = testdata.KpiConversionDictionary.First(x => x.Key == kpi.Id).Value;
                            if (!converted)
                                kpis.Add(kpi);
                        }

                        var evaluated = _testManager.EvaluateKPIs(kpis, e.Content);
                        if (evaluated.Count > 0)
                        {
                            // add each kpi to testdata cookie data
                            foreach (var eval in evaluated)
                            {
                                testdata.KpiConversionDictionary.Remove(eval);
                                testdata.KpiConversionDictionary.Add(eval, true);
                            }

                            // now check to see if all kpi objects have evalated
                            testdata.Converted = !testdata.KpiConversionDictionary.Where(x => x.Value == false).Any();

                            // now save the testdata to the cookie
                            _testDataCookieHelper.SaveTestDataToCookie(testdata);

                            // now if we have converted, fire the converted message 
                            // note : we wouldnt be here if we already converted on a previous loop
                            if (testdata.Converted)
                            {
                                Variant varUserSees = test.Variants.First(x => x.Id == testdata.TestVariantId);
                                _testManager.EmitUpdateCount(test.Id, varUserSees.ItemId, varUserSees.ItemVersion, CountType.Conversion);
                            }
                        }
                    }
                    catch(TestNotFoundException )
                    {
                        // now save the testdata to the cookie
                        _testDataCookieHelper.ExpireTestDataCookie(testdata);
                    }
                }
            }
        }

        public void LoadedContent(object sender, ContentEventArgs e)
        {
            if (!SwapDisabled == true)
            {
                IContent CurrentPage = _contextHelper.GetCurrentPageFromUrl();

                if (e.TargetLink != null)
                {
                    EvaluateKpis(e);    // new method to evaluate Kpi
                }

                // Causing numerous errors at startup
                if (e.Content == null)
                    return;

                var activeTest =
                    _testManager.CreateActiveTestCache().FirstOrDefault(x => x.OriginalItemId == e.Content.ContentGuid);

                var testCookieData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());
                var hasData = _testDataCookieHelper.HasTestData(testCookieData);

                if (activeTest != null)
                {
                    if (hasData && _testDataCookieHelper.IsTestParticipant(testCookieData) && testCookieData.ShowVariant)
                    {
                        ProcessedContentList.Add(e.ContentLink);
                        Swap(testCookieData, e);
                    }
                    else if (!hasData && CurrentPage !=null && _contextHelper.IsRequestedContent(CurrentPage,e.Content) && ProcessedContentList.Count == 0)
                    {
                        ProcessedContentList.Add(e.ContentLink);
                        //get a new random variant. 
                        var newVariant = _testManager.ReturnLandingPage(activeTest.Id);
                        testCookieData.TestId = activeTest.Id;
                        testCookieData.TestContentId = activeTest.OriginalItemId;
                        testCookieData.TestVariantId = newVariant.Id;

                        foreach (var kpi in activeTest.KpiInstances)
                        {
                            testCookieData.KpiConversionDictionary.Add(kpi.Id, false);
                        }

                        if (newVariant.Id != Guid.Empty)
                        {
                            var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;

                            if (newVariant.ItemVersion != contentVersion)
                            {
                                contentVersion = newVariant.ItemVersion;
                                testCookieData.ShowVariant = true;
                                _testDataCookieHelper.SaveTestDataToCookie(testCookieData);

                                Swap(testCookieData, e);
                            }
                            else
                            {
                                testCookieData.ShowVariant = false;
                            }

                            CalculateView(testCookieData, contentVersion);
                        }
                    }
                }
                else if (hasData)
                {
                    _testDataCookieHelper.ExpireTestDataCookie(testCookieData);
                }
            }
        }

        private void Swap(TestDataCookie cookie, ContentEventArgs activeContent)
        {
            if (cookie.ShowVariant)
            {
                var variant = _testManager.CreateVariantPageDataCache(activeContent.Content.ContentGuid, ProcessedContentList);
                //swap it with the cached version
                if (variant != null)
                {
                    activeContent.ContentLink = variant.ContentLink;
                    activeContent.Content = variant;
                }
            }
        }

        private void CalculateView(TestDataCookie cookie, int contentVersion)
        {
            //increment view if not already done
            if (cookie.Viewed == false)
            {
                _testManager.IncrementCount(cookie.TestId, cookie.TestContentId, contentVersion,
                    CountType.View);
            }
            //set viewed = true in testdata
            cookie.Viewed = true;
            _testDataCookieHelper.UpdateTestDataCookie(cookie);
        }

        public void Uninitialize()
        {
        }
    }
}