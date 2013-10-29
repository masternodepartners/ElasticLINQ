﻿// Copyright (c) Tier 3 Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 

using ElasticLinq.Test.TestSupport;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace ElasticLinq.Test
{
    public class ElasticQueryExtensionsTests
    {
        private class Sample { }

        [Fact]
        public void WhereAppliesToQueryIsAddedToExpressionTree()
        {
            const WhereTarget expectedTarget = WhereTarget.Query;
            var source = new FakeQueryProvider().CreateQuery<Sample>();
            var applied = source.WhereAppliesTo(expectedTarget);

            Assert.IsAssignableFrom<MethodCallExpression>(applied.Expression);
            var callExpression = (MethodCallExpression)applied.Expression;

            Assert.Equal(2, callExpression.Arguments.Count);
            Assert.IsType<ConstantExpression>(callExpression.Arguments[1]);
            Assert.Equal(((ConstantExpression)callExpression.Arguments[1]).Value, expectedTarget);
        }

        [Fact]
        public void WhereAppliesToFilterIsAddedToExpressionTree()
        {
            const WhereTarget expectedTarget = WhereTarget.Filter;
            var source = new FakeQueryProvider().CreateQuery<Sample>();
            var applied = source.WhereAppliesTo(expectedTarget);

            Assert.IsAssignableFrom<MethodCallExpression>(applied.Expression);
            var callExpression = (MethodCallExpression)applied.Expression;

            Assert.Equal(2, callExpression.Arguments.Count);
            Assert.IsType<ConstantExpression>(callExpression.Arguments[1]);
            Assert.Equal(((ConstantExpression)callExpression.Arguments[1]).Value, expectedTarget);
        }

        [Fact]
        public void OrderByScoreIsAddedToExpressionTree()
        {
            var source = new FakeQueryProvider().CreateQuery<Sample>();
            AssertIsAddedToExpressionTree<IQueryable<Sample>, Sample>(source, ElasticQueryExtensions.OrderByScore, "OrderByScore");
        }

        [Fact]
        public void OrderByScoreThrowArgumentNullExceptionIfSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ElasticQueryExtensions.OrderByScore<Sample>(null));
        }

        [Fact]
        public void OrderByScoreDescendingIsAddedToExpressionTree()
        {
            var source = new FakeQueryProvider().CreateQuery<Sample>();
            AssertIsAddedToExpressionTree<IQueryable<Sample>, Sample>(source, ElasticQueryExtensions.OrderByScoreDescending, "OrderByScoreDescending");
        }

        [Fact]
        [ExcludeFromCodeCoverage] // Expression isn't "executed"
        public void OrderByScoreDescendingThrowArgumentNullExceptionIfSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ElasticQueryExtensions.OrderByScoreDescending<Sample>(null));
        }

        [Fact]
        public void ThenByScoreIsAddedToExpressionTree()
        {
            var source = new FakeQueryProvider().CreateQuery<Sample>().OrderByScore();
            AssertIsAddedToExpressionTree<IOrderedQueryable<Sample>, Sample>(source, ElasticQueryExtensions.ThenByScore, "ThenByScore");
        }

        [Fact]
        [ExcludeFromCodeCoverage] // Expression isn't "executed"
        public void ThenByScoreThrowArgumentNullExceptionIfSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ElasticQueryExtensions.ThenByScore<Sample>(null));
        }

        [Fact]
        public void ThenByScoreDescendingIsAddedToExpressionTree()
        {
            var source = new FakeQueryProvider().CreateQuery<Sample>().OrderByScore();
            AssertIsAddedToExpressionTree<IOrderedQueryable<Sample>, Sample>(source, ElasticQueryExtensions.ThenByScoreDescending, "ThenByScoreDescending");
        }

        [Fact]
        [ExcludeFromCodeCoverage] // Expression isn't "executed"
        public void ThenByScoreDescendingThrowArgumentNullExceptionIfSourceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ElasticQueryExtensions.ThenByScoreDescending<Sample>(null));
        }

        private static void AssertIsAddedToExpressionTree<TSequence, TElement>(TSequence source, Func<TSequence, TSequence> method, string methodName)
            where TSequence : IQueryable<TElement>
        {
            var afterMethod = method(source);
            var final = afterMethod.ToList();

            Assert.NotSame(source, afterMethod);
            Assert.NotNull(final);

            Assert.IsType<FakeQueryProvider>(source.Provider);
            var finalExpression = ((FakeQueryProvider)source.Provider).FinalExpression;
            Assert.Equal(ExpressionType.Call, finalExpression.NodeType);
            Assert.Equal(methodName, ((MethodCallExpression)finalExpression).Method.Name);
        }
    }
}