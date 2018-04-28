using FakeXrmEasy;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Oss.FluentQuery.Tests
{
    [TestFixture]
    public class FluentFilterExpressionTests
    {
        [Test]
        public void It_Should_Properly_Set_Simple_Filter()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .Where(e => e
                    .Attribute(a => a
                        .Of("contact")
                        .Named("name")
                        .Is(ConditionOperator.Equal)
                        .Value("Test")
                    )
                    .With.Operator(LogicalOperator.And)
                )
                .Expression;

            Assert.That(query.Criteria.FilterOperator, Is.EqualTo(LogicalOperator.And));
            Assert.That(query.Criteria.Conditions[0].EntityName, Is.EqualTo("contact"));
            Assert.That(query.Criteria.Conditions[0].AttributeName, Is.EqualTo("name"));
            Assert.That(query.Criteria.Conditions[0].Operator, Is.EqualTo(ConditionOperator.Equal));
            Assert.That(query.Criteria.Conditions[0].Values, Is.EqualTo(new[] { "Test" }));
        }

        [Test]
        public void It_Should_Properly_Set_Nested_Filter()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .Where(e => e
                    .Attribute(a => a
                        .Of("contact")
                        .Named("name")
                        .Is(ConditionOperator.Equal)
                        .Value("Test")
                    )
                    .With.Operator(LogicalOperator.And)
                    .Where(e2 => e2
                        .Attribute(a => a
                            .Of("contact2")
                            .Named("name2")
                            .Is(ConditionOperator.NotEqual)
                            .Value("Test2")
                        )
                        .With.Operator(LogicalOperator.Or)
                    )
                )
                .Expression;

            Assert.That(query.Criteria.FilterOperator, Is.EqualTo(LogicalOperator.And));

            Assert.That(query.Criteria.Conditions[0].EntityName, Is.EqualTo("contact"));
            Assert.That(query.Criteria.Conditions[0].AttributeName, Is.EqualTo("name"));
            Assert.That(query.Criteria.Conditions[0].Operator, Is.EqualTo(ConditionOperator.Equal));
            Assert.That(query.Criteria.Conditions[0].Values, Is.EqualTo(new[] { "Test" }));

            var nestedFilter = query.Criteria.Filters[0];
            Assert.That(nestedFilter.FilterOperator, Is.EqualTo(LogicalOperator.Or));

            Assert.That(nestedFilter.Conditions[0].EntityName, Is.EqualTo("contact2"));
            Assert.That(nestedFilter.Conditions[0].AttributeName, Is.EqualTo("name2"));
            Assert.That(nestedFilter.Conditions[0].Operator, Is.EqualTo(ConditionOperator.NotEqual));
            Assert.That(nestedFilter.Conditions[0].Values, Is.EqualTo(new[] { "Test2" }));
        }
    }
}
