using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blazor.FileReader;

namespace RapidCMS.Core.Abstractions.Handlers
{
    public interface IFileUploadHandler
    {
        IAsyncEnumerable<string> ValidateFile(IFileInfo fileInfo);

        Task<object> SaveFileAsync(IFileInfo fileInfo, Stream stream);
    }
}
