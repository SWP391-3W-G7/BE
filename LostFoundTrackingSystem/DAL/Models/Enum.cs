using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public enum LostItemStatus
    {
        Lost = 0,
        Matched = 1,
        Returned = 2,
    }
    public enum FoundItemStatus
    {
        Stored = 0,
        Claimed = 1,
        Returned = 2,
    }
    public enum ClaimStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Returned = 3,
    }
}
