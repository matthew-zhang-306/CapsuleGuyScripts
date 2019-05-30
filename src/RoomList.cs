using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("RoomList")]
public class RoomList
{
    [XmlArray("Rooms"), XmlArrayItem("Room")]
    public List<Room> rooms;
    public Room FirstRoom { get { return rooms.Count > 0 ? rooms[0] : null; }}

    public static RoomList Load(string path) {
        TextAsset file = Resources.Load<TextAsset>(path);
        StringReader stream = new StringReader(file.text);
        return new XmlSerializer(typeof(RoomList)).Deserialize(stream) as RoomList;
    }
}
