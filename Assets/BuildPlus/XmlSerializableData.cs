// Build+ Unity Extension
// Copyright (c) 2012 Luminary Productions, Inc.
// Please direct any bugs/comments/suggestions to http://luminaryproductions.net

using System.Collections;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Text;

namespace BuildPlus
{
	public interface IXmlSerializableData
	{
		void PostDeserialize();
		void PreSerialize();	
		void OnUnknownElement(object sender, XmlElementEventArgs e);
		void OnUnknownNode(object sender, XmlNodeEventArgs e);
	}
		
	public class XmlSerializableData<T> : IXmlSerializableData where T : new()
	{	
		// These are methods that can be implemented by a class to do fixups to the object before saving
		public virtual void PostDeserialize() {}
		public virtual void PreSerialize() {}
		public virtual void OnUnknownElement(object sender, XmlElementEventArgs e) {}		
		public virtual void OnUnknownNode(object sender, XmlNodeEventArgs e) {}	
			
		public MemoryStream ToXMLStream()
		{
			PreSerialize();
			XmlSerializer serializer = new XmlSerializer(typeof(T));
			MemoryStream stream = new MemoryStream();
	#if true
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;
			using (XmlWriter writer = XmlWriter.Create(stream, settings))
				serializer.Serialize(writer, this);
	#else
			serializer.Serialize(stream, this);
	#endif
			stream.Seek(0, SeekOrigin.Begin);
			return stream;
		}
		
		public string ToXML()
		{
			StreamReader sr = new StreamReader(ToXMLStream());
			return sr.ReadToEnd();
		}
		
		static public T FromXML(string xml)
		{
			// The Encoding class is used, so that no BOM issues occur
			byte[] bytes = Encoding.UTF8.GetBytes(xml);
			T data = FromXML(new MemoryStream(bytes));
			return data;
		}
	
		static public T FromXML(Stream stream)
		{
			stream.Position = 0;
			T data = FromXML(XmlReader.Create(stream));
			return data;
		}
	
		static T FromXML(XmlReader reader)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(T));
			T data = new T();
			IXmlSerializableData iface = data as IXmlSerializableData;
			serializer.UnknownElement += iface.OnUnknownElement;
			serializer.UnknownNode += iface.OnUnknownNode;
			data = (T)serializer.Deserialize(reader);
			((IXmlSerializableData)data).PostDeserialize();
			return data;
		}
	}
}
