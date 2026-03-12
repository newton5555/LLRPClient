

using System;
using System.IO;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class for exporting the reader LLRP configuration to file for debug
  /// purposes.
  /// </summary>
  public class LlrpReaderDebug
  {
    private LlrpReader parent;

    /// <summary>Creates an instance of the LLRPReaderDebug class.</summary>
    /// <param name="parent"></param>
    public LlrpReaderDebug(LlrpReader parent) => this.parent = parent;

    /// <summary>Saves the reader LLRP configuration to a text file.</summary>
    /// <param name="path">
    /// The path and filename to save the LLRP configuration data to.
    /// </param>
    public void SaveLlrpConfigToFile(string path)
    {
      this.StringToFile("" + "///////////////////////////////////////////////////////\n" + "// ROSpec\n" + "///////////////////////////////////////////////////////\n" + this.parent.GetRoSpecs().ToString() + "\n\n\n" + "///////////////////////////////////////////////////////\n" + "// Reader Configuration\n" + "///////////////////////////////////////////////////////\n" + this.parent.GetReaderConfig().ToString() + "\n\n\n" + "///////////////////////////////////////////////////////\n" + "// Reader Capabilities\n" + "///////////////////////////////////////////////////////\n" + this.parent.GetReaderCapabilities().ToString(), path);
    }

    private void StringToFile(string contents, string path)
    {
      try
      {
        StreamWriter streamWriter = new StreamWriter(path);
        streamWriter.Write(contents);
        streamWriter.Close();
      }
      catch (Exception ex)
      {
        throw new LLRPSdkException("Error creating file: " + ex.Message);
      }
    }
  }
}
