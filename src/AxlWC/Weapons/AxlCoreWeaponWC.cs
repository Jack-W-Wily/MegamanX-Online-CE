namespace MMXOnline;

public class AxlWeaponWC : Weapon {
	// Controls the cooldown and other stuff.
	public int altFireSelected;
	public float altFireRate;
	public bool autoFire;
	// Sprite definitions.
	public bool isTwoHanded;
	public string flashSprite = "";
	public string chargedFlashSprite = "";
	public string sprite = "axl_arm_pistol";
	// Defaults to 3. Can be changed per weapon.
	public float maxSwapCooldown = 60 * 3;
	public float swapCooldown;
	public int spriteFrameIndex;

	public AxlWeaponWC() {}

	public virtual void preAxlUpdate(AxlWC axl, bool isSelected) { }
	public virtual void axlUpdate(AxlWC axl, bool isSelected) { }
	public virtual void postAxlUpdate(AxlWC axl, bool isSelected) { }
	public virtual bool attackCtrl(AxlWC axl) {
		return false;
	}
	public virtual float getFireRate(AxlWC axl, int chargeLevel) {
		return fireRate;
	}
	public virtual float getAltFireRate(AxlWC axl, int chargeLevel) {
		return altFireRate;
	}
	public virtual float getRecoil(AxlWC axl, int chargeLevel) {
		float recoil = getFireRate(axl, chargeLevel) - 2;
		return Helpers.clamp(recoil, 0, 12);
	}
	public virtual float getAltRecoil(AxlWC axl, int chargeLevel) {
		float recoil = getAltFireRate(axl, chargeLevel) - 2;
		return Helpers.clamp(recoil, 0, 12);
	}
	public virtual float getAmmoUse(AxlWC axl, int chargeLevel) {
		return 1;
	}
	public virtual float getAltAmmoUse(AxlWC axl, int chargeLevel) {
		return 1;
	}
	public virtual void shootMain(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
	}
	public virtual void shootAlt(AxlWC axl, Point pos, float byteAngle, int chargeLevel) {
	}
}
