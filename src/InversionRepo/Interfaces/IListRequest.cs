using System;
using System.Collections.Generic;
using System.Text;

namespace InversionRepo.Interfaces
{
    public interface IListRequest
    {
        int? PageSize { get; set; }
        int? PageNumber { get; set; }
        string SortField { get; set; }
        bool SortOrderAscending { get; set; }
    }
}
