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
    public class FluentOrderExpressionTests
    {
        [Test]
        public void It_Should_Add_Simple_Order_Expression()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .Order(o => o
                    .By("name")
                )
                .Expression;

            Assert.That(query.Orders.Count, Is.EqualTo(1));

            Assert.That(query.Orders.Single().AttributeName, Is.EqualTo("name"));
        }

        [Test]
        public void It_Should_Add_Multiple_Order_Expressions()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .Order(o => o
                    .By("name")
                )
                .Order(o => o
                    .By("address1_line1")
                )
                .Expression;

            Assert.That(query.Orders.Count, Is.EqualTo(2));

            Assert.That(query.Orders[0].AttributeName, Is.EqualTo("name"));
            Assert.That(query.Orders[1].AttributeName, Is.EqualTo("address1_line1"));
        }

        [Test]
        public void It_Should_Set_Direction()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .Order(o => o
                    .By("name")
                    .Ascending()
                )
                .Order(o => o
                    .By("address1_line1")
                    .Descending()
                )
                .Expression;

            Assert.That(query.Orders.Count, Is.EqualTo(2));

            Assert.That(query.Orders[0].AttributeName, Is.EqualTo("name"));
            Assert.That(query.Orders[0].OrderType, Is.EqualTo(OrderType.Ascending));

            Assert.That(query.Orders[1].AttributeName, Is.EqualTo("address1_line1"));
            Assert.That(query.Orders[1].OrderType, Is.EqualTo(OrderType.Descending));
        }
    }
}
