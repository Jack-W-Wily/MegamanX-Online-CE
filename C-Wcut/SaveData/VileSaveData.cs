using System;
using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Generic;

namespace MMXOnline;

public class VileSaveData {
	public VavaData vavaDataSave;
	
	public string configName;

	public VileSaveData() {
		vavaDataSave = new VavaData();

	}

	public static VileSaveData createMatchSettingsFromFile(string fileName) {
		string text = Helpers.ReadFromFile(fileName + ".json");
		if (string.IsNullOrEmpty(text)) {
			return new VileSaveData() {
				configName = fileName
			};
		} else {
			try {
				var result = JsonConvert.DeserializeObject<VileSaveData>(text);
				result.configName = fileName;
				return result;
			} catch {
				throw new Exception("Your VileSaveData.json file is corrupted, or does no longer work with this version. Please delete it and launch the game again.");
			}
		}
	}

	
	private static VileSaveData _vileSaveData;
	public static VileSaveData vileSaveData {
		get {
			if (_vileSaveData == null) {
				_vileSaveData = createMatchSettingsFromFile("VileSaveData");
			}
			return _vileSaveData;
		}
	}

	public void saveToFile() {
		string text = JsonConvert.SerializeObject(this, Formatting.Indented);
		Helpers.WriteToFile(configName + ".json", text);
	}
}


[ProtoContract]

	public class VavaData() {

	[ProtoMember(1)] public bool Tridentline;
	[ProtoMember(2)] public bool Fatboy;
	[ProtoMember(3)] public bool LongshotGizmo;
	[ProtoMember(4)] public bool FireMourain;


	
	

	public static VavaData getDefaults() {
		return new VavaData {
			Tridentline = false,
			Fatboy = false,
			LongshotGizmo = false,
			FireMourain = false,
		};
	}
}
