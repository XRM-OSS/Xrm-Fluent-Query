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
                .With.PageInfo(new PagingInfo { PageNumber = 1, Count = 1})
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
    }
}
