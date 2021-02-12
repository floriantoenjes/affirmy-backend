using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AffirmyBackend.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the AffirmyBackendUser class
    public class AffirmyBackendUser : IdentityUser
    {
        public string UserDatabaseName { get; set; }
    }
}
