using ImageMagick;

namespace Ollim.Infrastructure.Interfaces
{
    public interface IImageProfileProcessingService
    {
        void ConstructImageProfile(byte[] avatarBytes, string usernameText, string processedFilePath);
    }
}
