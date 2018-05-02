using FakeItEasy;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
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
    public class FluentQueryTests
    {
        private class Account : Entity
        {
            public Account() : base("account")
            {
            }
        }

        [Test]
        public void It_Should_Set_Entity_Name()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .IncludeColumns("name", "address1_line1")
                .Expression;

            Assert.That(query.EntityName, Is.EqualTo("account"));
        }

        [Test]
        public void It_Should_Set_Entity_Name_On_Generic_With_Parameter()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query<Account>("account")
                .IncludeColumns("name", "address1_line1")
                .Expression;

            Assert.That(query.EntityName, Is.EqualTo("account"));
        }

        [Test]
        public void It_Should_Set_Entity_Name_On_Early_Binding_Automatically()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query<Account>()
                .IncludeColumns("name", "address1_line1")
                .Expression;

            Assert.That(query.EntityName, Is.EqualTo("account"));
        }

        [Test]
        public void It_Should_Execute_Simple_Query()
        {
            var context = new XrmFakedContext();

            var testName = "Adventure Works";
            var testAddress = "Somewhere over the rainbow";

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", testName },
                    { "address1_line1", testAddress }
                }
            };
            context.Initialize(new[] { account });

            var service = context.GetFakedOrganizationService();

            var records = service.Query("account")
                .IncludeColumns("name", "address1_line1")
                .Retrieve();

            Assert.That(records.Count, Is.EqualTo(1));

            var record = records.Single();
            Assert.That(record.GetAttributeValue<string>("name"), Is.EqualTo(testName));
            Assert.That(record.GetAttributeValue<string>("address1_line1"), Is.EqualTo(testAddress));
        }

        [Test]
        public void It_Should_Add_All_Columns()
        {
            var context = new XrmFakedContext();

            var testName = "Adventure Works";
            var testAddress = "Somewhere over the rainbow";

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", testName },
                    { "address1_line1", testAddress }
                }
            };
            context.Initialize(new[] { account });

            var service = context.GetFakedOrganizationService();

            var records = service.Query("account")
                .IncludeAllColumns()
                .Retrieve();

            Assert.That(records.Count, Is.EqualTo(1));

            var record = records.Single();
            Assert.That(record.GetAttributeValue<string>("name"), Is.EqualTo(testName));
            Assert.That(record.GetAttributeValue<string>("address1_line1"), Is.EqualTo(testAddress));
        }

        [Test]
        public void It_Should_Retrieve_All()
        {
            var context = new XrmFakedContext();

            var testName = "Adventure Works";
            var testAddress = "Somewhere over the rainbow";

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", testName },
                    { "address1_line1", testAddress }
                }
            };

            var account2 = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", testName },
                    { "address1_line1", testAddress }
                }
            };

            context.Initialize(new[] { account, account2 });

            var service = context.GetFakedOrganizationService();

            var records = service.Query("account")
                .IncludeColumns("name", "address1_line1")
                .With.PagingInfo(p => p
                    .PageNumber(1)
                    .PageSize(1)
                )
                .RetrieveAll();

            Assert.That(records.Count, Is.EqualTo(2));

            Assert.That(records.Any(r => r.Id == account.Id), Is.True);
            Assert.That(records.Any(r => r.Id == account2.Id), Is.True);

            // FakeXrmEasy Bug?
            // A.CallTo(() => service.RetrieveMultiple(A<QueryExpression>._)).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Test]
        public void It_Should_Include_Columns()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();
            var columns = new[] { "name", "address1_line1" };

            var query = service.Query("account")
                .IncludeColumns(columns)
                .Expression;

            Assert.That(query.ColumnSet.Columns.ToArray(), Is.EquivalentTo(columns));
        }

        [Test]
        public void It_Should_Concat_Columns()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();
            var columns = new[] { "name", "address1_line1" };

            var query = service.Query("account")
                .IncludeColumns("name")
                .IncludeColumns("address1_line1")
                .Expression;

            Assert.That(query.ColumnSet.Columns.ToArray(), Is.EquivalentTo(columns));
        }

        [Test]
        public void It_Should_Set_Record_Count()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .With.RecordCount(10)
                .Expression;

            Assert.That(query.TopCount, Is.EqualTo(10));
        }

        [Test]
        public void It_Should_Set_Database_Lock()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .With.DatabaseLock()
                .Expression;

            Assert.That(query.NoLock, Is.EqualTo(false));
        }

        [Test]
        public void It_Should_Set_Database_Unique_Records()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .With.UniqueRecords()
                .Expression;

            Assert.That(query.Distinct, Is.EqualTo(true));
        }

        [Test]
        public void It_Should_Set_Total_Record_Count()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .With.TotalRecordCount()
                .Expression;

            Assert.That(query.PageInfo.ReturnTotalRecordCount, Is.EqualTo(true));
        }

        [Test]
        public void It_Should_Set_Paging_Info()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .With.PagingInfo(p => p
                    .PageNumber(1)
                )
                .Expression;

            Assert.That(query.PageInfo.PageNumber, Is.EqualTo(1));
        }

        [Test]
        public void It_Should_Properly_Execute_Query()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", "Adventure Works" }
                }
            };

            var account2 = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", "Contoso" },
                }
            };

            context.Initialize(new[] { account, account2 });

            var result = service.Query("account")
                .IncludeColumns("name")
                .Where(e => e
                    .Attribute(a => a
                        .Named("name")
                        .Is(ConditionOperator.Equal)
                        .To("Adventure Works")
                    )
                )
                .Link(l => l
                    .FromEntity("account")
                    .FromAttribute("primarycontactid")
                    .ToEntity("contact")
                    .ToAttribute("contactid")
                    .With.LinkType(JoinOperator.LeftOuter)
                )
                .Retrieve();
            
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(account.Id));
            Assert.That(result[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));
        }

        [Test]
        public void It_Should_Add_Condition_Post_Creation()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account");

            query.AddCondition(c => c
                .Named("emailaddress1")
                .Is(ConditionOperator.NotNull)
            );

            var expression = query.Expression;

            Assert.That(expression.Criteria.Conditions[0].AttributeName, Is.EqualTo("emailaddress1"));
            Assert.That(expression.Criteria.Conditions[0].Operator, Is.EqualTo(ConditionOperator.NotNull));
        }

        [Test]
        public void It_Should_Add_Filter_Post_Creation()
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
                );

            query.AddFilter(f => f
                .Attribute(a => a
                    .Named("emailaddress1")
                    .Is(ConditionOperator.NotNull)
                )
            );

            var expression = query.Expression;

            Assert.That(expression.Criteria.FilterOperator, Is.EqualTo(LogicalOperator.And));
            Assert.That(expression.Criteria.Conditions[0].EntityName, Is.EqualTo("contact"));
            Assert.That(expression.Criteria.Conditions[0].AttributeName, Is.EqualTo("name"));
            Assert.That(expression.Criteria.Conditions[0].Operator, Is.EqualTo(ConditionOperator.Equal));
            Assert.That(expression.Criteria.Conditions[0].Values, Is.EqualTo(new[] { "Test" }));

            Assert.That(expression.Criteria.Filters[0].Conditions[0].AttributeName, Is.EqualTo("emailaddress1"));
            Assert.That(expression.Criteria.Filters[0].Conditions[0].Operator, Is.EqualTo(ConditionOperator.NotNull));
        }

        [Test]
        public void It_Should_Add_Top_Level_Filter_Post_Creation()
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
                );

            query.AddFilter(f => f
                .Attribute(a => a
                    .Named("emailaddress1")
                    .Is(ConditionOperator.NotNull)
                )
            );

            var expression = query.Expression;

            Assert.That(expression.Criteria.FilterOperator, Is.EqualTo(LogicalOperator.And));
            Assert.That(expression.Criteria.Conditions[0].EntityName, Is.EqualTo("contact"));
            Assert.That(expression.Criteria.Conditions[0].AttributeName, Is.EqualTo("name"));
            Assert.That(expression.Criteria.Conditions[0].Operator, Is.EqualTo(ConditionOperator.Equal));
            Assert.That(expression.Criteria.Conditions[0].Values, Is.EqualTo(new[] { "Test" }));

            Assert.That(expression.Criteria.Filters[0].Conditions[0].AttributeName, Is.EqualTo("emailaddress1"));
            Assert.That(expression.Criteria.Filters[0].Conditions[0].Operator, Is.EqualTo(ConditionOperator.NotNull));
        }
    }
}
