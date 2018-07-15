using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FakeXrmEasy;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;

namespace Xrm.Oss.FluentQuery.Tests
{
    [TestFixture]
    public class CacheTests
    {
        private List<Entity> ExecuteQuery(MemoryCache memoryCache, IOrganizationService service)
        {
            return service.Query("account")
                .IncludeAllColumns()
                .UseCache(memoryCache, new DateTimeOffset(DateTime.UtcNow.AddHours(1)))
                .Retrieve();
        }

        private List<Entity> ExecuteQueryWithExplicitColumns(MemoryCache memoryCache, IOrganizationService service)
        {
            return service.Query("account")
                .IncludeColumns("name")
                .UseCache(memoryCache, new DateTimeOffset(DateTime.UtcNow.AddHours(1)))
                .Retrieve();
        }
        
        private List<Entity> ExecuteQueryAll(MemoryCache memoryCache, IOrganizationService service)
        {
            return service.Query("account")
                .IncludeAllColumns()
                .UseCache(memoryCache, new DateTimeOffset(DateTime.UtcNow.AddHours(1)))
                .RetrieveAll();
        }

        private List<Entity> ExecuteQueryWithExplicitColumnsAll(MemoryCache memoryCache, IOrganizationService service)
        {
            return service.Query("account")
                .IncludeColumns("name")
                .UseCache(memoryCache, new DateTimeOffset(DateTime.UtcNow.AddHours(1)))
                .RetrieveAll();
        }

        [Test]
        public void It_Should_Use_Result_From_Cache_If_Found()
        {
            var memoryCache = new MemoryCache("test");

            var context = new XrmFakedContext();

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", "Adventure Works" }
                }
            };
            context.Initialize(new[] { account });

            var service = context.GetFakedOrganizationService();
            var results = ExecuteQuery(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            results = ExecuteQuery(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            A.CallTo(() => service.RetrieveMultiple(A<QueryExpression>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void It_Should_Differentiate_Queries()
        {
            var memoryCache = new MemoryCache("test");

            var context = new XrmFakedContext();

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", "Adventure Works" }
                }
            };
            context.Initialize(new[] { account });

            var service = context.GetFakedOrganizationService();
            var results = ExecuteQuery(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            results = ExecuteQueryWithExplicitColumns(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            A.CallTo(() => service.RetrieveMultiple(A<QueryExpression>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);
        }
        
        [Test]
        public void It_Should_Use_Result_From_Cache_If_Found_Retrieve_All()
        {
            var memoryCache = new MemoryCache("test");

            var context = new XrmFakedContext();

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", "Adventure Works" }
                }
            };
            context.Initialize(new[] { account });

            var service = context.GetFakedOrganizationService();
            var results = ExecuteQueryAll(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            results = ExecuteQueryAll(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            A.CallTo(() => service.RetrieveMultiple(A<QueryExpression>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void It_Should_Differentiate_Queries_Retrieve_All()
        {
            var memoryCache = new MemoryCache("test");

            var context = new XrmFakedContext();

            var account = new Entity
            {
                Id = Guid.NewGuid(),
                LogicalName = "account",
                Attributes =
                {
                    { "name", "Adventure Works" }
                }
            };
            context.Initialize(new[] { account });

            var service = context.GetFakedOrganizationService();
            var results = ExecuteQueryAll(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            results = ExecuteQueryWithExplicitColumnsAll(memoryCache, service);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].GetAttributeValue<string>("name"), Is.EqualTo("Adventure Works"));

            A.CallTo(() => service.RetrieveMultiple(A<QueryExpression>.Ignored)).MustHaveHappened(Repeated.Exactly.Twice);
        }
    }
}
