using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stencil.Common.Integration
{
    public interface IUploadFiles
    {
        string ConstructUploadUrl(string filePathAndName);
        TemporaryUploadCredentials GenerateTemporaryUploadCredentials();
        string GeneratePreSignedUploadUrl(string verb, string filePathAndName, string contentType = "multipart/form-data");
        string UploadPhoto(Image image, ImageEncoding encoding, Size imageTargetSize, string filePathAndName);
    }
}
