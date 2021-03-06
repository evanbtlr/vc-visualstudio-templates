using System;
using System.Linq.Expressions;
using $safeprojectname$.Models;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;

namespace $safeprojectname$.Services
{
    /// <summary>
    ///  Provide CRUD operations for custom member instances.
    /// </summary>
    public class SupplierMemberService : CommerceMembersServiceImpl
    {
        public SupplierMemberService(Func<ICustomerRepository> repositoryFactory, IDynamicPropertyService dynamicPropertyService, ISecurityService securityService, IEventPublisher eventPublisher, ICommerceService commerceService)
            : base(repositoryFactory, dynamicPropertyService, commerceService, securityService, eventPublisher)
        {
        }


        // By overriding this method you can search by your custom tables and columns
        protected override Expression<Func<MemberDataEntity, bool>> GetQueryPredicate(MembersSearchCriteria criteria)
        {
            var retVal = base.GetQueryPredicate(criteria);
            if (criteria.SearchPhrase != null)
            {
                var predicate = PredicateBuilder.False<MemberDataEntity>();
                predicate = predicate.Or(x => x is SupplierDataEntity && (x as SupplierDataEntity).ContractNumber.Contains(criteria.SearchPhrase));
                retVal = retVal.Or(LinqKit.Extensions.Expand(predicate));
            }
            return retVal;
        }
    }
}
