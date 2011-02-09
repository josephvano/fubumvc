﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FubuFastPack.Domain;
using FubuFastPack.Querying;
using FubuLocalization;
using Microsoft.Practices.ServiceLocation;
using System.Linq;
using FubuCore;

namespace FubuFastPack.JqGrid
{
    public abstract class Grid<TEntity, TService> : ISmartGrid where TEntity : DomainEntity
    {
        private readonly GridDefinition<TEntity> _definition = new GridDefinition<TEntity>();

        public GridResults Invoke(IServiceLocator services, GridDataRequest request)
        {
            var runner = services.GetInstance<IGridRunner<TEntity, TService>>();
            var source = BuildSource(runner.Service);

            return runner.RunGrid(_definition, source, request);
        }

        public IEnumerable<FilteredProperty> AllFilteredProperties(IQueryService queryService)
        {
            // Force the enumerable to execute so we don't keep building new FilteredProperty objects
            var properties = _definition.Columns.SelectMany(x => x.FilteredProperties()).ToList();
            properties.Each<FilteredProperty>(x => x.Operators = queryService.FilterOptionsFor<TEntity>(x.Accessor));
            return properties;
        }

        public IGridDefinition Definition
        {
            get { return _definition; }
        }

        protected FilterColumn<TEntity> FilterOn(Expression<Func<TEntity, object>> expression)
        {
            return _definition.AddColumn(new FilterColumn<TEntity>(expression));
        }

        protected GridColumn<TEntity> Show(Expression<Func<TEntity, object>> expression)
        {
            return _definition.Show(expression);
        }

        protected LinkColumn<TEntity> ShowViewLink(Expression<Func<TEntity, object>> expression)
        {
            return _definition.ShowViewLink(expression);
        }

        public GridDefinition<TEntity>.OtherEntityLinkExpression<TOther> ShowViewLinkForOther<TOther>(Expression<Func<TEntity, TOther>> entityProperty) where TOther : DomainEntity
        {
            return _definition.ShowViewLinkForOther(entityProperty);
        }

        public abstract IGridDataSource<TEntity> BuildSource(TService service);
    }
}