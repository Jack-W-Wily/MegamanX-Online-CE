using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMXOnline;

public enum NetCharBoolStateNum {
	One,
	Two,
	Three
}

public class NetCharBoolState {
	private Character character;
	private int byteIndex;
	private Func<Character, bool> getBSValue;
	public NetCharBoolStateNum netCharStateNum;

	public NetCharBoolState(Character character, int byteIndex, NetCharBoolStateNum netCharStateNum, Func<Character, bool> getBSValue) {
		this.character = character;
		this.byteIndex = byteIndex;
		this.getBSValue = getBSValue;
		this.netCharStateNum = netCharStateNum;
	}

	public bool getValue() {
		if (character.ownedByLocalPlayer) {
			return getBSValue(character);
		}
		if (netCharStateNum == NetCharBoolStateNum.One) {
			return Helpers.getByteValue(character.netCharState1, byteIndex);
		} 
		if (netCharStateNum == NetCharBoolStateNum.Two) {
			return Helpers.getByteValue(character.netCharState2, byteIndex);
		} 
		if (netCharStateNum == NetCharBoolStateNum.Three) {
			return Helpers.getByteValue(character.netCharState3, byteIndex);
		} 
		return false;
	}

	public void updateValue() {
		if (netCharStateNum == NetCharBoolStateNum.One) {
			Helpers.setByteValue(ref character.netCharState1, byteIndex, getValue());
		} 
		if (netCharStateNum == NetCharBoolStateNum.Two) {
			Helpers.setByteValue(ref character.netCharState2, byteIndex, getValue());
		}
		if (netCharStateNum == NetCharBoolStateNum.Three) {
			Helpers.setByteValue(ref character.netCharState3, byteIndex, getValue());
		}
	}
}

public partial class Character {
	// NET CHAR STATE 1 SECTION
	public byte netCharState1;

	public NetCharBoolState isFrozenCastleActiveBS;
	public NetCharBoolState isStrikeChainHookedBS;
	public NetCharBoolState shouldDrawArmBS;
	public NetCharBoolState isAwakenedZeroBS;
	public NetCharBoolState isAwakenedGenmuZeroBS;
	public NetCharBoolState isInvisibleBS;
	public NetCharBoolState isReturnIXBS;
	public NetCharBoolState isHyperSigmaBS;

	public void initNetCharState1() {
		isFrozenCastleActiveBS = new NetCharBoolState(this, 0, NetCharBoolStateNum.One, (character) => {
			if (character is not Vile vile) {
				return false;
			}
			return vile.hasFrozenCastleBarrier(); 
		});
		isStrikeChainHookedBS = new NetCharBoolState(this, 1, NetCharBoolStateNum.One, (character) => { return character.charState is StrikeChainHooked; });
		shouldDrawArmBS = new NetCharBoolState(this, 2, NetCharBoolStateNum.One, (character) => {
			return (character as Axl)?.shouldDrawArm() == true; 
		});
		isAwakenedZeroBS = new NetCharBoolState(this, 3, NetCharBoolStateNum.One, (character) => {
			return (character as Zero)?.isAwakenedZero() == true;
		});
		isAwakenedGenmuZeroBS = new NetCharBoolState(this, 4, NetCharBoolStateNum.One, (character) => {
			return (character as Zero)?.isAwakenedGenmuZero() == true;
		});
		isInvisibleBS = new NetCharBoolState(this, 5, NetCharBoolStateNum.One, (character) => { return character.isInvisible(); });
		isReturnIXBS = new NetCharBoolState(this, 6, NetCharBoolStateNum.One, (character) => {
			return (character as MegamanX)?.isReturnIX == true;
		});
		isHyperSigmaBS = new NetCharBoolState(this, 7, NetCharBoolStateNum.One, (character) => { 
			if (character is KaiserSigma) {
				return true;
			}
			return (character as BaseSigma)?.isHyperSigma == true;
		});
	}

	public byte updateAndGetNetCharState1() {
		isFrozenCastleActiveBS.updateValue();
		isStrikeChainHookedBS.updateValue();
		shouldDrawArmBS.updateValue();
		isAwakenedZeroBS.updateValue();
		isAwakenedGenmuZeroBS.updateValue();
		isInvisibleBS.updateValue();
		isReturnIXBS.updateValue();
		isHyperSigmaBS.updateValue();
		return netCharState1;
	}

	// NET CHAR STATE 2 SECTION
	public byte netCharState2;

	public NetCharBoolState isHyperChargeActiveBS;
	public NetCharBoolState isSpeedDevilActiveBS;
	public NetCharBoolState isInvulnBS;
	public NetCharBoolState hasUltimateArmorBS;
	public NetCharBoolState isDefenderFavoredBS;
	public NetCharBoolState hasSubtankCapacityBS;
	public NetCharBoolState isNightmareZeroBS;
	public NetCharBoolState isDarkHoldBS;

	public bool CanOnHitCancel { get; internal set; }


	public void initNetCharState2() {
		isHyperChargeActiveBS = new NetCharBoolState(this, 0, NetCharBoolStateNum.Two, (character) => { return character.player.showHyperBusterCharge(); });
		isSpeedDevilActiveBS = new NetCharBoolState(this, 1, NetCharBoolStateNum.Two, (character) => { return character.player.speedDevil; });
		isInvulnBS = new NetCharBoolState(this, 2, NetCharBoolStateNum.Two, (character) => { return character.invulnTime > 0; });
		hasUltimateArmorBS = new NetCharBoolState(this, 3, NetCharBoolStateNum.Two, (character) => { return character.player.hasUltimateArmor(); });
		isDefenderFavoredBS = new NetCharBoolState(this, 4, NetCharBoolStateNum.Two, (character) => { return character.player.isDefenderFavored; });
		hasSubtankCapacityBS = new NetCharBoolState(this, 5, NetCharBoolStateNum.Two, (character) => { return character.player.hasSubtankCapacity(); });
		isNightmareZeroBS = new NetCharBoolState(this, 6, NetCharBoolStateNum.Two, (character) => {
			return (character as Zero)?.isNightmareZero == true;
		});
		isDarkHoldBS = new NetCharBoolState(this, 7, NetCharBoolStateNum.Two, (character) => { return character.charState is DarkHoldState; });
	}

	public byte updateAndGetNetCharState2() {
		isHyperChargeActiveBS.updateValue();
		isSpeedDevilActiveBS.updateValue();
		isInvulnBS.updateValue();
		hasUltimateArmorBS.updateValue();
		isDefenderFavoredBS.updateValue();
		hasSubtankCapacityBS.updateValue();
		isNightmareZeroBS.updateValue();
		isDarkHoldBS.updateValue();
		return netCharState2;
	}

	
	// NET CHAR STATE 3 SECTION
	public byte netCharState3;

	public NetCharBoolState isLightArmorXBS;
	public NetCharBoolState isGigaArmorXBS;
	public NetCharBoolState isMaxArmorXBS;
	public NetCharBoolState isForceArmorXBS;
	public NetCharBoolState isFalconArmorXBS;
	public NetCharBoolState isGaeaArmorXBS;
	public NetCharBoolState isBladeArmorXBS;
	public NetCharBoolState isShadowArmorXBS;

	

	public void initNetCharState3() {
		isLightArmorXBS = new NetCharBoolState(this, 0, NetCharBoolStateNum.Three, (character) => {	return character.player.hasFullLight(); });
		isGigaArmorXBS = new NetCharBoolState(this, 1, NetCharBoolStateNum.Three, (character) => { return character.player.hasFullGiga(); });
		isMaxArmorXBS = new NetCharBoolState(this, 2, NetCharBoolStateNum.Three, (character) => {return character.player.hasAllX3Armor(); });
		isForceArmorXBS = new NetCharBoolState(this, 3, NetCharBoolStateNum.Three, (character) => { return character.player.HasFullForce(); });
		isFalconArmorXBS = new NetCharBoolState(this, 4, NetCharBoolStateNum.Three, (character) => { return character.player.HasFullFalcon(); });
		isGaeaArmorXBS = new NetCharBoolState(this, 5, NetCharBoolStateNum.Three, (character) => { return character.player.HasFullGaea(); });
		isBladeArmorXBS = new NetCharBoolState(this, 6, NetCharBoolStateNum.Three, (character) => {return character.player.HasFullBlade(); });
		isShadowArmorXBS = new NetCharBoolState(this, 7, NetCharBoolStateNum.Three, (character) => {return character.player.HasFullShadow(); });
	}

	public byte updateAndGetNetCharState3() {
		isLightArmorXBS.updateValue();
		isGigaArmorXBS.updateValue();
		isMaxArmorXBS.updateValue();
		isForceArmorXBS.updateValue();
		isFalconArmorXBS.updateValue();
		isGaeaArmorXBS.updateValue();
		isBladeArmorXBS.updateValue();
		isShadowArmorXBS.updateValue();
		return netCharState3;
	}
}
