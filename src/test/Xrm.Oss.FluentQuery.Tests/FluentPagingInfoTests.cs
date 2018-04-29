using FakeXrmEasy;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Oss.FluentQuery.Tests
{
    [TestFixture]
    public class FluentPagingInfoTests
    {
        [Test]
        public void It_Should_Apply_Paging_Info()
        {
            var context = new XrmFakedContext();
            var service = context.GetFakedOrganizationService();

            var query = service.Query("account")
                .With.PagingInfo(p => p
                    .PageNumber(2)
                    .PageSize(100)
                    .PagingCookie("asdf")
                    .ReturnTotalRecordCount()
                )
                .Expression;

            Assert.That(query.PageInfo, Is.Not.Null);

            Assert.That(query.PageInfo.PageNumber, Is.EqualTo(2));
            Assert.That(query.PageInfo.Count, Is.EqualTo(100));
            Assert.That(query.PageInfo.PagingCookie, Is.EqualTo("asdf"));
            Assert.That(query.PageInfo.ReturnTotalRecordCount, Is.True);
        }
    }
}
