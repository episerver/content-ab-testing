﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// interacts with the httpcontext for reading and manipulating the objects therein
    /// </summary>
    public class HttpContextHelper : IHttpContextHelper
    {
        public bool HasItem(string itemId)
        {
            return HttpContext.Current.Items.Contains(itemId);
        }

        public void SetItemValue(string itemId, object value)
        {
            HttpContext.Current.Items[itemId] = value;
        }

        public bool HasCookie(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies.AllKeys.Contains(cookieKey);
        }

        public string GetCookieValue(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies[cookieKey].Value;
        }

        public HttpCookie GetResponseCookie(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies.Get(cookieKey);
        }

        public HttpCookie GetRequestCookie(string cookieKey)
        {
            return HttpContext.Current.Request.Cookies.Get(cookieKey);
        }

        public string[] GetResponseCookieKeys()
        {
            return HttpContext.Current.Response.Cookies.AllKeys;
        }

        public string[] GetRequestCookieKeys()
        {
            return HttpContext.Current.Request.Cookies.AllKeys;
        }

        public void RemoveCookie(string cookieKey)
        {
            HttpContext.Current.Response.Cookies.Remove(cookieKey);
            HttpContext.Current.Request.Cookies.Remove(cookieKey);
        }

        public void AddCookie(HttpCookie cookie)
        {
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public bool CanWriteToResponse()
        {
            return HttpContext.Current.Response.Filter.CanWrite;
        }

        public Stream GetResponseFilter()
        {
            return HttpContext.Current.Response.Filter;
        }

        public void SetResponseFilter(Stream stream)
        {
            HttpContext.Current.Response.Filter = stream;
        }
    }
}
