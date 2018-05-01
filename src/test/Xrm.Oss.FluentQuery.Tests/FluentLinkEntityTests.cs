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
    public class FluentLinkEntityTests
    {
        [Test]
        public void It_Should_Add_Link_Entity()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var entityAlias = "accountContacts";
            var fromEntity = "account";
            var fromAttribute = "accountid";
            var toEntity = "contact";
            var toAttribute = "contactid";

            var query = service.Query("account")
                .Link(l => l
                    .With.Alias(entityAlias)
                    .FromEntity(fromEntity)
                    .FromAttribute(fromAttribute)
                    .ToEntity(toEntity)
                    .ToAttribute(toAttribute)
                )
                .Expression;

            Assert.That(query.LinkEntities.Count, Is.EqualTo(1));

            Assert.That(query.LinkEntities.Single().EntityAlias, Is.EqualTo(entityAlias));
            Assert.That(query.LinkEntities.Single().LinkFromEntityName, Is.EqualTo(fromEntity));
            Assert.That(query.LinkEntities.Single().LinkFromAttributeName, Is.EqualTo(fromAttribute));
            Assert.That(query.LinkEntities.Single().LinkToEntityName, Is.EqualTo(toEntity));
            Assert.That(query.LinkEntities.Single().LinkToAttributeName, Is.EqualTo(toAttribute));
        }

        [Test]
        public void It_Should_Add_Nested_Link_Entity()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var entityAlias = "accountContacts";
            var fromEntity = "account";
            var fromAttribute = "accountid";
            var toEntity = "contact";
            var toAttribute = "contactid";

            var entityAlias2 = "accountContacts2";
            var fromEntity2 = "account2";
            var fromAttribute2 = "accountid2";
            var toEntity2 = "contact2";
            var toAttribute2 = "contactid2";

            var query = service.Query("account")
                .Link(l => l
                    .With.Alias(entityAlias)
                    .FromEntity(fromEntity)
                    .FromAttribute(fromAttribute)
                    .ToEntity(toEntity)
                    .ToAttribute(toAttribute)
                    .Link(l2 => l2
                        .With.Alias(entityAlias2)
                        .FromEntity(fromEntity2)
                        .FromAttribute(fromAttribute2)
                        .ToEntity(toEntity2)
                        .ToAttribute(toAttribute2)
                    )
                )
                .Expression;

            Assert.That(query.LinkEntities.Count, Is.EqualTo(1));

            Assert.That(query.LinkEntities.Single().EntityAlias, Is.EqualTo(entityAlias));
            Assert.That(query.LinkEntities.Single().LinkFromEntityName, Is.EqualTo(fromEntity));
            Assert.That(query.LinkEntities.Single().LinkFromAttributeName, Is.EqualTo(fromAttribute));
            Assert.That(query.LinkEntities.Single().LinkToEntityName, Is.EqualTo(toEntity));
            Assert.That(query.LinkEntities.Single().LinkToAttributeName, Is.EqualTo(toAttribute));

            var innerLink = query.LinkEntities.Single().LinkEntities.Single();

            Assert.That(innerLink.EntityAlias, Is.EqualTo(entityAlias2));
            Assert.That(innerLink.LinkFromEntityName, Is.EqualTo(fromEntity2));
            Assert.That(innerLink.LinkFromAttributeName, Is.EqualTo(fromAttribute2));
            Assert.That(innerLink.LinkToEntityName, Is.EqualTo(toEntity2));
            Assert.That(innerLink.LinkToAttributeName, Is.EqualTo(toAttribute2));
        }

        [Test]
        public void It_Should_Set_Link_Type()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .Link(l => l
                    .With.LinkType(JoinOperator.Inner)
                )
                .Expression;

            Assert.That(query.LinkEntities.Count, Is.EqualTo(1));

            Assert.That(query.LinkEntities.Single().JoinOperator, Is.EqualTo(JoinOperator.Inner));
        }

        [Test]
        public void It_Should_Set_Columns()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .Link(l => l
                    .IncludeColumns("name", "address1_line1")
                )
                .Expression;

            Assert.That(query.LinkEntities.Count, Is.EqualTo(1));

            Assert.That(query.LinkEntities.Single().Columns.Columns.ToArray, Is.EquivalentTo(new[] { "name", "address1_line1" }));
        }

        [Test]
        public void It_Should_Set_Filter()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();
            
            var entityAlias = "accountContacts";
            var fromEntity = "account";
            var fromAttribute = "accountid";
            var toEntity = "contact";
            var toAttribute = "contactid";

            var query = service.Query("account")
                .Link(l => l
                    .With.Alias(entityAlias)
                    .FromEntity(fromEntity)
                    .FromAttribute(fromAttribute)
                    .ToEntity(toEntity)
                    .ToAttribute(toAttribute)
                    .Where(e => e
                        .Attribute(a => a
                            .Named("emailaddress1")
                            .Is(ConditionOperator.NotNull)
                        )
                    )
                )
                .Expression;

            Assert.That(query.LinkEntities.Count, Is.EqualTo(1));

            Assert.That(query.LinkEntities.Single().LinkCriteria.Conditions.Count, Is.EqualTo(1));

            var link = query.LinkEntities.Single();
            Assert.That(link.LinkCriteria.Conditions[0].AttributeName, Is.EqualTo("emailaddress1"));
            Assert.That(link.LinkCriteria.Conditions[0].Operator, Is.EqualTo(ConditionOperator.NotNull));
        }
    }
}
