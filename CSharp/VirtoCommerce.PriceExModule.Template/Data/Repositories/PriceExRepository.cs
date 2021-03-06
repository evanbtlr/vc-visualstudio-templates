﻿using System.Data.Entity;
using $safeprojectname$.Models;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace $safeprojectname$.Repositories
{
    public class PriceExRepository : PricingRepositoryImpl
    {
        public PriceExRepository()
            : base()
        {
        }

        public PriceExRepository(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, interceptors)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PriceExEntity>().ToTable("PriceEx").HasKey(x => x.Id).Property(x => x.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}