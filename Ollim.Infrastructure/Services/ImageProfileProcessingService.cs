using ImageMagick;
using ImageMagick.Drawing;
using Ollim.Infrastructure.Interfaces;

namespace Ollim.Infrastructure.Services
{
    public class ImageProfileProcessingService : IImageProfileProcessingService
    {
        public void ConstructImageProfile(byte[] avatarBytes, string usernameText, string processedFilePath)
        {
            using (var avatarImage = new MagickImage(avatarBytes))
            {
                avatarImage.Resize(270, 270);

                var bgContainer = new MagickImage(new MagickColor(0, 0, 0), 1537, 709);

                var username = new MagickImage(new MagickColor(MagickColors.None), bgContainer.Width, bgContainer.Height);

                var usernameDraw = new Drawables()
                    .FontPointSize(64)
                    .Font("Arial")
                    .StrokeColor(new MagickColor("white"))
                    .FillColor(new MagickColor("white"))
                    .Text(70, 140, usernameText);

                usernameDraw.Draw(username);

                bgContainer.Composite(username, CompositeOperator.Over);

                int avatarX = 70;
                int avatarY = 170;

                bgContainer.Composite(avatarImage, avatarX, avatarY, CompositeOperator.Over);

                ProcessImage(bgContainer, processedFilePath);
            }
        }

        private void ProcessImage(MagickImage image, string processedFilePath)
        {
            image.Write(processedFilePath);
        }

    }
}
