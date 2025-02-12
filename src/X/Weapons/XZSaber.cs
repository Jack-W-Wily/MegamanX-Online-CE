namespace MMXOnline;

public class ZXSaber : Weapon {
	public static ZXSaber netWeapon = new();
	public ZXSaber() : base() {
		//damager = new Damager(player, 4, Global.defFlinch, 0.25f);
		shootSounds = new string[] { "", "", "", "", "" };
		
		index = (int)WeaponIds.XSaber;
		weaponBarBaseIndex = (int)WeaponBarIndex.ZSaber;
		weaponBarIndex = weaponBarBaseIndex;
		killFeedIndex = 66;
		weaponSlotIndex = (int)SlotIndex.ZSaber;
		type = index;
		damage = "2/4";
		Flinch = "10/26";
		FlinchCD = "0";
		hitcooldown = "0.3/0.3|0.5";
		displayName = "Z-Saber ";
		effect = "Zero's Saber. \nCharged shoots a saber wave.";
		
	}


	public override void shoot(Character character, int[] args) {
		int chargeLevel = args[0];
		Point pos = character.getShootPos();
		int xDir = character.getShootXDir();
		Player player = character.player;

		if (chargeLevel < 3) {
		character.changeState(new X6SaberState(character.grounded), true);
		} else {
		character.changeState(new XSaberState(character.grounded), true);
	
		}
	}
}

public class XSaberProj : Projectile {
	public XSaberProj(
		Point pos, int xDir,
		Player player, ushort netProjId, bool rpc = false
	) : base(
		ZXSaber.netWeapon, pos, xDir, 300, 4, player, "zsaber_shot", 
		Global.defFlinch, 0.5f, netProjId, player.ownedByLocalPlayer
	) {
		reflectable = true;
		projId = (int)ProjIds.XSaberProj;
		maxTime = 0.5f;

		if (rpc) {
			rpcCreate(pos, player, netProjId, xDir);
		}
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new XSaberProj(
			arg.pos, arg.xDir, arg.player, arg.netId
		);
	}
}

public class XSaberState : CharState {
	bool fired;
	bool grounded;
	public XSaberState(bool grounded) : base(grounded ? "beam_saber" : "beam_saber_air") {
		this.grounded = grounded;
		landSprite = "beam_saber";
		airMove = true;
		useDashJumpSpeed = true;
	}

	public override void update() {
		base.update();
		if (character.frameIndex >= 6 && !fired) {
			fired = true;
			character.playSound("zerosaberx3");
			new XSaberProj(
				character.pos.addxy(20 * character.xDir, -20), 
				character.xDir, player, player.getNextActorNetId(), rpc: true
			);
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}

public class X6SaberState : CharState {
	bool fired;
	bool grounded;
	public X6SaberState(bool grounded) : base(grounded ? "beam_saber" : "beam_saber_air") {
		this.grounded = grounded;
		landSprite = "beam_saber";
		airMove = true;
		useDashJumpSpeed = true;
		normalCtrl = true;
	}

	public override void update() {
		base.update();
		int frameSound = 1;
		if (character.frameIndex >= frameSound && !fired) {
			fired = true;
			character.playSound("raijingeki");
			//new XSaberProj(new XSaber(player), character.pos.addxy(30 * character.xDir, -29), character.xDir, player, player.getNextActorNetId(), rpc: true);
		}

		if (player.character.canCharge() && player.input.isHeld(Control.Shoot, player)) {
			player.character.increaseCharge();
		}

		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}
	}
}
