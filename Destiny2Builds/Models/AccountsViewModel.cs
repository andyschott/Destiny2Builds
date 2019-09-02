using System.Collections.Generic;
using Destiny2Builds.Helpers;

namespace Destiny2Builds.Models
{
    public class AccountsViewModel
    {
        public IEnumerable<Account> Accounts { get; set; }

        public string GetAccountName(Account account)
        {
            return Utilities.GetDescription(account.Type);
        }
    }
}