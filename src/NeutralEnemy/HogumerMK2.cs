namespace MMXOnline;

public class HogumerMK2 : Character {

	Character owner = null!;
	Player pl = null!;
	public float maxHealth = 8;
	public float health = 8;


		private float AtkCD;

public HGM2RPC rpc;

	public HogumerMK2(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn, false, false
	) {
		charId = CharIds.HogumerMK2;
			Global.level.addGameObject(this);
				owner = player.character;
	
		createActorRpc(player.id);
	

	//	ai = new AI(this);
	}

	public override bool canDash() {
		return false;
	}

	public override bool canWallClimb() {
		return false;
	}


		
	
		public override void update() {
		// To assure they Appear Online
	//	if (rpc == null && !Global.isOffline && charState is not WarpIn){
	//		rpc = new HGM2RPC(new SoulBody(), pos, xDir, player, player.getNextActorNetId(), true);
	//	}
		base.update();
		Helpers.decrementTime(ref AtkCD);


		if (health <= 0) {
			destroySelf();

		}

		if (charState.attackCtrl && AtkCD == 0 && grounded){


			foreach (var otherPlayer in Global.level.players) {
				if (otherPlayer.character == null) continue;
				if (otherPlayer == player) continue;
				if (otherPlayer == parasiteDamager?.owner) continue;
				if (otherPlayer.character.isInvulnerable()) continue;
				if (Global.level.gameMode.isTeamMode && otherPlayer.alliance != player.alliance) continue;
				if (otherPlayer.character.getCenterPos().distanceTo(getCenterPos()) > ParasiticBomb.carryRange) continue;
				Character target = otherPlayer.character;

				if (pos.x > target.pos.x) {
					if (xDir != -1) {
					xDir = -1;
					}
				} else {
					if (xDir != 1) {
					xDir = 1;
					}
				}
				
					changeState(new HogumerShot(), true);
				break;
			}
	
		AtkCD = 2;
		}


		
	}

	
	public override void onDestroy() {
		base.onDestroy();
		if (owner is Rock rk)
		rk.hgm = null;
		playSound("rcExplode");
		Anim.createGibEffect("hogumermk2_piece", getCenterPos(), netOwner, GibPattern.Radial);

	}




	public override void applyDamage(float fDamage, Player? attacker, Actor? actor, int? weaponIndex, int? projId) {
		if (!ownedByLocalPlayer) return;
		decimal damage = decimal.Parse(fDamage.ToString());

		if (damage > 0 && actor != null && attacker != null && health > 0) {
			health -= fDamage;
			//playSound("hit", sendRpc: true);

	//		if (player.hasPlasma() && !plasma) {
	//			new BusterForcePlasmaHit(
	//				0, player.weapon, getCenterPos(), xDir, player,
	//				player.getNextActorNetId(), true
	//			);
	//			plasma = true;
	//		}
			
			base.applyDamage(fDamage, attacker, actor, weaponIndex, projId);
			return;
		}
	}

	



	public override string getSprite(string spriteName) {
		return "hogumermk2_" + spriteName;
	}





	
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {
		
		if (sprite.name.Contains("idle")) {
			return new GenericMeleeProj(
				new SonicSlicer(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		}

		return null;
	}


}








public class HogumerShot : CharState {

	private float partTime;

	private float chargeTime;

	private float specialPressTime;
	
	public float pushBackSpeed;

	ZBuster3Proj proj;

	public HogumerShot(string transitionSprite = "")
		: base("shoot", "", "", transitionSprite)
	{
	airMove = true;
	
	}

	public override void update()
	{
		if (!character.grounded){
		character.changeSpriteFromName("airshoot", true);
		}

		if (!character.grounded && pushBackSpeed > 0) {
			character.useGravity = false;
			character.move(new Point(-60 * character.xDir, -pushBackSpeed * 2f));
			pushBackSpeed -= 7.5f;
		} else {
			if (!character.grounded) {
				character.move(new Point(-30 * character.xDir, 0));
			}
			character.useGravity = true;
		}

		
		if (proj == null && character.frameIndex >= 5 && character.ownedByLocalPlayer){
		character.playSound("buster3X3", forcePlay: false, sendRpc: true);
		proj = new ZBuster3Proj(
				character.getShootPos(), 
				character.xDir, 0, player, 
				player.getNextActorNetId(), rpc: true);
		}
		
		base.update();
		Helpers.decrementTime(ref specialPressTime);
	
		if (character.isAnimOver()) {
			character.changeToIdleOrFall();
		}



	}

	public override void onEnter(CharState oldState) {
		base.onEnter(oldState);
		if (!character.grounded) {
			character.stopMovingWeak();
			pushBackSpeed = 100;
		}
	
		
	
	}

	public override void onExit(CharState newState) {
		base.onExit(newState);
		character.useGravity = true;
    }
}





public class HGM2RPC : Projectile {

float distance;
	const float maxDist = 1;
	int frameCount;



	public HGM2RPC(
		Weapon weapon, Point pos, int xDir, 
		Player player, ushort netProjId,  bool rpc = false
	) : base(
		weapon, pos, xDir, 0, 0, player,
		"hogumermk2_idle", 0, 0.33f, netProjId,
		player.ownedByLocalPlayer
	) {
		
		projId = (int)ProjIds.HGM2RPC;
		fadeOnAutoDestroy = true;
		frameSpeed = 0;
	//	changeSprite(owner.character.hgm.sprite.name, false);
	//	frameIndex = owner.character.hgm.frameIndex;
	//	owner.character.hgm.rpc = this;
		maxTime = 9999f;
		setIndestructableProperties();
		canBeLocal = false;

		if (rpc) rpcCreate(pos, player, netProjId, xDir);
	}

	public static Projectile rpcInvoke(ProjParameters arg) {
		return new HGM2RPC(
			SoulBody.netWeapon, arg.pos, arg.xDir, arg.player, arg.netId
		);
	}
	
	public override void update() {
		base.update();
		if (!ownedByLocalPlayer) return;
		
			if (owner.character.hgm == null) destroySelf();
	
		if (owner.character.destroyed || owner.character.charState is Die
		|| owner.health < 1) {
			destroySelf();
			return;
		}

			if (owner.character.hgm != null){
			xDir = owner.character.hgm.xDir;

		if (distance < maxDist) distance += 4;
		else distance = maxDist;

	

		changePos(owner.character.hgm.pos.addxy(owner.character.hgm.getShootXDir() * distance, 0));
		changeSprite(owner.character.hgm.sprite.name, false);
		frameIndex = owner.character.hgm.frameIndex;
		frameCount++;

		if (time >= maxTime * 0.75f) {
			visible = frameCount % 2 == 0;
		}


		} else{
		destroySelf();
		}
	}





	
//	public override List<ShaderWrapper>? getShaders() {
//		var shaders = new List<ShaderWrapper>();
//		ShaderWrapper cloneShader = Helpers.cloneShaderSafe("soulBodyPalette");
//		int index = (frameCount / 2) % 7;
//		if (index == 0) index++;
//
	//	cloneShader.SetUniform("palette", index);
	//	cloneShader.SetUniform("paletteTexture", Global.textures["soul_body_palette"]);
	//	shaders.Add(cloneShader);
	//
	//	if (shaders.Count > 0) {
	//		return shaders;
	//	} else {
	//		return base.getShaders();
	//	}
	//} 

	public override void onDestroy() {
		base.onDestroy();
		owner.character.hgm = null;
	}
}




