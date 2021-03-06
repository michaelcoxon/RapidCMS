﻿using System.Threading.Tasks;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Models.Data;

namespace RapidCMS.Core.Abstractions.Services
{
    internal interface IParentService
    {
        Task<IParent?> GetParentAsync(ParentPath? parentPath);
    }
}
