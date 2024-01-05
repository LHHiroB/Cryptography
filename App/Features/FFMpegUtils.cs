using FFMpegCore;

namespace IOApp.Features
{
    public static class FFMpegUtils
    {
        public static bool IsReadbleMediaFile(string filePath)
        {
            try
            {
                return FFProbe.Analyse(filePath) != null;
            }
            catch
            {
                return false;
            }
        }

        public static IMediaAnalysis GetMediaAnalysis(string filePath)
        {
            try
            {
                var result =  FFProbe.Analyse(filePath);
                if (result?.PrimaryVideoStream == null && result?.PrimaryAudioStream == null && result?.PrimarySubtitleStream == null)
                    return null;
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}