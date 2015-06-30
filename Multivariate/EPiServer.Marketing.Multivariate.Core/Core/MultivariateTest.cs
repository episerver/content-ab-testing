﻿using System;
using System.Collections.Generic;
using System.Linq;

using EPiServer.Core;
using log4net;

namespace EPiServer.Marketing.Multivariate
{
    public class MultivariateTest : IMultivariateTest
    {
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string Owner { get; set; }
        
        public TestState State { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Guid OriginalItemId { get; set; }
        public Guid VariantItemId { get; set; }
        public Guid ConversionItemId { get; set; }

        public List<TestResult> Results { get; set; }
    }
}
