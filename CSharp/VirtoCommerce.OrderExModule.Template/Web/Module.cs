﻿using System;
using $ext_safeprojectname$.Core.Models.Cart;
using $ext_safeprojectname$.Core.Models.Order;
using $ext_safeprojectname$.Data.Models.Cart;
using $ext_safeprojectname$.Data.Models.Order;
using $ext_safeprojectname$.Data.Repositories;
using $ext_safeprojectname$.Data.Services;
using Microsoft.Practices.Unity;
using VirtoCommerce.CartModule.Data.Model;
using VirtoCommerce.CartModule.Data.Repositories;
using VirtoCommerce.Domain.Cart.Model;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.OrderModule.Data.Model;
using VirtoCommerce.OrderModule.Data.Repositories;
using VirtoCommerce.OrderModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.Platform.Data.Repositories;

namespace $safeprojectname$
{
    using CartConfiguration = Data.Migrations.Cart.Configuration;
    using CartLineItem = VirtoCommerce.Domain.Cart.Model.LineItem;
    using LineItemEntity = VirtoCommerce.CartModule.Data.Model.LineItemEntity;
    using OrderConfiguration = Data.Migrations.Order.Configuration;
    using OrderLineItem = VirtoCommerce.Domain.Order.Model.LineItem;

    public class Module : ModuleBase
    {
        private readonly string _connectionString = ConfigurationHelper.GetConnectionStringValue("VirtoCommerce.Cart") ?? ConfigurationHelper.GetConnectionStringValue("VirtoCommerce");
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public override void SetupDatabase()
        {
            base.SetupDatabase();

            using (var db = new CartExRepository(_connectionString, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<CartExRepository, CartConfiguration>();
                initializer.InitializeDatabase(db);
            }

            using (var db = new OrderExRepository(_connectionString, _container.Resolve<AuditableInterceptor>()))
            {
                var initializer = new SetupDatabaseInitializer<OrderExRepository, OrderConfiguration>();
                initializer.InitializeDatabase(db);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Func<ICartRepository> cartRepFactory = () =>
                   new CartExRepository(_connectionString, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>(),
                       new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { nameof(CartExEntity), nameof(LineItemExEntity) }));
            _container.RegisterInstance(instance: cartRepFactory);

            Func<IOrderRepository> orderRepFactory = () =>
                new OrderExRepository(_connectionString, new EntityPrimaryKeyGeneratorInterceptor(), _container.Resolve<AuditableInterceptor>(),
                    new ChangeLogInterceptor(_container.Resolve<Func<IPlatformRepository>>(), ChangeLogPolicy.Cumulative, new[] { nameof(CustomerOrderExEntity), nameof(InvoiceEntity) }));
            _container.RegisterInstance(instance: orderRepFactory);

            _container.RegisterType<ICartRepository>(new InjectionFactory(c => new CartExRepository(_connectionString, _container.Resolve<AuditableInterceptor>(), new EntityPrimaryKeyGeneratorInterceptor())));
            _container.RegisterType<IOrderRepository>(new InjectionFactory(c => new OrderExRepository(_connectionString, _container.Resolve<AuditableInterceptor>(), new EntityPrimaryKeyGeneratorInterceptor())));

            // Override ICustomerOrderBuilder default implementation
            _container.RegisterType<ICustomerOrderBuilder, CustomerOrderBuilderExService>();
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

            AbstractTypeFactory<ShoppingCart>.OverrideType<ShoppingCart, CartEx>();
            AbstractTypeFactory<ShoppingCartEntity>.OverrideType<ShoppingCartEntity, CartExEntity>();
            AbstractTypeFactory<CartLineItem>.OverrideType<CartLineItem, Core.Models.Cart.LineItemEx>();
            AbstractTypeFactory<LineItemEntity>.OverrideType<LineItemEntity, LineItemExEntity>();

            AbstractTypeFactory<IOperation>.OverrideType<CustomerOrder, CustomerOrderEx>();
            AbstractTypeFactory<CustomerOrderEntity>.OverrideType<CustomerOrderEntity, CustomerOrderExEntity>();
            AbstractTypeFactory<CustomerOrder>.OverrideType<CustomerOrder, CustomerOrderEx>().WithFactory(() => new CustomerOrderEx { OperationType = "CustomerOrder" });
            AbstractTypeFactory<OrderLineItem>.OverrideType<OrderLineItem, Core.Models.Order.LineItemEx>();

            // Thats need for PolymorphicOperationJsonConverter for API deserialization
            AbstractTypeFactory<IOperation>.RegisterType<Invoice>();
        }
    }
}
