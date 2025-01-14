namespace MMXOnline;

public class GBD : Character {
	public GBD(
		Player player, float x, float y, int xDir,
		bool isVisible, ushort? netId, bool ownedByLocalPlayer,
		bool isWarpIn = true
	) : base(
		player, x, y, xDir, isVisible, netId, ownedByLocalPlayer, isWarpIn
	) {
		charId = CharIds.GBD;

		spriteFrameToSounds["tonfa_charged_fw/2"] = "recoilRod2";
		spriteFrameToSounds["tonfa_charged_up/2"] = "recoilRod2";

	}




	private float flameCreateTime = 0;

	private float SuperGBDCreateTime = 0;

	public float shiningSparkStacks = 0;

	public bool isOnBike = false;

	public bool canUseBike;

	public float TPCooldown;

	public float MinesCooldown;


	public override bool normalCtrl() {

		if (player.input.isHeld(Control.Up, player) &&
			!isAttacking() && grounded && //noBlockTime == 0 &&
			charState is not SwordBlock
		) {
			changeState(new SwordBlock());
			return true;
		}
		return base.normalCtrl();
	}


	public override void update() {
		base.update();

		Helpers.decrementTime(ref TPCooldown);
		Helpers.decrementTime(ref MinesCooldown);

		// For the shooting animation.
		if (shootAnimTime > 0 && sprite.name == getSprite(charState.shootSprite)) {
			shootAnimTime -= speedMul;
			if (shootAnimTime <= 0) {
				shootAnimTime = 0;
				changeSpriteFromName(charState.defaultSprite, false);
				if (charState is WallSlide) {
					frameIndex = sprite.totalFrameNum - 1;
				}
			}
		}

		int upOrDown = player.input.getYDir(player);

		if (charState.attackCtrl && ChainrodProj == null &&
		player.input.isPressed(Control.WeaponLeft, player)) {
			ChainrodProj = new ChainrodProj(new ShieldBoomerang(), getShootPos(), xDir, player, player.getNextActorNetId(), upOrDown, true);
			setShootAnim();
			playSound("recoilRod1", true);
		}

		if (player.input.isHeld(Control.Down, player) && MinesCooldown == 0 &&
			player.input.isPressed(Control.Special1, player)) {
			var magnetMineProj = new MagnetMineProj(new ShieldBoomerang(), pos, xDir, player, player.getNextActorNetId(), true);
			magnetMines.Add(magnetMineProj);
			if (magnetMines.Count > 8) {
				magnetMines[0].destroySelf();
				magnetMines.RemoveAt(0);
				MinesCooldown = 2;
			}
		}

		if (grounded) {
			if (player.input.isPressed(Control.Jump, player)) {
				vel.y = -getJumpPower();
				isDashing = (
					isDashing || player.dashPressed(out string dashControl) && canDash());
				changeState(new Jump());
			}
		}
		if (!canUseBike && player.input.isPressed(Control.Special2, player) && player.currency > 2) {
			player.currency -= 3;
			canUseBike = true;
			Global.level.gameMode.setHUDErrorMessage(
						player, "Enabled Ride Chaser.", playSound: false, resetCooldown: true
					);
		}

		//KillingSpreeThemes
		//	if (KillingSpree == 3){
		//			if (musicSource == null) {
		//	if (Helpers.randomRange(0,1) == 0)	addMusicSource("x2opening", getCenterPos(), true);  
		//	if (Helpers.randomRange(0,1) == 1)	addMusicSource("BlueWaterBlueSky", getCenterPos(), true); 
		//
		//			}
		//	} 


		//if (shiningSparkStacks > 0){
		//{
		slideVel = xDir * shiningSparkStacks;
		//	}
		//}

		flameCreateTime += Global.spf;
		SuperGBDCreateTime += Global.spf;
		if (flag == null) {
			if (charState is not Hurt &&
				charState is not InRideArmor && (
				deltaPos.x != 0 ||
				charState is Run ||
				charState is Dash ||
				charState is AirDash ||
				charState is WallKick
			)) {
				if (shiningSparkStacks < 6) {
					shiningSparkStacks += Global.spf * 0.75f;
				} else {
					shiningSparkStacks += Global.spf;
				}
				if (shiningSparkStacks < 125 && shiningSparkStacks > 6) {
					if (shiningSparkStacks < 16) {
						shiningSparkStacks += (Global.spf * (shiningSparkStacks - 6f)) / 8;
					} else {
						shiningSparkStacks += 0.125f;
					}
				}
			} else if (
				shiningSparkStacks > 0 &&
				!charState.inTransition() && (
					charState is Idle ||
					charState is Jump ||
					charState is Crouch ||
					charState is Fall ||
					charState is Hurt
				)
			) {
				if (shiningSparkStacks > 1) {
					shiningSparkStacks -= Global.spf * (5 * shiningSparkStacks);
				} else {
					shiningSparkStacks -= Global.spf * 5;
				}
				if (shiningSparkStacks < 0) {
					shiningSparkStacks = 0;
				}
				if (shiningSparkStacks > 40) {
					shiningSparkStacks = 40;
				}
			}
		} //else {
		  //		shiningSparkStacks = 0;
		  //}
		if (shiningSparkStacks < 0) {
			shiningSparkStacks = 0;
		}

		if (dashedInAir == 0 && charState.attackCtrl && player.input.isHeld("up", player)
		&& player.input.isPressed("jump", player) && charState is not WallKick) {
			isDashing = true;
			dashedInAir += 1;
			vel.y = 0f - getJumpPower();
			changeState(new WallKick(), true);
		}
		if (charState.attackCtrl && !isOnBike &&
		 player.input.isPressed(Control.WeaponRight, player)) {
			if ((player.input.isHeld("left", player) ||
			player.input.isHeld("right", player))
			&& !player.input.isHeld("up", player)) {
				changeState(new GBDSpear1(), true);
			}
			if ((!player.input.isHeld("left", player) &&
			!player.input.isHeld("right", player))
			&& player.input.isHeld("up", player)) {
				changeState(new GBDSpearUp(), true);
			}
			if ((!player.input.isHeld("left", player) &&
			!player.input.isHeld("right", player))
			&& !player.input.isHeld("up", player)) {
				changeState(new GBDSpearSpin(), true);
			}
			if ((player.input.isHeld("left", player) ||
			player.input.isHeld("right", player))
			&& player.input.isHeld("up", player)) {
				changeState(new GBDSpearRising(), true);
			}

		}


		if (charState.attackCtrl && !isOnBike &&
		 player.input.isPressed(Control.Shoot, player)) {
			if (!sprite.name.Contains("tonfa")) {
				changeSpriteFromName("tonfa_1", true);
				playSound("recoilRod1", true);

			}

			if (sprite.name.Contains("tonfa_1") && sprite.frameIndex > 3) {
				changeSpriteFromName("tonfa_2", true);
				playSound("recoilRod1", true);

			}

			if (sprite.name.Contains("tonfa_2") && sprite.frameIndex > 3) {
				changeSpriteFromName("tonfa_3", true);
				playSound("recoilRod1", true);

			}




		}

		if (sprite.name.Contains("tonfa") && isAnimOver()) {
			changeToIdleOrFall();
		}

		if (charState.attackCtrl && TPCooldown == 0f &&
		 player.input.isPressed("special1", player)) {
			if (player.input.isHeld("up", player)) {
				changeState(new EnfrenteMeuDisco2(), true);
				TPCooldown = 1.5f;
			}
			if (!player.input.isHeld("up", player) && !player.input.isHeld("down", player)) {
				changeState(new EnfrenteMeuDisco(), true);
				TPCooldown = 1f;
			}
		}
		if (canUseBike && player.input.isPressed(Control.Special2, player)) {
			if (linkedRideChaser == null) {
				linkedRideChaser = new RideChaser(player, pos, 0, player.getNextActorNetId(), true, sendRpc: true);
			} else {
				linkedRideChaser.changePos(player.character.pos);

			}
		}





		if (charState is KnockedDown) isOnBike = false;

		if (charState.attackCtrl && player.input.isPressed(Control.Dash, player)
			&& player.input.checkDoubleTap(Control.Dash)) {
			changeState(new XTeleportState(), true);
			//xSaberCooldown = 1f;
			//new MagnetMine().getProjectile(getShootPos(), getShootXDir(), player, 0, player.getNextActorNetId());
		}

		if (!isInDamageSprite()) {
			chargeLogic(shoot);
		}
	}


	public void setShootAnim() {
		string shootSprite = getSprite(charState.shootSprite);
		if (!Global.sprites.ContainsKey(shootSprite)) {
			if (grounded) { shootSprite = "tgbd_shoot"; } else { shootSprite = "tgbd_fall_shoot"; }
		}
		if (shootAnimTime == 0) {
			changeSprite(shootSprite, false);
		} else if (charState is Idle) {
			frameIndex = 0;
			frameTime = 0;
		}
		if (charState is LadderClimb) {
			if (player.input.isHeld(Control.Left, player)) {
				this.xDir = -1;
			} else if (player.input.isHeld(Control.Right, player)) {
				this.xDir = 1;
			}
		}
		shootAnimTime = DefaultShootAnimTime;
	}



	public void shoot(int chargeLevel) {
		if (chargeLevel == 0) { return; }
		int currencyUse = 0;
		if (chargeLevel >= 2) {
			if (!player.input.isHeld(Control.Up, player)) {
				changeSpriteFromName("tonfa_charged_fw", true);
			} else {
				changeSpriteFromName("tonfa_charged_up", true);
			}
		}

	}

	public override bool chargeButtonHeld() {
		return player.input.isHeld(Control.Special1, player)
		|| player.input.isHeld(Control.Shoot, player);
	}


	public override int getMaxChargeLevel() {
		return 2;
	}


	public override bool canCharge() {
		return !isInvulnerableAttack();
	}



	// This can run on both owners and non-owners. So data used must be in sync
	public override Projectile getProjFromHitbox(Collider collider, Point centerPoint) {

		if (sprite.name.Contains("_block")) {
			return new GenericMeleeProj(
				new GBDKick(), centerPoint, ProjIds.SigmaSwordBlock, player, 0, 0, 0, isDeflectShield: true
			);
		} else if (sprite.name.Contains("dash") && (isOnBike || shiningSparkStacks > 10)) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			 ProjIds.SigmaSwordBlock, player, 2f, 20, 15f, null,
			 isShield: false, isDeflectShield: true,
			 isJuggleProjectile: true, ShouldClang: true);
		} else if (sprite.name.Contains("jump") && player.input.isPressed("jump", player)) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.GBDKick, player, 0f, 0, 0f);
		} else if (!isOnBike && sprite.name.Contains("fall") &&
		   player.input.isPressed("jump", player)) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.GBDKick, player, 0f, 0, 0f);
		} else if (sprite.name.Contains("wall_kick")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.MechFrogStompShockwave, player, 2f, 0, 15f);
		}

		if (sprite.name.Contains("spear_1")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.Raijingeki2, player, 2f, 30, 14f,
			isJuggleProjectile: true);
		}
		if (sprite.name.Contains("spear_up")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.Raijingeki2, player, 2f, 30, 15f,
			isJuggleProjectile: true);
		}
		if (sprite.name.Contains("spear_rising")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			 ProjIds.Raijingeki2, player, 2f, 30, 30f,
			isJuggleProjectile: true);
		}
		if (sprite.name.Contains("spear_spin")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.Raijingeki2, player, 1f, 10, 15f,
			isJuggleProjectile: true, ShouldClang: true);
		}

		if (sprite.name.Contains("tonfa_1")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.UPPunch, player, 1f, 10, 15f,
			 ShouldClang: true);
		}
		if (sprite.name.Contains("tonfa_2")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.UPPunch, player, 1f, 10, 15f,
			 ShouldClang: true);
		}
		if (sprite.name.Contains("tonfa_3")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.UPPunch, player, 2f, 10, 15f,
			isJuggleProjectile: true, ShouldClang: true);
		}
		if (sprite.name.Contains("tonfa_charged_fw")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.NormalPush, player, 4f, 0, 15f,
			isJuggleProjectile: true, ShouldClang: true);
		}
		if (sprite.name.Contains("tonfa_charged_up")) {
			return new GenericMeleeProj(new GBDKick(), centerPoint,
			ProjIds.BlockableLaunch, player, 4f, 0, 15f,
			isJuggleProjectile: true, ShouldClang: true);
		}
		return null;
	}


	public override bool canDash() {
		return true;
	}

	public override bool canControlAirDash() {
		return true;
	}

	public override bool canWallClimb() {
		return true;
	}

	public override string getSprite(string spriteName) {
		if (isOnBike) return "tgbdbike_" + spriteName;
		return "tgbd_" + spriteName;
	}
}

