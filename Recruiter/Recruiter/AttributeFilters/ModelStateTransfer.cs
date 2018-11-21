using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recruiter.AttributeFilters
{
    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        protected const string Key = nameof(ModelStateTransfer);
    }
}
