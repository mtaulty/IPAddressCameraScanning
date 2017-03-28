namespace MediaFrameQrProcessing.Processors
{
  using MediaFrameQrProcessing.VideoDeviceFinders;
  using System.Runtime.InteropServices.WindowsRuntime;
  using System.Threading.Tasks;
  using Windows.Devices.Enumeration;
  using Windows.Media.Capture;
  using Windows.Media.Capture.Frames;
  using Windows.Media.Ocr;
  using System;
  using System.Text.RegularExpressions;
  using System.Net;

  public class IPAddressFrameProcessor : MediaCaptureFrameProcessor
  {
    public IPAddress Result { get; private set; }

    public IPAddressFrameProcessor(
      MediaFrameSourceFinder mediaFrameSourceFinder, 
      DeviceInformation videoDeviceInformation, 
      string mediaEncodingSubtype, 
      MediaCaptureMemoryPreference memoryPreference = MediaCaptureMemoryPreference.Cpu) 

      : base(
          mediaFrameSourceFinder, 
          videoDeviceInformation, 
          mediaEncodingSubtype, 
          memoryPreference)
    {
    }
    protected override async Task<bool> ProcessFrameAsync(MediaFrameReference frameReference)
    {
      bool done = false;

      // doc here https://msdn.microsoft.com/en-us/library/windows/apps/xaml/windows.media.capture.frames.videomediaframe.aspx
      // says to dispose this softwarebitmap if you access it.
      using (var bitmap = frameReference.VideoMediaFrame.SoftwareBitmap)
      {
        try
        {
          if (this.ocrEngine == null)
          {
            this.ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            this.regex = new Regex(IP_ADDRESS_PATTERN);
          }
          var results = await this.ocrEngine.RecognizeAsync(bitmap);

          if (results != null)
          {
            var matchingResults = this.regex.Matches(results.Text);

            for (int i = 0; !done && (i < matchingResults.Count); i++)
            {
              IPAddress parsedAddress;

              done = IPAddress.TryParse(matchingResults[i].Value, out parsedAddress);

              if (done)
              {
                this.Result = parsedAddress;
              }
            }
          }
        }
        catch
        {
        }
      }
      return (done);
    }
    Regex regex;
    OcrEngine ocrEngine;

    // Taken from S.O. http://stackoverflow.com/questions/106179/regular-expression-to-match-dns-hostname-or-ip-address
    const string IP_ADDRESS_PATTERN =
      @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}";
  }
}