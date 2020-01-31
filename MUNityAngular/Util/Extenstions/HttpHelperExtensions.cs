﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MUNityAngular.Util.Extenstions
{
    public static class HttpHelperExtensions
    {
        public static string DecodeUrl(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var realtext = System.Web.HttpUtility.UrlDecode(input);
            if (realtext == null)
                return string.Empty;

            if (realtext.EndsWith("|"))
                realtext = realtext.Substring(0, realtext.Length - 1);

            return realtext;
        }
    }
}
