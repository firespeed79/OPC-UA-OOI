﻿
using System;
using System.Diagnostics;
using UAOOI.SemanticData.DataManagement.Encoding;

namespace UAOOI.SemanticData.DataManagement.MessageHandling
{

  /// <summary>
  /// Class MessageHeader  represent information in the protocol message header.
  /// </summary>
  public abstract class MessageHeader
  {

    #region API
    /// <summary>
    /// Gets the producer message header.
    /// </summary>
    /// <param name="writer">The writer <see cref="IBinaryHeaderEncoder" /> to populate the payload with the header information.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="lengthFieldType">Type of the length field in the the message header.</param>
    /// <returns>MessageHeader.</returns>
    internal static MessageHeader GetProducerMessageHeader(IBinaryHeaderEncoder writer, FieldEncodingEnum encoding, MessageLengthFieldTypeEnum lengthFieldType)
    {
      return new ProducerMessageHeader(writer, encoding, lengthFieldType);
    }
    /// <summary>
    /// Gets the consumer message header.
    /// </summary>
    /// <param name="reader">The reader <see cref="IBinaryDecoder" /> used to read the header data from the message.</param>
    /// <returns>MessageHeader.</returns>
    internal static MessageHeader GetConsumerMessageHeader(IBinaryDecoder reader)
    {
      return new ConsumerMessageHeader(reader);
    }
    /// <summary>
    /// Synchronizes this instance content with the underlying stream using provided <see cref="IBinaryDecoder"/> or <see cref="IBinaryHeaderEncoder"/> depending on the message handler role.
    /// </summary>
    internal abstract void Synchronize();
    #endregion

    #region Header
    /// <summary>
    /// struct ConfigurationVersionDataType - this data type is used to indicate configuration changes in the information send by the message producer.
    /// </summary>
    public struct ConfigurationVersionDataType
    {
      /// <summary>
      /// Gets or sets the major version. 
      /// </summary>
      /// <remarks>
      /// The major number reflects the primary format of the DataSet and must be equal for both producer and consumer.
      /// Removing fields from the DataSet, reordering fields, adding fields in between other fields or a DataType 
      /// change in fields shall result in an update of the MajorVersion. The initial value for the MajorVersion is 0. 
      /// If the MajorVersion is incremented, the MinorVersion shall be set to 0.
      /// An overflow of the MajorVersion is treated like any other major version change and requires a meta data exchange.
      /// </remarks>
      /// <value>The major version.</value>
      public byte MajorVersion { get; set; }
      /// <summary>
      /// Gets or sets the minor version.
      /// </summary>
      /// <remarks>The minor number reflects backward compatible changes of the DataSet like adding a field at the end of the DataSet.
      /// The initial value for the MinorVersion is 0. The MajorVersion shall be incremented after an overflow of the MinorVersion.</remarks>
      /// <value>The minor version.</value>
      public byte MinorVersion { get; set; }
    }
    /// <summary>
    /// Gets or sets the type of the message.
    /// </summary>
    /// <value>The type of the message.</value>
    public abstract MessageTypeEnum MessageType { get; set; }
    /// <summary>
    /// Gets or sets the encoding flags.
    /// </summary>
    /// <value>The encoding flags.</value>
    public abstract byte EncodingFlags { get; }
    /// <summary>
    /// Gets or sets the length of the message.
    /// </summary>
    /// <value>The length of the message data structure including the header information and length field.</value>
    public abstract UInt32 MessageLength { get; }
    /// <summary>
    /// Gets or sets the message sequence number.
    /// </summary>
    /// <remarks>
    /// A receiver shall ignore older messages than the last sequence processed. Receivers need to be aware of sequence numbers roll over.
    /// </remarks>
    /// <value>The message sequence number. A monotonically increasing sequence number assigned by the publisher to each message sent.
    /// </value>
    public abstract UInt16 MessageSequenceNumber { get; set; }
    /// <summary>
    /// Gets or sets the configuration version.
    /// </summary>
    /// <value>The configuration version used as consistency check for the metadata about the published variables.</value>
    public abstract ConfigurationVersionDataType ConfigurationVersion { get; set; }
    /// <summary>
    /// Gets or sets the time stamp of th data contained in the message.
    /// </summary>
    /// <value>The time the Data was collected.</value>
    public abstract DateTime TimeStamp { get; set; }
    /// <summary>
    /// Gets or sets the field count.
    /// </summary>
    /// <value>Number of fields of the DataSet contained in the Message.</value>
    public abstract UInt16 FieldCount { get; set; }
    #endregion

    #region private
    //vars
    private const byte EncodingFlagsMessageLengthMask = 0x3;
    private const byte EncodingFlagsFieldEncodingMask = 0xC;
    //types
    private class ProducerMessageHeader : MessageHeader
    {

      #region creator
      public ProducerMessageHeader(IBinaryHeaderEncoder writer, FieldEncodingEnum encoding, MessageLengthFieldTypeEnum lengthFieldType)
      {
        m_Encoding = encoding;
        m_lengthFieldType = lengthFieldType;
        m_HeaderWriter = new HeaderWriter(writer, PackageHeaderLength());
        MessageSequenceNumber = 0;
      }
      #endregion

      #region MessageHeader
      public override MessageTypeEnum MessageType
      {
        get; set;
      }
      public override byte EncodingFlags
      {
        get
        {
          return (byte)((byte)m_Encoding & (byte)m_lengthFieldType);
        }
      }
      /// <summary>
      /// Gets or sets the length of the message.
      /// </summary>
      /// <value>The length of the message data structure including the header information and length field.</value>
      /// <exception cref="System.ApplicationException">This operation is not applicable for the Producer Message Header</exception>
      public override UInt32 MessageLength
      {
        get { throw new ApplicationException("This operation is not applicable for the Producer Message Header"); }
      }
      public override UInt16 MessageSequenceNumber
      {
        get; set;
      }
      public override ConfigurationVersionDataType ConfigurationVersion
      {
        get; set;
      }
      public override DateTime TimeStamp
      {
        get; set;
      }
      public override ushort FieldCount
      {
        get; set;
      }
      internal override void Synchronize()
      {
        m_HeaderWriter.WriteHeader(WriteHeader);
      }
      #endregion

      #region private
      //vars
      private HeaderWriter m_HeaderWriter;
      FieldEncodingEnum m_Encoding = FieldEncodingEnum.VariantFieldEncoding;
      MessageLengthFieldTypeEnum m_lengthFieldType = MessageLengthFieldTypeEnum.TwoBytes;
      //methods
      private ushort PackageHeaderLength()
      {
        ushort _length = 6;
        switch ((MessageLengthFieldTypeEnum)(EncodingFlags & EncodingFlagsMessageLengthMask))
        {
          case MessageLengthFieldTypeEnum.OneByte:
            _length += 1;
            break;
          case MessageLengthFieldTypeEnum.TwoBytes:
            _length += 2;
            break;
          case MessageLengthFieldTypeEnum.FourBytes:
            _length += 4;
            break;
        }
        switch (MessageType)
        {
          case MessageTypeEnum.DataKeyFrame:
          case MessageTypeEnum.DataDeltaFrame:
          case MessageTypeEnum.Event:
            _length += 10;
            break;
          case MessageTypeEnum.KeepAlive:
            break;
          case MessageTypeEnum.DataSetMetadata:
            break;
          default:
            break;
        }
        return _length;
      }
      private void WriteHeader(IBinaryHeaderEncoder writer, ushort messageLength)
      {
        writer.Write((byte)MessageType);
        writer.Write(EncodingFlags);
        switch (m_lengthFieldType)
        {
          case MessageLengthFieldTypeEnum.OneByte:
            writer.Write(Convert.ToByte(messageLength));
            break;
          case MessageLengthFieldTypeEnum.TwoBytes:
            writer.Write(Convert.ToUInt16(messageLength));
            break;
          case MessageLengthFieldTypeEnum.FourBytes:
            writer.Write(Convert.ToUInt32(messageLength));
            break;
        }
        writer.Write(MessageSequenceNumber);
        writer.Write(ConfigurationVersion.MajorVersion);
        writer.Write(ConfigurationVersion.MinorVersion);
        switch (MessageType)
        {
          case MessageTypeEnum.DataKeyFrame:
          case MessageTypeEnum.DataDeltaFrame:
          case MessageTypeEnum.Event:
            writer.Write(TimeStamp);
            writer.Write(FieldCount);
            break;
          case MessageTypeEnum.KeepAlive:
            break;
          case MessageTypeEnum.DataSetMetadata:
            break;
          default:
            break;
        }
      }
      #endregion

    }
    private class ConsumerMessageHeader : MessageHeader
    {

      #region creator
      public ConsumerMessageHeader(IBinaryDecoder reader)
      {
        m_reader = reader;
      }
      #endregion

      #region MessageHeader
      public override MessageTypeEnum MessageType
      {
        get
        {
          AssertSynchronized();
          return m_MessageType;
        }
        set { throw new ApplicationException(m_OperationIsNotApplicableMessage); }
      }
      public override byte EncodingFlags
      {
        get
        {
          AssertSynchronized();
          return m_EncodingFlags;
        }
      }
      public override UInt32 MessageLength
      {
        get
        {
          AssertSynchronized();
          return m_MessageLength;
        }
      }
      public override ushort MessageSequenceNumber
      {
        get
        {
          AssertSynchronized();
          return m_MessageSequenceNumber;
        }
        set { throw new ApplicationException(m_OperationIsNotApplicableMessage); }
      }
      public override ConfigurationVersionDataType ConfigurationVersion
      {
        get
        {
          AssertSynchronized();
          return m_ConfigurationVersion;
        }
        set { throw new ApplicationException(m_OperationIsNotApplicableMessage); }
      }
      public override DateTime TimeStamp
      {
        get
        {
          AssertSynchronized();
          return m_TimeStamp;
        }
        set { throw new ApplicationException(m_OperationIsNotApplicableMessage); }
      }
      public override ushort FieldCount
      {
        get
        {
          AssertSynchronized();
          return m_FieldCount;
        }
        set { throw new ApplicationException(m_OperationIsNotApplicableMessage); }
      }
      internal override void Synchronize()
      {
        m_MessageType = (MessageTypeEnum)m_reader.ReadByte();
        m_EncodingFlags = m_reader.ReadByte();
        switch ((MessageLengthFieldTypeEnum)(m_EncodingFlags & EncodingFlagsMessageLengthMask))
        {
          case MessageLengthFieldTypeEnum.OneByte:
            m_MessageLength = m_reader.ReadByte();
            break;
          case MessageLengthFieldTypeEnum.TwoBytes:
            m_MessageLength = m_reader.ReadUInt16();
            break;
          case MessageLengthFieldTypeEnum.FourBytes:
            m_MessageLength = m_reader.ReadUInt32();
            break;
        }
        m_MessageSequenceNumber = m_reader.ReadUInt16();
        m_ConfigurationVersion.MajorVersion = m_reader.ReadByte();
        m_ConfigurationVersion.MinorVersion = m_reader.ReadByte();
        switch (m_MessageType)
        {
          case MessageTypeEnum.DataKeyFrame:
          case MessageTypeEnum.DataDeltaFrame:
          case MessageTypeEnum.Event:
            m_TimeStamp = m_reader.ReadDateTime();
            m_FieldCount = m_reader.ReadUInt16();
            break;
          case MessageTypeEnum.KeepAlive:
            break;
          case MessageTypeEnum.DataSetMetadata:
            break;
          default:
            break;
        }
        m_IsSynchronized = true;
      }
      #endregion

      #region private
      //vars
      private const string m_OperationIsNotApplicableMessage = "This operation is not applicable for the consumer message header";
      private bool m_IsSynchronized = false;
      private IBinaryDecoder m_reader;
      private UInt32 m_MessageLength;
      private byte m_EncodingFlags;
      private ushort m_MessageSequenceNumber;
      private ConfigurationVersionDataType m_ConfigurationVersion = new ConfigurationVersionDataType() { MajorVersion = 0, MinorVersion = 0 };
      private DateTime m_TimeStamp;
      private MessageTypeEnum m_MessageType;
      private ushort m_FieldCount;

      //methods
      [Conditional("DEBUG")]
      private void AssertSynchronized()
      {
        Debug.Assert(m_IsSynchronized, "Producer message must be synchronized with the underlying stream before the header fields will be available.");
      }
      #endregion

    }
    private MessageHeader() { }
    #endregion

  }

}
