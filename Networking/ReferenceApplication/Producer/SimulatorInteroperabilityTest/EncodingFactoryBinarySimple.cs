﻿
using System;
using System.ComponentModel.Composition;
using System.Xml;
using UAOOI.Configuration.Networking.Serialization;
using UAOOI.Networking.SemanticData;
using UAOOI.Networking.SemanticData.DataRepository;
using UAOOI.Networking.SemanticData.Encoding;

namespace UAOOI.Networking.ReferenceApplication.Producer.SimulatorInteroperabilityTest
{

  /// <summary>
  /// Class EncodingFactoryBinarySimple - provides <see cref="IEncodingFactory"/> functionality limited to encoding simple data only.
  /// </summary>
  /// <seealso cref="UAOOI.Networking.SemanticData.IEncodingFactory" />
  [Export(ProducerCompositionSettings.EncodingFactoryContract, typeof(IEncodingFactory))]
  public class EncodingFactoryBinarySimple : IEncodingFactory
  {

    #region IEncodingFactory
    /// <summary>
    /// Updates the value converter.
    /// </summary>
    /// <param name="binding">An object responsible to transfer the value between the message and ultimate destination in the repository.</param>
    /// <param name="repositoryGroup">The repository group.</param>
    /// <param name="sourceEncoding">The source encoding.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// repositoryGroup
    /// or
    /// binding
    /// </exception>
    void IEncodingFactory.UpdateValueConverter(IBinding binding, string repositoryGroup, UATypeInfo sourceEncoding)
    {
      if (repositoryGroup != Settings.ProducerConfigurationRepositoryGroup)
        throw new ArgumentOutOfRangeException(nameof(repositoryGroup));
      if (sourceEncoding.BuiltInType != binding.Encoding.BuiltInType)
        throw new ArgumentOutOfRangeException(nameof(binding));
    }
    /// <summary>
    /// Gets the ua decoder.
    /// </summary>
    /// <value>The ua decoder.</value>
    IUAEncoder IEncodingFactory.UAEncoder { get; } = new UABinaryEncoderImplementation();
    /// <summary>
    /// Gets the decoder that provides methods to be used to decode OPC UA Built-in types.
    /// </summary>
    /// <value>The object implementing <see cref="T:UAOOI.Networking.SemanticData.Encoding.IUADecoder" /> interface.</value>
    /// <exception cref="System.NotImplementedException"></exception>
    IUADecoder IEncodingFactory.UADecoder
    {
      get
      {
        throw new NotImplementedException();
      }
    }
    #endregion

    #region private
    /// <summary>
    /// Class UABinaryEncoderImplementation - limited implementation of the <see cref="UABinaryEncoder"/> for the testing purpose only.
    /// </summary>
    private class UABinaryEncoderImplementation : UABinaryEncoder
    {
      public override void Write(IBinaryEncoder encoder, IDataValue value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, IDiagnosticInfo value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, IExpandedNodeId value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, IExtensionObject value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, ILocalizedText value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, INodeId value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, IQualifiedName value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, IStatusCode value)
      {
        throw new NotImplementedException();
      }
      public override void Write(IBinaryEncoder encoder, XmlElement value)
      {
        throw new NotImplementedException();
      }
    }
    #endregion

  }

}