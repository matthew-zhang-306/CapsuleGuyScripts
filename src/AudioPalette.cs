using System.Xml.Serialization;

public class AudioPalette
{
    [XmlAttribute("name")]
    public string name;
    [XmlElement("Event")]
    public string eventName;
    [XmlElement("Intensity")]
    public float intensity;
    [XmlElement("EndParameter")]
    public string endParameter;

    public AudioPalette() {
        // Empty constructor required to read XML from a file - otherwise, this does absolutely nothing
    }
}
