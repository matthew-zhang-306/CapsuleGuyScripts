using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("AudioList")]
public class AudioPaletteList
{
    [XmlArray("Audio"), XmlArrayItem("AudioPalette")]
    public List<AudioPalette> audios;
    public AudioPalette Default { get { return audios[0]; }}

    public static AudioPaletteList Load(string path) {
        TextAsset file = Resources.Load<TextAsset>(path);
        StringReader stream = new StringReader(file.text);
        return new XmlSerializer(typeof(AudioPaletteList)).Deserialize(stream) as AudioPaletteList;
    }
}
