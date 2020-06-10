using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blazor.FileReader;
using RapidCMS.Core.Extensions;
using RapidCMS.Core.Handlers;

namespace RapidCMS.Core.Abstractions.Handlers
{
    public interface IFileUploadHandler
    {
        Task<IEnumerable<string>> ValidateFileAsync(IFileInfo fileInfo);

        Task<object> SaveFileAsync(IFileInfo fileInfo, Stream stream);

        public static string GetFileUploaderAlias(Type handlerType)
        {
            var type = (handlerType.GetGenericTypeDefinition() == typeof(ApiFileUploadHandler<>))
                ? handlerType.GetGenericArguments().FirstOrDefault()
                : handlerType;

            Console.WriteLine(type);
            return type?.Name.ToUrlFriendlyString() ?? "unknown-file-handler";
        }
    }
}
