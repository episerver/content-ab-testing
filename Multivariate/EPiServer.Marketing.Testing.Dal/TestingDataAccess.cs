﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using System.Reflection;

namespace EPiServer.Marketing.Testing.Dal
{
    public class TestingDataAccess : ITestingDataAccess
    {
        internal IRepository _repository;

        public TestingDataAccess()
        {
            // TODO : Load repository from service locator.
            _repository = new BaseRepository(new DatabaseContext());
        }
        internal TestingDataAccess(IRepository repository)
        {
            _repository = repository;
        }

        public void Archive(Guid testObjectId)
        {
            SetTestState(testObjectId, TestState.Archived);
        }

        public void Delete(Guid testObjectId)
        {
            _repository.DeleteTest(testObjectId);
            _repository.SaveChanges();
        }

        public IABTest Get(Guid testObjectId)
        {
            return _repository.GetById(testObjectId);
        }

        public List<IABTest> GetTestByItemId(Guid originalItemId)
        {
            return _repository.GetAll().Where(t => t.OriginalItemId == originalItemId).ToList();
        }

        public List<IABTest> GetTestList(TestCriteria criteria)
        {
            // if no filters are passed in, just return all tests
            var filters = criteria.GetFilters();
            if (!filters.Any())
            {
                return _repository.GetAll().ToList();
            }

            var variantOperator = FilterOperator.And;
            IQueryable<IABTest> variantResults = null;
            var variantId = Guid.Empty;
            var pe = Expression.Parameter(typeof(ABTest), "test");
            Expression wholeExpression = null;

            // build up expression tree based on the filters that are passed in
            foreach (var filter in filters)
            {
                // if we are filtering on a single property(not an element in a list) create the expression
                if (filter.Property != MultivariateTestProperty.VariantId)
                {
                    var left = Expression.Property(pe, typeof (ABTest).GetProperty(filter.Property.ToString()));
                    var right = Expression.Constant(filter.Value);
                    var expression = Expression.Equal(left, right);

                    // first time through, so we just set the expression to the first filter criteria and continue to the next one
                    if (wholeExpression == null)
                    {
                        wholeExpression = expression;
                        continue;
                    }

                    // each subsequent iteration we check to see if the filter is for an AND or OR and append accordingly
                    wholeExpression = filter.Operator == FilterOperator.And
                        ? Expression.And(wholeExpression, expression) : Expression.Or(wholeExpression, expression);
                }
                else // if we are filtering on an item in a list, then generate simple results that we can lump in at the end
                {
                    variantId = new Guid(filter.Value.ToString());
                    variantOperator = filter.Operator;
                    variantResults = _repository.GetAll().Where(x => x.Variants.Any(v => v.VariantId == variantId));
                }
            }

            IQueryable<IABTest> results = null;
            var tests = _repository.GetAll().AsQueryable();

            // if we have created an expression tree, then execute it against the tests to get the results
            if (wholeExpression != null)
            {
                var whereCallExpression = Expression.Call(
                    typeof (Queryable),
                    "Where",
                    new Type[] {tests.ElementType},
                    tests.Expression,
                    Expression.Lambda<Func<ABTest, bool>>(wholeExpression, new ParameterExpression[] {pe})
                    );

                results = tests.Provider.CreateQuery<ABTest>(whereCallExpression);
            }

            // if we are also filtering against a variantId, include those results
            if (variantResults != null)
            {
                if (results == null)
                {
                    return variantResults.ToList();
                }

                results = variantOperator == FilterOperator.And 
                    ? results.Where(test => test.Variants.Any(v => v.VariantId == variantId)) 
                    : results.Concat(variantResults).Distinct();
            }

            return results.ToList<IABTest>();
        }


        public void IncrementCount(Guid testId, Guid testItemId, CountType resultType)
        {
            var test = _repository.GetById(testId);
            var result = test.MultivariateTestResults.FirstOrDefault(v => v.ItemId == testItemId);

            if (resultType == CountType.View)
            {
                result.Views++;
            }
            else
            {
                result.Conversions++;
            }

            Save(test);
        }

        public Guid Save(IABTest testObject)
        {
            var test = _repository.GetById(testObject.Id) as ABTest;
            Guid id;

            // if a test doesn't exist, add it to the db
            if (test == null)
            {
                _repository.Add(testObject);
                id = testObject.Id;
            }
            else
            {
                switch (test.TestState)
                {
                    case TestState.Inactive:
                        // update test properties
                        test.Title = testObject.Title;
                        test.StartDate = testObject.StartDate;
                        test.EndDate = testObject.EndDate;
                        test.ModifiedDate = DateTime.UtcNow;
                        test.LastModifiedBy = testObject.LastModifiedBy;
                        test.TestState = testObject.TestState;

                        // update original item and corresponding result record if different
                        if (test.OriginalItemId != testObject.OriginalItemId)
                        {
                            var result =
                                test.MultivariateTestResults.FirstOrDefault(r => r.ItemId == test.OriginalItemId);
                            if (null != result)
                            {
                                test.MultivariateTestResults.Remove(result);
                                _repository.Delete(result);
                            }

                            test.OriginalItemId = testObject.OriginalItemId;
                            test.MultivariateTestResults.Add(
                                new TestResult()
                                {
                                    Id = Guid.NewGuid(),
                                    ItemId = testObject.OriginalItemId
                                }
                                );
                        }

                        // variant items and corresponding result records
                        var removeVariant = true;
                        var variantsToAdd = new List<Variant>(testObject.Variants);

                        // loop over current variants to see if they are also the new variants
                        // if so, leave them as is, if not, remove them from the test
                        foreach (var currentVariant in test.Variants.ToArray())
                        {
                            foreach (var newVariant in
                                testObject.Variants.Where(newVariant => currentVariant.VariantId == newVariant.VariantId)
                                )
                            {
                                removeVariant = false;
                                variantsToAdd.Remove(newVariant);
                                break;
                            }

                            // if the currentVariant is not in the new testObject, then we need to remove it from the existing test along with its result
                            if (removeVariant)
                            {
                                test.Variants.Remove(currentVariant);
                                _repository.Delete(currentVariant);
                                var result =
                                    test.MultivariateTestResults.FirstOrDefault(
                                        r => r.ItemId == currentVariant.VariantId);

                                if (null != result)
                                {
                                    test.MultivariateTestResults.Remove(result);
                                    _repository.Delete(result);
                                }
                            }

                            removeVariant = true;
                        }

                        // add remaining new variants from testObject to the existing test along with their results
                        foreach (var variant in variantsToAdd)
                        {
                            test.Variants.Add(variant);
                            test.MultivariateTestResults.Add(
                                new TestResult()
                                {
                                    Id = Guid.NewGuid(),
                                    ItemId = variant.VariantId
                                });
                        }

                        id = test.Id;
                        break;
                    case TestState.Active:
                        if (testObject.TestState == TestState.Done)
                        {
                            test.TestState = TestState.Done;
                        }
                        test.EndDate = testObject.EndDate;
                        id = test.Id;
                        break;
                    case TestState.Done:
                        if (testObject.TestState == TestState.Archived)
                        {
                            test.TestState = TestState.Archived;
                        }
                        id = test.Id;
                        break;
                    default:
                        id = test.Id;
                        break;
                }
            }

            _repository.SaveChanges();

            return id;
        }

        public void Start(Guid testObjectId)
        {
            var test = _repository.GetById(testObjectId);
            if (IsTestActive(test.OriginalItemId))
            {
                throw new Exception("The test page already has an Active test");
            }

            SetTestState(testObjectId, TestState.Active);
        }

        public void Stop(Guid testObjectId)
        {
            SetTestState(testObjectId, TestState.Done);
        }
        private bool IsTestActive(Guid originalItemId)
        {
            var tests = _repository.GetAll()
                .Where(t => t.OriginalItemId == originalItemId && t.TestState == TestState.Active);

            return tests.Any();
        }
        private void SetTestState(Guid theTestId, TestState theState)
        {
            var aTest = _repository.GetById(theTestId);
            aTest.TestState = theState;
            Save(aTest);
        }
    }
}
