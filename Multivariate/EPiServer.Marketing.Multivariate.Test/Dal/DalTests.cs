﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Multivariate.Test.Core;
using Xunit;

namespace EPiServer.Marketing.Multivariate.Test.Dal
{
    public class DalTests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;


        public DalTests()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
        }

        [Fact]
        public void AddMultivariateTest()
        {
            var newTests = AddMultivariateTests(_context, 2);
            _context.SaveChanges();

            Assert.Equal(_context.ABTests.Count(), 2);
        }

        [Fact]
        public void DeleteMultivariateTest()
        {
            var newTests = AddMultivariateTests(_context, 3);
            _context.SaveChanges();

            _context.ABTests.Remove(newTests[0]);
            _context.SaveChanges();

            Assert.Equal(_context.ABTests.Count(), 2);
        }

        [Fact]
        public void UpdateMultivariateTest()
        {
            var id = Guid.NewGuid();

            var test = new ABTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                State = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.ABTests.Add(test);
            _context.SaveChanges();

            var newTitle = "NewTitle";
            test.Title = newTitle;
            _context.SaveChanges();

            Assert.Equal(_context.ABTests.Find(id).Title, newTitle);
        }

        [Fact]
        public void AddVariantToTest()
        {
            var id = Guid.NewGuid();

            var test = new ABTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                State = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Variants = new List<Variant>()
            };

            _context.ABTests.Add(test);

            var variant = new Variant()
            {
                Id = Guid.NewGuid(),
                ItemId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow
            };

            test.Variants.Add(variant);
            _context.SaveChanges();

            Assert.Equal(test.Variants.Count(), 1);

            Assert.Equal(1, _context.Variants.Count());
        }

        [Fact]
        public void AddKeyPerformanceIndicatorToTest()
        {
            var id = Guid.NewGuid();

            var test = new ABTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                State = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                KeyPerformanceIndicators = new List<KeyPerformanceIndicator>()
            };

            _context.ABTests.Add(test);

            var kpi = new KeyPerformanceIndicator()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                KeyPerformanceIndicatorId = Guid.NewGuid()
            };

            test.KeyPerformanceIndicators.Add(kpi);
            _context.SaveChanges();

            Assert.Equal(1, test.KeyPerformanceIndicators.Count());

            Assert.Equal(1, _context.KeyPerformanceIndicators.Count());
        }

        [Fact]
        public void AddTestResultToTest()
        {
            var id = Guid.NewGuid();

            var test = new ABTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                State = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                TestResults = new List<TestResult>()
            };

            _context.ABTests.Add(test);

            var tr = new TestResult()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ItemId = Guid.NewGuid(),
                Conversions = 1,
                Views = 1
            };

            test.TestResults.Add(tr);
            _context.SaveChanges();

            Assert.Equal(1, test.TestResults.Count());

            Assert.Equal(1, _context.ABTestsResults.Count());
        }
    }
}
