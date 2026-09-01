using System;
using Newtonsoft.Json;
using ProtoBuf;
using System.Collections.Generic;

namespace MMXOnline;

public class XSaveData {
	public XData DataSave;
	
	public string configName;

	public XSaveData() {
		DataSave = new XData();

	}

	public static XSaveData createMatchSettingsFromFile(string fileName) {
		string text = Helpers.ReadFromFile(fileName + ".json");
		if (string.IsNullOrEmpty(text)) {
			return new XSaveData() {
				configName = fileName
			};
		} else {
			try {
				var result = JsonConvert.DeserializeObject<XSaveData>(text);
				result.configName = fileName;
				return result;
			} catch {
				throw new Exception("Your XSaveData.json file is corrupted, or does no longer work with this version. Please delete it and launch the game again.");
			}
		}
	}

	
	private static XSaveData _SaveData;
	public static XSaveData SaveData {
		get {
			if (_SaveData == null) {
				_SaveData = createMatchSettingsFromFile("XSaveData");
			}
			return _SaveData;
		}
	}

	public void saveToFile() {
		string text = JsonConvert.SerializeObject(this, Formatting.Indented);
		Helpers.WriteToFile(configName + ".json", text);
	}
}


[ProtoContract]

	public class XData() {

	[ProtoMember(1)] public bool HighWayStageClearX1;

	// 8 Mavericks
	[ProtoMember(2)] public bool MammothStageClearX1;
	[ProtoMember(3)] public bool KuwangerStageClearX1;
	[ProtoMember(4)] public bool EagleStageClearX1;
	[ProtoMember(5)] public bool PenguinStageClarX1;
	[ProtoMember(6)] public bool MandrilStageClearX1;
	[ProtoMember(7)] public bool ArmadilloStageClearX1;
	[ProtoMember(8)] public bool OctopusStageClearX1;
	[ProtoMember(9)] public bool ChameleonStageClearX1;
	

	// EtraLives

	[ProtoMember(10)] public int extraLives;
	
	[ProtoMember(11)] public bool LightArmorBoots;
	[ProtoMember(12)] public bool LightArmorHelmet;
	[ProtoMember(13)] public bool LightArmorBuster;
	[ProtoMember(14)] public bool LightArmorChest;

	[ProtoMember(15)] public int metals;

	[ProtoMember(16)] public bool recievedZbuster;

	[ProtoMember(17)] public bool hadoukenLearned;
	

	//

	[ProtoMember(25)] public bool FortressStage1ClearX1;
	[ProtoMember(26)] public bool FortressStage2ClearX1;
	[ProtoMember(27)] public bool FortressStage3ClearX1;
	[ProtoMember(28)] public bool FortressStage4ClearX1;

	[ProtoMember(29)] public bool badEndingTriggerX1;
	
	
	

	public static XData getDefaults() {
		return new XData {
			HighWayStageClearX1 = false,

	        MammothStageClearX1 = false,
	        KuwangerStageClearX1 = false,
	        EagleStageClearX1 = false,
	        PenguinStageClarX1 = false,
	        MandrilStageClearX1 = false,
	        ArmadilloStageClearX1 = false,
	        OctopusStageClearX1 = false,
	        ChameleonStageClearX1 = false,
	
	   		extraLives = 0,
	        LightArmorBoots = false,
	        LightArmorHelmet = false,
	        LightArmorBuster = false,
	        LightArmorChest = false,
			metals = 0,
			hadoukenLearned = false,
			recievedZbuster = false,

	        FortressStage1ClearX1 = false,
	        FortressStage2ClearX1 = false,
	        FortressStage3ClearX1 = false,
	        FortressStage4ClearX1 = false,

	        badEndingTriggerX1 = false,
		};
	}
}
