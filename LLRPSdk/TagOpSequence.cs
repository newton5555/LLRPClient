
using System.Collections.Generic;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Class for containing and configuring a sequence of tag operations
  /// that are submitted to the reader using the
  /// <see cref="M:LLRPSdk.LLRPReader.AddOpSequence(LLRPSdk.TagOpSequence)" /> method.
  /// </summary>
  public class TagOpSequence
  {
    private static uint LastId;
    /// <summary>
    /// Definition of trigger that will cause this operating sequence to stop.
    /// </summary>
    public SequenceTriggerType SequenceStopTrigger;
    /// <summary>
    /// The unique ID for this sequence. A unique ID is automatically assigned
    /// by the class constructor.
    /// </summary>
    public uint Id;
    /// <summary>
    /// The Antenna ID to which this sequence will be applied.
    /// Antennas start at antenna 1, and for all antennas, use AntennasIds.All.
    /// </summary>
    public ushort AntennaId;
    /// <summary>
    /// Identifies whether the sequence is disabled or active.
    /// </summary>
    public SequenceState State;
    /// <summary>
    /// When the SequenceStopTrigger member variable is set to ExecutionCount,
    /// this parameter identifies the number of times this sequence of
    /// operations will execute before it gets deleted.
    /// A value of zero (0) means that the sequence never gets deleted.
    /// </summary>
    public ushort ExecutionCount;
    /// <summary>
    /// Identifies the target tag(s) on which to perform this tag operation
    /// sequence.
    /// </summary>
    public TargetTag TargetTag = new TargetTag();
    /// <summary>The list of tag operations to perform.</summary>
    public List<TagOp> Ops = new List<TagOp>();
    /// <summary>Flag whether BlockWrites should be used, or not.</summary>
    public bool BlockWriteEnabled;
    /// <summary>
    /// Specify whether 16 or 32-bit BlockWrites should be used.
    /// </summary>
    public ushort BlockWriteWordCount;//无意义，
    /// <summary>
    /// Specify how many retries to use for BlockWrite operations.
    /// </summary>
    public ushort BlockWriteRetryCount;//无意义

    /// <summary>
    /// Default constructor that applies defaults to all member variables.
    /// </summary>
    public TagOpSequence() => this.SetDefaults();

    private void SetDefaults()
    {
      this.AutoAssignId();
      this.SequenceStopTrigger = SequenceTriggerType.ExecutionCount;
      this.ExecutionCount = (ushort) 1;
      this.BlockWriteEnabled = false;
      this.State = SequenceState.Active;
    }

    private void AutoAssignId()
    {
      lock (this)
      {
        if (TagOpSequence.LastId == uint.MaxValue)
          TagOpSequence.LastId = 1U;
        else
          ++TagOpSequence.LastId;
        this.Id = TagOpSequence.LastId;
      }
    }
  }
}
