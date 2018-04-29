using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Oss.FluentQuery
{
    public static class IOrganizationServiceFluentQuery
    {
        public static IFluentQuery<T> Query<T>(this IOrganizationService service, string entityName) where T : Entity
        {
            return new FluentQuery<T>(entityName, service);
        }

        public static IFluentQuery<Entity> Query(this IOrganizationService service, string entityName)
        {
            return new FluentQuery<Entity>(entityName, service);
        }
    }

    public interface IFluentQuery<T> where T : Entity
    {
        IFluentQuery<T> IncludeColumns(params string[] columns);
        QueryExpression Expression { get; }
        List<T> Retrieve();
        List<T> RetrieveAll();

        IFluentQuerySetting<T> With { get; }

        IFluentQuery<T> Link(Action<IFluentLinkEntity> definition);
        IFluentQuery<T> Where(Action<IFluentFilterExpression> definition);
        IFluentQuery<T> Order(Action<IFluentOrderExpression> definition);
    }

    public interface IFluentQuerySetting<T> where T : Entity
    {
        IFluentQuery<T> RecordCount(int? topCount);
        IFluentQuery<T> DatabaseLock(bool useLock = true);
        IFluentQuery<T> UniqueRecords(bool unique = true);
        IFluentQuery<T> PagingInfo(Action<IFluentPagingInfo> definition);
        IFluentQuery<T> TotalRecordCount(bool returnTotalRecordCount = true);
    }

    public class FluentQuery<T> : IFluentQuery<T>, IFluentQuerySetting<T> where T : Entity
    {
        private QueryExpression _query;
        private IOrganizationService _service;

        public FluentQuery(string entityName, IOrganizationService service)
        {
            _query = new QueryExpression
            {
                EntityName = entityName,
                NoLock = true
            };

            _service = service;
        }

        public IFluentQuery<T> IncludeColumns(params string[] columns)
        {
            if(_query.ColumnSet == null)
            {
                _query.ColumnSet = new ColumnSet();
            }

            _query.ColumnSet.AddColumns(columns);

            return this;
        }

        public IFluentQuerySetting<T> With
        {
            get
            {
                return this;
            }
        }

        public IFluentQuery<T> RecordCount(int? topCount)
        {
            _query.TopCount = topCount;

            return this;
        }

        public IFluentQuery<T> DatabaseLock(bool useLock = true)
        {
            _query.NoLock = !useLock;

            return this;
        }

        public List<T> Retrieve()
        {
            return _service.RetrieveMultiple(_query).Entities.Select(e => e.ToEntity<T>())
                .ToList();
        }

        public List<T> RetrieveAll()
        {
            var records = new List<T>();

            var previousPageNumber = _query.PageInfo.PageNumber;
            var previousPagingCookie = _query.PageInfo.PagingCookie;

            var moreRecords = false;
            var pageNumber = previousPageNumber;
            string pagingCookie = previousPagingCookie;

            do
            {
                _query.PageInfo.PageNumber = pageNumber;
                _query.PageInfo.PagingCookie = pagingCookie;

                var response = _service.RetrieveMultiple(_query);
                var result = response.Entities.Select(e => e.ToEntity<T>())
                    .ToList();

                records.AddRange(result);

                moreRecords = response.MoreRecords;
                pageNumber++;
            }
            while (moreRecords);

            _query.PageInfo.PageNumber = previousPageNumber;
            _query.PageInfo.PagingCookie = previousPagingCookie;

            return records;
        }

        public IFluentQuery<T> UniqueRecords(bool unique = true)
        {
            _query.Distinct = true;

            return this;
        }

        public IFluentQuery<T> PagingInfo(PagingInfo PagingInfo)
        {
            _query.PageInfo = PagingInfo;

            return this;
        }

        public IFluentQuery<T> Link(Action<IFluentLinkEntity> definition)
        {
            var link = new FluentLinkEntity();

            definition(link);

            _query.LinkEntities.Add(link.GetLinkEntity());

            return this;
        }

        public IFluentQuery<T> Where(Action<IFluentFilterExpression> definition)
        {
            var filter = new FluentFilterExpression();

            definition(filter);

            _query.Criteria = filter.GetFilter();

            return this;
        }

        public IFluentQuery<T> Order(Action<IFluentOrderExpression> definition)
        {
            var order = new FluentOrderExpression();

            definition(order);
            
            _query.Orders.Add(order.GetOrder());

            return this;
        }

        public IFluentQuery<T> PagingInfo(Action<IFluentPagingInfo> definition)
        {
            var PagingInfo = new FluentPagingInfo();

            definition(PagingInfo);

            _query.PageInfo = PagingInfo.GetPagingInfo();

            return this;
        }

        public IFluentQuery<T> TotalRecordCount(bool returnTotalRecordCount = true)
        {
            if (_query.PageInfo == null)
            {
                _query.PageInfo = new PagingInfo();
            }

            _query.PageInfo.ReturnTotalRecordCount = true;

            return this;
        }

        public QueryExpression Expression
        {
            get
            {
                return _query;
            }
        }
    }

    public interface IFluentLinkEntity
    {
        IFluentLinkEntity FromEntity(string entityName);
        IFluentLinkEntity FromAttribute(string attributeName);
        IFluentLinkEntity ToEntity(string entityName);
        IFluentLinkEntity ToAttribute(string attributeName);

        IFluentLinkEntity IncludeColumns(params string[] columns);

        IFluentLinkEntitySetting With { get; }

        IFluentLinkEntity Link(Action<IFluentLinkEntity> definition);
        IFluentLinkEntity Where(Action<IFluentFilterExpression> definition);
    }

    public interface IFluentLinkEntitySetting
    {
        IFluentLinkEntity Alias(string name);
        IFluentLinkEntity LinkType(JoinOperator joinOperator);
    }

    public class FluentLinkEntity : IFluentLinkEntity, IFluentLinkEntitySetting
    {
        private LinkEntity _linkEntity;

        public FluentLinkEntity()
        {
            _linkEntity = new LinkEntity
            {
                Columns = new ColumnSet()
            };
        }

        public IFluentLinkEntitySetting With
        {
            get
            {
                return this;
            }
        }

        public IFluentLinkEntity Alias(string name)
        {
            _linkEntity.EntityAlias = name;

            return this;
        }

        public IFluentLinkEntity FromAttribute(string attributeName)
        {
            _linkEntity.LinkFromAttributeName = attributeName;

            return this;
        }

        public IFluentLinkEntity FromEntity(string entityName)
        {
            _linkEntity.LinkFromEntityName = entityName;

            return this;
        }

        public IFluentLinkEntity IncludeColumns(params string[] columns)
        {
            _linkEntity.Columns.AddColumns(columns);

            return this;
        }

        public IFluentLinkEntity Where(Action<IFluentFilterExpression> definition)
        {
            var filter = new FluentFilterExpression();

            definition(filter);

            _linkEntity.LinkCriteria = filter.GetFilter();

            return this;
        }

        public IFluentLinkEntity Link(Action<IFluentLinkEntity> definition)
        {
            var link = new FluentLinkEntity();

            definition(link);

            _linkEntity.LinkEntities.Add(link.GetLinkEntity());

            return this;
        }

        public IFluentLinkEntity LinkType(JoinOperator joinOperator)
        {
            _linkEntity.JoinOperator = joinOperator;

            return this;
        }

        public IFluentLinkEntity ToAttribute(string attributeName)
        {
            _linkEntity.LinkToAttributeName = attributeName;

            return this;
        }

        public IFluentLinkEntity ToEntity(string entityName)
        {
            _linkEntity.LinkToEntityName = entityName;

            return this;
        }

        internal LinkEntity GetLinkEntity()
        {
            return _linkEntity;
        }
    }

    public interface IFluentFilterExpression
    {
        IFluentFilterExpressionSetting With { get; }

        IFluentFilterExpression Attribute(Action<IFluentConditionExpression> definition);
        IFluentFilterExpression Where(Action<IFluentFilterExpression> definition);
    }

    public interface IFluentFilterExpressionSetting
    {
        IFluentFilterExpression Operator(LogicalOperator filterOperator);
    }

    public class FluentFilterExpression : IFluentFilterExpression, IFluentFilterExpressionSetting
    {
        private FilterExpression _filter;

        public FluentFilterExpression()
        {
            _filter = new FilterExpression();
        }

        public IFluentFilterExpressionSetting With
        {
            get
            {
                return this;
            }
        }

        public IFluentFilterExpression Operator(LogicalOperator filterOperator)
        {
            _filter.FilterOperator = filterOperator;

            return this;
        }

        public IFluentFilterExpression Where(Action<IFluentFilterExpression> definition)
        {
            var filter = new FluentFilterExpression();

            definition(filter);

            _filter.Filters.Add(filter.GetFilter());

            return this;
        }

        public IFluentFilterExpression Attribute(Action<IFluentConditionExpression> definition)
        {
            var condition = new FluentConditionExpression();

            definition(condition);

            _filter.Conditions.Add(condition.GetCondition());

            return this;
        }

        internal FilterExpression GetFilter()
        {
            return _filter;
        }
    }

    public interface IFluentConditionExpression
    {
        IFluentConditionExpression Of(string entityName);
        IFluentConditionExpression Named(string attributeName);
        IFluentConditionExpression Is(ConditionOperator conditionOperator);
        IFluentConditionExpression Value(object value);
        IFluentConditionExpression Value(params object[] value);
        IFluentConditionExpression To(object value);
        IFluentConditionExpression To(params object[] value);
    }

    public class FluentConditionExpression : IFluentConditionExpression
    {
        private ConditionExpression _condition;

        public FluentConditionExpression()
        {
            _condition = new ConditionExpression();
        }

        public IFluentConditionExpression Is(ConditionOperator conditionOperator)
        {
            _condition.Operator = conditionOperator;

            return this;
        }

        public IFluentConditionExpression Named(string attributeName)
        {
            _condition.AttributeName = attributeName;

            return this;
        }

        public IFluentConditionExpression Of(string entityName)
        {
            _condition.EntityName = entityName;

            return this;
        }

        public IFluentConditionExpression To(object value)
        {
            return Value(value);
        }

        public IFluentConditionExpression To(params object[] value)
        {
            return Value(value);
        }

        public IFluentConditionExpression Value(object value)
        {
            _condition.Values.Add(value);

            return this;
        }

        public IFluentConditionExpression Value(params object[] value)
        {
            _condition.Values.AddRange(value);

            return this;
        }

        internal ConditionExpression GetCondition ()
        {
            return _condition;
        }
    }

    public interface IFluentOrderExpression
    {
        IFluentOrderExpression By(string attributeName);
        IFluentOrderExpression Ascending();
        IFluentOrderExpression Descending();
    }

    public class FluentOrderExpression : IFluentOrderExpression
    {
        private OrderExpression _order;

        public FluentOrderExpression ()
        {
            _order = new OrderExpression();
        }
        
        public IFluentOrderExpression By(string attributeName)
        {
            _order.AttributeName = attributeName;

            return this;
        }

        public IFluentOrderExpression Ascending()
        {
            _order.OrderType = OrderType.Ascending;

            return this;
        }

        public IFluentOrderExpression Descending()
        {
            _order.OrderType = OrderType.Descending;

            return this;
        }

        internal OrderExpression GetOrder()
        {
            return _order;
        }
    }

    public interface IFluentPagingInfo
    {
        IFluentPagingInfo PageNumber(int number);
        IFluentPagingInfo PagingCookie(string pagingCookie);
        IFluentPagingInfo PageSize(int number);
        IFluentPagingInfo ReturnTotalRecordCount(bool returnTotal = true);
    }

    public class FluentPagingInfo : IFluentPagingInfo
    {
        private PagingInfo _pagingInfo;

        public FluentPagingInfo()
        {
            _pagingInfo = new PagingInfo();
        }

        public IFluentPagingInfo PageNumber(int number)
        {
            _pagingInfo.PageNumber = number;

            return this;
        }

        public IFluentPagingInfo PageSize(int number)
        {
            _pagingInfo.Count = number;

            return this;
        }

        public IFluentPagingInfo PagingCookie(string pagingCookie)
        {
            _pagingInfo.PagingCookie = pagingCookie;

            return this;
        }

        public IFluentPagingInfo ReturnTotalRecordCount(bool returnTotal = true)
        {
            _pagingInfo.ReturnTotalRecordCount = returnTotal;

            return this;
        }

        public PagingInfo GetPagingInfo()
        {
            return _pagingInfo;
        }
    }
}
