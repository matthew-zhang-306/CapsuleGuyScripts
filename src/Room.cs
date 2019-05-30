using System.Xml.Serialization;

public class Room
{
    [XmlAttribute("name")]
    public string name;
    [XmlAttribute("displayName")]
    public string displayName;
    [XmlIgnore]
    public string previousRoom;
    [XmlElement("Next")]
    public string nextRoom;
    [XmlElement("Secret")]
    public string secretRoom;

    [XmlAttribute("noLaser")]
    public bool noLaser;
    [XmlAttribute("ignoreInLevelSelect")]
    public bool ignoreInLevelSelect;
    [XmlElement("Group")]
    public string group;
    [XmlElement("DefaultAudio")]
    public string defaultAudio;
    [XmlAttribute("audioOverrides")]
    public bool overrideAudio;

    public Room() {
        // Empty constructor required to read XML from a file - otherwise, this does absolutely nothing
    }

    public Room(string name, string displayName, string nextRoom, string secretRoom) {
        this.name = name;
        this.displayName = displayName;
        this.nextRoom = nextRoom;
        this.secretRoom = secretRoom;
    }
}
