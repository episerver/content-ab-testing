﻿using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using System.Runtime.Caching;
using EPiServer.Editor;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web
{
    internal class TestHandler
    {
        internal List<ContentReference> ProcessedContentList;
        private TestDataCookie _testData;
        private TestDataCookieHelper _testDataCookieHelper;
        private readonly MemoryCache _marketingTestCache = MemoryCache.Default;
        private readonly TestManager _testManager = new TestManager();



        internal void Initialize()
        {
            _testData = new TestDataCookie();
            _testDataCookieHelper = new TestDataCookieHelper();
            ProcessedContentList = new List<ContentReference>();

            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedContent += LoadedContent;

        }


        private void LoadedContent(object sender, ContentEventArgs e)
        {
            ProcessedContentList.Add(e.ContentLink);
            PageData variant = _marketingTestCache.Get(e.Content.ContentGuid.ToString()) as PageData;


            _testData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());


            if (_testDataCookieHelper.HasTestData(_testData)
                && _testDataCookieHelper.IsTestParticipant(_testData)
                && !PageEditing.PageIsInEditMode)
            {
                if (_testData.ShowVariant)
                {
                    //swap it with the cached version
                    Swap(variant, e);
                }
            }
            else if (e.Content is PageData
              && ContentUnderTest(e.Content.ContentGuid)
              && ProcessedContentList.Count == 1
              && !_testDataCookieHelper.HasTestData(_testData)
              && !PageEditing.PageIsInEditMode)
            {
                //get the cached content variant in case we need it.
                if (variant == null)
                {
                    _testManager.CreateCacheVariant(e.Content.ContentGuid);
                    variant = _marketingTestCache.Get(e.Content.ContentGuid.ToString()) as PageData;
                }

                //get a new random variant. 
                Variant newVariant = GetVariant(e.Content.ContentGuid);
                _testData.TestId = GetActiveTestGuid(e.Content.ContentGuid);
                _testData.TestContentId = e.Content.ContentGuid;
                _testData.TestVariantId = newVariant.Id;

                if (newVariant.Id != Guid.Empty)
                {
                    var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;

                    if (newVariant.ItemVersion != contentVersion)
                    {
                        contentVersion = newVariant.ItemVersion;

                        _testData.ShowVariant = true;

                        Swap(variant, e);
                    }
                    else
                    {
                        _testData.ShowVariant = false;
                    }

                    CalculateView(contentVersion);
                }
                else
                {
                    _testData.TestVariantId = Guid.Empty;
                }

                _testDataCookieHelper.SaveTestDataToCookie(_testData);
            }
        }

        private void Swap(PageData variant, ContentEventArgs activeContent)
        {
            if (_testData.ShowVariant)
            {
                //swap it with the cached version
                if (variant != null)
                {
                    activeContent.ContentLink = variant.ContentLink;
                    activeContent.Content = variant;
                }
            }
        }

        private void CalculateView(int contentVersion)
        {
            //increment view if not already done
            if (_testData.Viewed == false)
            {
                _testManager.IncrementCount(_testData.TestId, _testData.TestVariantId, contentVersion,
                    CountType.View);
            }
            //set viewed = true in testdata
            _testData.Viewed = true;
        }

        private Guid GetActiveTestGuid(Guid contentGuid)
        {
            Guid activeTestGuid = _testManager.GetTestByItemId(contentGuid).Where(x => x.OriginalItemId == contentGuid).Where(x => x.State == TestState.Active).Select(x => x.Id).First();
            return activeTestGuid;
        }

        private bool ContentUnderTest(Guid contentGuid)
        {
            var contentUnderTest = _testManager.CreateActiveTestCache();
            return contentUnderTest.Any(x => x.OriginalItemId == contentGuid);
        }

        private Variant GetVariant(Guid targetContentGuid)
        {
            var test = _testManager.GetTestByItemId(targetContentGuid).First(x => x.State.Equals(TestState.Active));
            return _testManager.ReturnLandingPage(test.Id);
        }

        public void Uninitialize()
        {
        }
    }


}