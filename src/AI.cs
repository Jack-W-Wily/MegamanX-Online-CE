using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public enum AITrainingBehavior {
	Default,
	Idle,
	Attack,
	Jump,
	Crouch,
	Guard
}

public class AI {
	public Character character;
	public AIState aiState;
	public Actor? target;
	public float shootTime;
	public float dashTime = 0;
	public float jumpTime = 0;
	public float weaponTime = 0;
	public float maxChargeTime = 0;
	public int framesChargeHeld = 0;
	public float jumpZoneTime = 0;
	public bool flagger = false; //Will this ai aggressively capture the flag?
	public static AITrainingBehavior trainingBehavior;
	public int axlAccuracy;
	public int mashType; //0=no mash, 1 = light, 2 = heavy

	public Player player { get { return character.player; } }

	public AI(Character character) {
		this.character = character;
		aiState = new AimAtPlayer(this.character);
		if (Global.level.flaggerCount < 2) {
			flagger = true;
			Global.level.flaggerCount++;
		}
		axlAccuracy = Helpers.randomRange(10, 30);
		mashType = Helpers.randomRange(0, 2);
		if (Global.level.isTraining()) mashType = 0;
	}

	public void doJump(float jumpTime = 0.75f) {
		if (this.jumpTime == 0) {
			//this.player.release(Control.Jump);
			player.press(Control.Jump);
			this.jumpTime = jumpTime;
		}
	}

	public RideChaser? raceAiSetupRc;

	public RideChaser? getRaceAIChaser() {
		var rideChasers = new List<RideChaser>();
		foreach (var go in Global.level.gameObjects) {
			if (go is RideChaser rc && !rc.destroyed && rc.character == null) {
				rideChasers.Add(rc);
			}
		}
		rideChasers = rideChasers.OrderBy(rc => rc.pos.distanceTo(character.pos)).ToList();
		var rideChaser = rideChasers.FirstOrDefault();
		return rideChaser;
	}

	//Ride Chaser AI
	public void raceChaserAI() {
		if (character == null || character.charState is WarpIn) {
			return;
		}

		if (character.rideChaser == null) {
			var bestAIRideChaser = getRaceAIChaser();
			if (bestAIRideChaser != null) {
				raceAiSetupRc = bestAIRideChaser;
			} else if (raceAiSetupRc != null && ((raceAiSetupRc.character != null && raceAiSetupRc.character != character) || raceAiSetupRc.destroyed)) {
				raceAiSetupRc = null;
			}

			if (raceAiSetupRc == null) return;

			bool movedLastFrame = false;
			if (character.pos.x - raceAiSetupRc.pos.x > 5) {
				player.press(Control.Left);
				movedLastFrame = true;
			} else if (character.pos.x - raceAiSetupRc.pos.x < -5) {
				player.press(Control.Right);
				movedLastFrame = true;
			}

			if (!movedLastFrame) {
				player.press(Control.Jump);
			} else {
				player.release(Control.Jump);
			}
		} else {
			bool shouldShoot = false;
			var hits = Global.level.raycastAll(character.getCenterPos(), character.getCenterPos().addxy(character.xDir * 100, 0), new List<Type>() { typeof(RideChaser), typeof(Character) });
			foreach (var hit in hits) {
				if (hit?.gameObject is RideChaser rc) {
					if (rc.character != null && rc.character != character) {
						shouldShoot = true;
						break;
					}
				} else if (hit?.gameObject is Character hitChar) {
					if (hitChar != character) {
						shouldShoot = true;
						break;
					}
				}
			}
			if (shouldShoot) {
				player.press(Control.Shoot);
			} else {
				player.release(Control.Shoot);
			}

			var brakeZones = Global.level.getTriggerList(character.abstractedActor(), 0, 0, null, typeof(BrakeZone));
			if ((Global.level.gameMode as Race)?.getPlace(character.player) > 1) {
				dashTime = 100;
			} else {
				dashTime = 0;
			}

			if ((dashTime > 0 || jumpTime > 0) && brakeZones.Count == 0) {
				player.press(Control.Dash);
				dashTime -= Global.spf;
				if (dashTime < 0) dashTime = 0;
			}

			var turnZones = Global.level.getTriggerList(character.abstractedActor(), 0, 0, null, typeof(TurnZone));
			if (turnZones.FirstOrDefault()?.gameObject is TurnZone turnZone && turnZone.xDir != character.xDir) {
				if (turnZone.xDir == -1) {
					player.release(Control.Left);
					player.press(Control.Left);
				} else {
					player.release(Control.Right);
					player.press(Control.Right);
				}
			}

			if (jumpTime == 0) {
				var jumpZones = Global.level.getTriggerList(character.abstractedActor(), 0, 0, null, typeof(JumpZone));
				int jumpTurnZoneCount = turnZones.Count(turnZone => turnZone.gameObject is TurnZone tz && tz.jumpAfterTurn && tz.xDir == character.xDir);

				if (jumpZones.Count + jumpTurnZoneCount > 0 && character.rideChaser?.grounded == true) {
					jumpTime = (jumpZones.FirstOrDefault()?.gameObject as JumpZone)?.jumpTime ?? 0.5f;
				} else if (Helpers.randomRange(0, 300) < 1) {
					jumpTime = 0.5f;
				}
			} else {
				player.release(Control.Jump);
				player.press(Control.Jump);
				jumpTime -= Global.spf;
				if (jumpTime <= 0) {
					jumpTime = 0;
				}
			}
		}
	}
	//End of Ride Chaser AI
	public virtual void update() {
		if (Global.level.isRace() && Global.level.supportsRideChasers && Global.level.levelData.raceOnly) {
			raceChaserAI();
			return;
		}

		if (Global.debug || Global.level.isTraining()) {
			if (trainingBehavior == AITrainingBehavior.Idle) {
				player.release(Control.Shoot);
				player.release(Control.Jump);
				return;
			}
			if (trainingBehavior == AITrainingBehavior.Attack) {
				player.release(Control.Jump);
				player.press(Control.Shoot);
				return;
			}
			if (trainingBehavior == AITrainingBehavior.Jump) {
				player.release(Control.Shoot);
				player.press(Control.Jump);
				return;
			}
			if (trainingBehavior == AITrainingBehavior.Guard) {
				player.press(Control.WeaponLeft);
				return;
			}
			if (trainingBehavior == AITrainingBehavior.Crouch) {
				if (player.isSigma) {
					character?.changeState(new SigmaBlock(), true);
					player.press(Control.Down);
				} else {
					player.press(Control.Down);
				}
				return;
			}
		}

		if (Global.level.gameMode.isOver) return;

		var gameMode = Global.level.gameMode;
		if (!player.isMainPlayer && player.isX &&
			player.aiArmorUpgradeIndex < player.aiArmorUpgradeOrder.Count && !Global.level.is1v1()
		) {
			var upgradeNumber = player.aiArmorUpgradeOrder[player.aiArmorUpgradeIndex];
			if (upgradeNumber == 0 && player.currency >= MegamanX.bootsArmorCost) {
				UpgradeArmorMenu.upgradeBootsArmor(player, player.aiArmorPath);
				player.aiArmorUpgradeIndex++;
			} else if (upgradeNumber == 1 && player.currency >= MegamanX.bodyArmorCost) {
				UpgradeArmorMenu.upgradeBodyArmor(player, player.aiArmorPath);
				player.aiArmorUpgradeIndex++;
			} else if (upgradeNumber == 2 && player.currency >= MegamanX.headArmorCost) {
				UpgradeArmorMenu.upgradeHelmetArmor(player, player.aiArmorPath);
				player.aiArmorUpgradeIndex++;
			} else if (upgradeNumber == 3 && player.currency >= MegamanX.armArmorCost) {
				UpgradeArmorMenu.upgradeArmArmor(player, player.aiArmorPath);
				player.aiArmorUpgradeIndex++;
			}
		}

		if (framesChargeHeld > 0) {
			if (character.chargeTime < maxChargeTime) {
				//console.log("HOLD");
				player.press(Control.Shoot);
				if (player.isAxl && player.weapon is AxlBullet or DoubleBullet)
					player.press(Control.Special1);
			} else {
				//this.player.release(control.Shoot.key);
			}
		}

		if (target != null && !Global.level.gameObjects.Contains(target)) {
			target = null;
		}

		target = Global.level.getClosestTarget(character.pos, player.alliance, true, isRequesterAI: true);

		if (character is KaiserSigma || character is BaseSigma sigma && sigma.isHyperSigma) {
			int attack = Helpers.randomRange(0, 1);
			if (attack == 0) {
				player.release(Control.Special1);
				player.press(Control.Special1);
			} else if (attack == 1) {
				player.release(Control.Shoot);
				player.press(Control.Shoot);
			}
			if (Helpers.randomRange(0, 60) < 5) {
				player.changeWeaponSlot(Helpers.randomRange(0, 2));
			}
			return;
		}

		if (aiState is not InJumpZone) {
			var jumpZones = Global.level.getTriggerList(character.abstractedActor(), 0, 0, null, typeof(JumpZone));
			var neighbor = (aiState as FindPlayer)?.neighbor;
			if (neighbor != null) {
				jumpZones = jumpZones.FindAll(j => !neighbor.isJumpZoneExcluded(j.gameObject.name));
			}
			if (jumpZones.Count > 0 && jumpZones[0].gameObject is JumpZone jumpZone) {
				var jumpZoneDir = character.xDir;
				if (jumpZone.forceDir != 0) {
					jumpZoneDir = jumpZone.forceDir;
				}
				if (jumpZoneDir == 0) jumpZoneDir = -1;

				if (jumpZone.targetNode == null || jumpZone.targetNode == aiState.getNextNodeName()) {
					if (aiState is not FindPlayer) {
						changeState(new InJumpZone(character, jumpZone, jumpZoneDir));
					} else {
						if (jumpZone.forceDir == -1) {
							player.press(Control.Left);
						} else if (jumpZone.forceDir == 1) {
							player.press(Control.Right);
						}

						if (character.charState is not LadderClimb) {
							doJump();
							jumpZoneTime += Global.spf;
							if (jumpZoneTime > 2 && character.player.isVile) {
								jumpZoneTime = 0;
								player.press(Control.Up);
							}
						}
					}
				} else {
				}
			} else {
				jumpZoneTime = 0;
			}
		}

		if (character.flag != null) {
			target = null;
		} else if (Global.level.gameMode is CTF) {
			/*
			foreach (var player in Global.level.players)
			{
				if (player.character != null && player.alliance != character.player.alliance && player.character.flag != null)
				{
					target = player.character;
					break;
				}
			}
			*/
		}

		float stuckTime = (aiState as FindPlayer)?.stuckTime ?? 0;
		bool inNodeTransition = (aiState as FindPlayer)?.nodeTransition != null;

		if (aiState is not InJumpZone) {
			if (target == null) {
				if (aiState is not FindPlayer) {
					changeState(new FindPlayer(character));
				}
			} else {
				if (aiState is FindPlayer) {
					changeState(new AimAtPlayer(character));
				}
			}

			if (target != null) {
				if (character.charState is LadderClimb) {
					doJump();
				}
				var xDist = target.pos.x - character.pos.x;
				if (Math.Abs(xDist) > getMaxDist()) {
					changeState(new MoveTowardsTarget(character));
				}
			}
		}

		if (aiState.facePlayer && target != null) {
			if (character.pos.x > target.pos.x) {
				if (character.xDir != -1) {
					player.press(Control.Left);
				}
			} else {
				if (character.xDir != 1) {
					player.press(Control.Right);
				}
			}
			if (player.isAxl) {
				player.axlCursorPos = target.pos
					.addxy(-Global.level.camX, -Global.level.camY)
					.addxy(Helpers.randomRange(-axlAccuracy, axlAccuracy), Helpers.randomRange(-axlAccuracy, axlAccuracy));
			}
		}

		// Vile: Go MK2 to MKV
		if (character is Vile vile1) {
			if (player.canReviveVile() && vile1.isVileMK1)
				player.reviveVile(false);
			if (vile1.isVileMK2 && player.canReviveVile())
				player.reviveVile(true);
		}
		// Axl: 
		if (character is Axl) {
			if (player.weapon is not IceGattling)
				player.release(Control.Shoot);
			if (player.weapon is not IceGattling or PlasmaGun or RayGun)
				player.release(Control.Special1);

		}

		// X:
		if (character is MegamanX mmx) {
			if (player.canUpgradeUltimateX() && player.health >= 16) {
				if (!player.character.boughtUltimateArmorOnce) {
					player.currency -= Player.ultimateArmorCost;
					player.character.boughtUltimateArmorOnce = true;
				}
				player.setUltimateArmor(true);
				return;
			}
			if (player.hasAllX3Armor() && player.canUpgradeGoldenX() && player.health >= 16) {
				if (!player.character.boughtGoldenArmorOnce) {
					player.currency -= Player.goldenArmorCost;
					player.character.boughtGoldenArmorOnce = true;
				}
				player.setGoldenArmor(true);
				return;
			}
			if (player.canReviveX()) { player.reviveX(); }
			if (mmx.canCharge()) {
				player.character.increaseCharge();
			}
			if (mmx.isHyperX && mmx.canShoot()) {
				mmx.unpoShotCount = Math.Max(mmx.unpoShotCount, 4);
				player.release(Control.Shoot);
				player.press(Control.Shoot);
			}
		}

		// Sigma:
		if (character is BaseSigma baseSigma1) {
			if (player.canReviveSigma(out var spawnPoint)) player.reviveSigma(spawnPoint);
		}

		// Zero:
		if (character is Zero zero1)
		{
			if (player.isZSaber() && !zero1.isAwakenedZeroBS.getValue() && !(zero1.charState is HyperZeroStart or DarkHoldState or Hurt)  && zero1.sprite.name != null)
			{
				if (zero1.grounded && zero1.sprite.name == "zero_attack" && zero1.framePercent > 0.5f)
			 	{
					zero1.changeSprite("zero_attack2", true);
					zero1.turnToInput(player.input, player);
					zero1.playSound("saber2", sendRpc: true);
			 	}
				else if (zero1.grounded && zero1.sprite.name == "zero_attack2" && zero1.framePercent > 0.5f) {
					zero1.changeSprite("zero_attack3", true);
					zero1.turnToInput(player.input, player);
					zero1.playSound("saber3", sendRpc: true);
				}
				else if (zero1.grounded && zero1.sprite.name == "zero_attack3" && zero1.framePercent > 0.5f) {
					zero1.changeState(new ZeroUppercut(new RisingWeapon(zero1.player),zero1.isUnderwater()), forceChange: true);
				}
				else if (zero1.sprite.name == "zero_rising" && zero1.framePercent > 0.8f) {
					zero1.changeState(new ZeroFallStab(new HyouretsuzanWeapon(player)), forceChange: true);
				}
			}
		}

		//Should Attack?
		if (aiState.shouldAttack && target != null) { //do not if is invulnerable
			if (!character.isInvulnerable()) {
				if (shootTime == 0) {
					bool isTargetInAir = target.pos.y < character.pos.y - 50;
					bool isTargetBellowYou = target.pos.y < character.pos.y + 10;
					bool isTargetSuperClose = target.pos.x - 3 >= character.pos.x;
					bool isTargetClose = target.pos.x - 15 > character.pos.x;
					bool isTargetSSC = target.pos.x == character.pos.x;

					// Always check that Kaiser Sigma is on Air
					if (target is Character chr && chr.player.isKaiserNonViralSigma() || player.isWolfSigma() || player.isViralSigma()) isTargetInAir = true;

					// is Facing the target?
					if (character.isFacing(target)) {
						//Makes the AI release the charge
						if (framesChargeHeld > 0) {
							if (character.chargeTime >= maxChargeTime) {
								player.release(Control.Shoot);
								framesChargeHeld = 0;
							}
						}

						//Zero Start
						if (character is Zero zero) {
							
							if (player.currency >= Player.zeroHyperCost && !zero.isSpriteInvulnerable() && !zero.isInvulnerable() 
							&& !(zero.charState is HyperZeroStart or LadderClimb) && !(zero.isHyperZero() || zero.isNightmareZeroBS.getValue())) {
									zero.changeState(new HyperZeroStart(zero.zeroHyperMode), true);
							}
							if ((zero.charState is not HyperZeroStart or LadderClimb or DarkHoldState or Hurt or Frozen or Crystalized or Die or WarpIn or WarpOut or WallSlide or WallKick or SwordBlock or ZeroFallStabLand) 
							&& zero.charState?.isGrabbedState == false && !player.isDead && zero.sprite.name != null && zero.charState.canAttack())
							{

								if (zero.isHyperZero() || zero.isNightmareZeroBS.getValue()) {
									switch (Helpers.randomRange(0, 96)) {
										case 1: // Zero Giga attack spam
											zero.changeState(new Rakuhouha(zero.zeroGigaAttackWeapon), true);
											break;
										case 2: // Double Buster Spam!
											zero.changeState(new ZeroDoubleBuster(true, true), true);
											break;
										case 3: // Genmurei Spam!
											zero.changeState(new GenmuState(), true);
											break;
										case 4: 
											if (zero.isBlackZero2()) {
											zero.changeState(new Rakuhouha(new CFlasher(zero.player)), true);
											}
											break;	
									}
								}		

								////Zero Buster Start	
								if (player.isZBusterZero()) {
									int ZBattack = Helpers.randomRange(0, 4);
									if (isTargetInAir) ZBattack = 4;
										switch (ZBattack) {
											case 0: // Press Shoot to lemon
												player.press(Control.Shoot);
												break;
											case 1: // Saber Swing
												player.press(Control.Special1);
												break;
											case 2: // Another action if the enemy is on Do Jump and do SaberSwing
												if (isTargetInAir)
													player.press(Control.Jump); player.press(Control.Special1);
												break;
											case 3:
												if (zero.stockedXSaber)
													zero.swingStockedSaber();
												break;
											case 4: //Do rising if enemy is on air and you are not on the ground counting if you are on Fall or Jump state
												if (isTargetInAir && !zero.grounded && zero.charState is Fall or Jump)
													zero.changeState(new ZeroUppercut(new RisingWeapon(player), zero.isUnderwater()), forceChange: true);
												break;
										}
								}
								//Zero Buster End

								//Zero Saber Start		
								if (player.isZSaber()) {
									int ZSattack = Helpers.randomRange(0, 12);
									if (isTargetInAir) ZSattack = 9;
									if (!(zero.sprite.name == "zero_attack" || zero.sprite.name == "zero_attack3" || zero.sprite.name == "zero_attack2")) {	
										switch (ZSattack) {
											//Randomizador
											case 0: // Attack
												if (zero.grounded && !zero.isAwakenedZeroBS.getValue()) {
													zero.changeSprite("zero_attack", true);
												}
												else {
													player.press(Control.Shoot);
												}
												break;
											case 1: //Uppercut 
												if (isTargetSuperClose && zero.grounded) {
													player.press(Control.Special1);
													player.press(Control.Up);
												}
												break;
											case 2: //Uppercut
												if (isTargetSuperClose && zero.grounded) {
													player.press(Control.Shoot); 
													player.press(Control.Up);
												}
												break;
											case 3: //Crouch slash
												if (zero.grounded && isTargetSuperClose) {
													player.press(Control.Down);
													player.press(Control.Shoot);
												}
												break;
											case 4: // If Zero is dashing, press special and do shippuga
												if (zero.charState is Dash && isTargetClose) {
													player.press(Control.Special1);
												}
												break;
											case 5: // If Zero is on the ground and has giga attack ammo of at least 8 to above do "Rakuhouha"
												if (zero.grounded && zero.zeroGigaAttackWeapon.ammo >= 8f) {
													player.press(Control.Down); 
													player.press(Control.Special1);
												}
												break;
											case 6: // Air special
												if (!zero.grounded) {
													player.press(Control.Special1);
												}
												break;
											case 7: // if the character is on fall state, Downthrust attack
												if (zero.charState is Fall && zero.charState is not ZeroUppercut) {
													zero.changeState(new ZeroFallStab(zero.zeroDownThrustWeaponA));
												}
												break;
											case 8: // if the character is on fall state, Downthrust attack
												if (zero.charState is Fall && zero.charState is not ZeroUppercut) {
													zero.changeState(new ZeroFallStab(zero.zeroDownThrustWeaponS));
												}
												break;
											case 9:
												if (isTargetInAir && !zero.grounded && zero.charState is Fall or Jump) {
													zero.changeState(new ZeroUppercut(new EBladeWeapon(player), zero.isUnderwater()), forceChange: true);
												}
												break;
											case 10: // Dash slash
												if (zero.charState is Dash && isTargetClose) {
													player.press(Control.Shoot);
												}
												break;
											case 11:
												if (zero.stockedXSaber) {
													zero.swingStockedSaber();
												}
												break;
											case 12:
												if (zero.grounded) {
													player.press(Control.Special1);
												}
												break;
										}
									}
								}
								//Zero Saber End

								//Zero Knuckle Start
								if (player.hasKnuckle()) {
									int ZKattack = Helpers.randomRange(0, 8);
									if (isTargetInAir) {ZKattack = 6;}

										switch (ZKattack) {
											//Randomizador
											case 0: // press shoot
												if (zero.grounded)
													zero.changeSprite(zero.getSprite(zero.charState.attackSprite.Replace("attack", "punch")), true);
												break;
											case 1: //Uppercut 
												if (zero.grounded)
													player.press(Control.Shoot); player.press(Control.Up);
												break;
											case 2: // If Zero is dashing, press special and do shippuga
												if (zero.charState is Dash)
													player.press(Control.Special1);
												break;
											case 3: // If Zero is on the ground and has giga attack ammo of at least 8 to above do "Rakuhouha"
												if (zero.grounded && zero.zeroGigaAttackWeapon.ammo >= 8f)
													player.press(Control.Down); player.press(Control.Special1);
												break;
											case 4: // 
												player.press(Control.Special1);
												break;
											case 5: // if the character is on fall state, Downthrust attack
												if (zero.charState is Fall)
													zero.changeState(new DropKickState(), true);
												break;
											case 6:
												if (isTargetInAir && !zero.grounded && zero.charState is Fall or Jump)
													zero.changeState(new ZeroUppercut(new ZeroShoryukenWeapon(player), zero.isUnderwater()), forceChange: true);
												break;
											case 7:
												if (zero.charState is Jump or Fall)
													player.press(Control.Shoot);
												break;
											case 8:
												if (zero.stockedXSaber)
													zero.swingStockedSaber();
												break;
										}
								}
								//Zero Knuckle end
							}
						}
						//Zero End

						//Sigma Start
						if (character is BaseSigma baseSigma) {
							bool once = false;
							if (baseSigma.player.weapon is MaverickWeapon mw &&
								mw.maverick == null && once == false &&
								baseSigma.canAffordMaverick(mw)
							) {
								baseSigma.buyMaverick(mw);
								if (mw.maverick != null) {
									baseSigma.changeState(new CallDownMaverick(mw.maverick, true, false), true);
								}
								mw.summon(player, baseSigma.pos.addxy(0, -112), baseSigma.pos, baseSigma.xDir);
								player.changeToSigmaSlot();
								once = true;
							}
							//Commander Sigma Start
							if (character is CmdSigma cmdSigma && character.charState is not LadderClimb) {
								int Sattack = Helpers.randomRange(0, 4);
								if (isTargetInAir) Sattack = 1;
								if (cmdSigma?.charState?.isGrabbedState == false && !player.isDead
									&& !cmdSigma.isInvulnerable() && cmdSigma.charState.canAttack()
									&& !(cmdSigma.charState is CallDownMaverick or SigmaSlashState)) {
									switch (Sattack) {
										case 0: // Beam Saber
											if (isTargetSuperClose) {
												cmdSigma.changeState(new SigmaSlashState(cmdSigma.charState), true);
											}
											break;
										case 1: // Machine Gun if the enemy is on the air
											if (cmdSigma.grounded && isTargetInAir) {
												cmdSigma?.changeState(new SigmaBallShoot(), forceChange: true);
											}
											break;
										case 2: // Triangle Kick
											if (cmdSigma.charState is Dash && cmdSigma.grounded) {
												cmdSigma.changeState(new SigmaWallDashState(cmdSigma.xDir, true), true);
											}
											break;
										case 3:
											if (!once) {
												cmdSigma.player.changeWeaponSlot(1);
												once = true;
											}
											break;
										case 4:
											if (!once) {
												cmdSigma.player.changeWeaponSlot(2);
												once = true;
											}
											break;
									}
								}
							}
							//Commander Sigma End

							//Neo Sigma Start
                            if (character is NeoSigma neoSigma && character.charState is not LadderClimb) {
                                int Neoattack = Helpers.randomRange(0, 5);
                                if (isTargetInAir) Neoattack = 2;
                                if (neoSigma?.charState?.isGrabbedState == false && !player.isDead && !neoSigma.isInvulnerable()
                                	&& !(neoSigma.charState is CallDownMaverick or SigmaElectricBall2State or SigmaElectricBallState)){
                                    switch (Neoattack) {
                                        case 0: 
                                          	neoSigma.changeState(new SigmaClawState(neoSigma.charState, neoSigma.grounded), true);											
                                            break;
                                        case 1:
                                            if (neoSigma.grounded && isTargetInAir) {
                                            	neoSigma.changeState(new SigmaUpDownSlashState(true), true);
											}
                                            break;
                                        case 2:
                                            if (!neoSigma.grounded && isTargetBellowYou) {
                                            	neoSigma.changeState(new SigmaUpDownSlashState(false), true);
											}
                                            break;
                                        case 3:
                                       		if (!once) {
                                      			neoSigma.player.changeWeaponSlot(1);
                                       			once = true;
											}
                                        	break;
                                        case 4:
                                        	if (!once) {
                                       			neoSigma.player.changeWeaponSlot(2);
                                        		once = true;
											}
                                       		break;
										case 5:
									   		neoSigma.player.changeWeaponSlot(0);
											break;
                                    }
                                }
                            }
							//Neo Sigma End

							//Doppma Sigma Start
                            if (character is Doppma DoppmaSigma && character.charState is not LadderClimb) {
                                int DoppmaSigmaAttack = Helpers.randomRange(0, 4);
                                if (isTargetInAir) DoppmaSigmaAttack = 1;
                                if (DoppmaSigma?.charState?.isGrabbedState == false && !player.isDead && !DoppmaSigma.isInvulnerable()
                                	&& !(DoppmaSigma.charState is CallDownMaverick or SigmaThrowShieldState or Sigma3Shoot)){
                                    switch (DoppmaSigmaAttack) {
                                        case 0: 
											DoppmaSigma.changeState(new Sigma3Shoot(player.input.getInputDir(player)), true);
                                            break;
                                        case 1:
                                            if (DoppmaSigma.grounded) {
                                          	DoppmaSigma.changeState(new SigmaThrowShieldState(), true);											
											}
                                            break;
                                        case 2:
                                       		if (!once) {
                                      			DoppmaSigma.player.changeWeaponSlot(1);
                                       			once = true;
											}
                                        	break;
                                        case 3:
                                        	if (!once) {
                                       			DoppmaSigma.player.changeWeaponSlot(2);
                                        		once = true;
											}
                                       		break;
										case 4:
									   		DoppmaSigma.player.changeWeaponSlot(0);
											break;
                                    }
                                }
                            }
							//Doppma Sigma End
						}
						//Vile Start	
						if (character is Vile vile) {
							// You dare to grab me? i will blow myself up
							if (character.charState?.isGrabbedState == true && player.health >= 12) {
								if (Helpers.randomRange(0, 100) < 1)
									character.changeState(new NecroBurstAttack(vile.grounded), true);
							}

							if (Helpers.randomRange(0, 100) < 1) {
								if (isTargetInAir && isTargetSuperClose && !(character.charState is VileRevive or HexaInvoluteState) && player.vileAmmo >= 24)
									character.changeState(new RisingSpecterState(vile.grounded), true);
							}
							int Vattack = Helpers.randomRange(0, 18);
							if (vile?.charState?.isGrabbedState == false && !player.isDead && vile.charState.canAttack()
                            && !(character.charState is VileRevive or HexaInvoluteState or NecroBurstAttack 
                            or StraightNightmareAttack or RisingSpecterState or VileMK2GrabState)) { 
								switch (Vattack) {
									case 0:
										player.press(Control.Shoot);
										break;
									case 1:
										player.weapon.vileShoot(WeaponIds.FrontRunner, vile);
										break;
									case 2:
										player.vileRocketPunchWeapon.vileShoot(WeaponIds.RocketPunch, vile);
										break;
									case 3:
										if (!character.grounded) {
											player.vileBallWeapon.vileShoot(WeaponIds.VileBomb, vile);
										}
										break;
									case 4:
										player.vileMissileWeapon.vileShoot(WeaponIds.ElectricShock, vile);
										break;
									case 5:
										player.vileCutterWeapon.vileShoot(WeaponIds.VileCutter, vile);
										break;
									case 6:
										if (character.grounded) {
											player.vileNapalmWeapon.vileShoot(WeaponIds.Napalm, vile);
										}
										break;
									case 7:
										if (character.charState is Fall) {
											player.vileFlamethrowerWeapon.vileShoot(WeaponIds.VileFlamethrower, vile);
										}
										break;
									case 8:
										if (player.vileAmmo >= 24) {
											player.vileLaserWeapon.vileShoot(WeaponIds.VileLaser, vile);
										}
										break;
									case 9:
										if (vile.isVileMK5) {
											vile?.changeState(new HexaInvoluteState(), true);
										}
										break;
								}
							}
						}
						//Axl Start
						if (character is Axl axl) {
							if (player.currency >= 10 && !player.isDead && !axl.isSpriteInvulnerable() && !axl.isInvulnerable() && !axl.isWhiteAxl()
							&& !(axl.charState is Hurt or Die or Frozen or Crystalized or Stunned or WarpIn or HyperAxlStart or WallSlide or WallKick or DodgeRoll))
								axl.changeState(new HyperAxlStart(axl.grounded), true);

							int AAttack = Helpers.randomRange(0, 1);
							if (axl.charState.canShoot() && !axl.isSpriteInvulnerable() && player.weapon.ammo > 0 && player.axlWeapon != null && axl.canShoot()
							&& axl?.charState?.isGrabbedState == false && !player.isDead && axl.canChangeWeapons() && character.canChangeWeapons()
							&& !(axl.charState is Hurt or Die or Frozen or Crystalized or Stunned or WarpIn or HyperAxlStart or WallSlide or WallKick or LadderClimb or DodgeRoll)) {
								switch (AAttack) {
									case 0:
										player.press(Control.Shoot);
										break;
									case 1:
										if (axl.player.weapon is not IceGattling or PlasmaGun)
											player.press(Control.Special1);
										break;
								}
							}
						}
						//X Start
						if (character is MegamanX megamanX) {
							int Xattack = Helpers.randomRange(0, 12);
							if (!megamanX.isHyperX && megamanX?.charState?.isGrabbedState == false && !player.isDead && megamanX.charState.canAttack() && megamanX.canShoot()
							&& megamanX.canChangeWeapons()
							&& !(character.charState is Hurt or Die or Frozen or Crystalized or Stunned or WarpIn or LadderClimb or Hadouken or Shoryuken or XUPGrabState)) {
								switch (Xattack) {
									case 0:
										// If X AI is facing Zero or Sigma
										if (target is Zero or CmdSigma or NeoSigma or Doppma) {
											switch (Helpers.randomRange(0, 10)) {
												// SpeedBurner	
												case 0:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(16);
													megamanX.player.press(Control.Shoot);
													break;
												// Frost Shield
												case 1:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(23);
													megamanX.player.press(Control.Shoot);
													break;
												// Triad Thunder
												case 2:
													if (isTargetSSC)
														megamanX.player.changeWeaponSlot(19);
													megamanX.player.press(Control.Shoot);
													break;
												case 3:
													// Gravity Well
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(22);
													megamanX.player.press(Control.Shoot);
													break;
												// Tornado Fang
												case 4:
													if (isTargetSuperClose)
														megamanX.player.changeWeaponSlot(24);
													megamanX.player.press(Control.Shoot);
													break;
												// Acid Burst
												case 5:
													if (isTargetSSC)
														megamanX.player.changeWeaponSlot(17);
													megamanX.player.press(Control.Shoot);
													break;
												// Parasite Bomb
												case 6:
													if (isTargetSSC)
														megamanX.player.changeWeaponSlot(18);
													megamanX.player.press(Control.Shoot);
													break;
												// Crystal Hunter
												case 7:
													if (isTargetSSC)
														megamanX.player.changeWeaponSlot(9);
													megamanX.player.press(Control.Shoot);
													break;
												// SilkShot
												case 8:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(11);
													megamanX.player.press(Control.Shoot);
													break;
												// Spin Wheel
												case 9:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(12);
													megamanX.player.press(Control.Shoot);
													break;
												// Electric Spark
												case 10:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(6);
													megamanX.player.press(Control.Shoot);
													break;
											}
										}
										break;
									case 1:
										// If X AI is facing X or Vile
										if (target is MegamanX or Vile) {
											switch (Helpers.randomRange(0, 12)) {
												// Rolling Shield	
												case 0:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(3);
													megamanX.player.press(Control.Shoot);
													break;
												// Storm Tornado
												case 1:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(5);
													megamanX.player.press(Control.Shoot);
													break;
												// Torpedo
												case 2:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(1);
													megamanX.player.press(Control.Shoot);
													break;
												case 3:
													// C Sting
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(3);
													megamanX.player.press(Control.Shoot);
													break;
												// B Cutter
												case 4:
													if (isTargetSuperClose)
														megamanX.player.changeWeaponSlot(7);
													megamanX.player.press(Control.Shoot);
													break;
												// S Ice
												case 5:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(8);
													megamanX.player.press(Control.Shoot);
													break;
												// Speed Burner
												case 6:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(16);
													megamanX.player.press(Control.Shoot);
													break;
												// Crystal Hunter
												case 7:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(9);
													megamanX.player.press(Control.Shoot);
													doJump(0.75f);
													break;
												// SpeedB
												case 8:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(16);
													megamanX.player.press(Control.Shoot);
													break;
												// Sonic Slicer
												case 9:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(13);
													megamanX.player.press(Control.Shoot);
													break;
												// Electric Spark
												case 10:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(6);
													megamanX.player.press(Control.Shoot);
													break;
												// FShield
												case 11:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(23);
													megamanX.player.press(Control.Shoot);
													break;
												// Acid
												case 12:
													if (isTargetSuperClose)
														megamanX.player.changeWeaponSlot(17);
													megamanX.player.press(Control.Shoot);
													break;
											}
										}
										break;
									case 2:
										// If X AI is facing Axl	
										if (target is Axl) {
											switch (Helpers.randomRange(0, 10)) {
												// Rolling Shield	
												case 0:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(3);
													megamanX.player.press(Control.Shoot);
													break;
												// Strike Chain
												case 1:
													if (isTargetSuperClose)
														megamanX.player.changeWeaponSlot(14);
													megamanX.player.press(Control.Shoot);
													break;
												// Bubbles
												case 2:
													if (isTargetSuperClose)
														megamanX.player.changeWeaponSlot(10);
													megamanX.player.press(Control.Shoot);
													break;
												case 3:
													// P Bomb
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(18);
													megamanX.player.press(Control.Shoot);
													break;
												// Acid
												case 4:
													if (isTargetSuperClose)
														megamanX.player.changeWeaponSlot(17);
													megamanX.player.press(Control.Shoot);
													break;
												// F Shield
												case 5:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(23);
													megamanX.player.press(Control.Shoot);
													break;
												// BCutter
												case 6:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(7);
													megamanX.player.press(Control.Shoot);
													break;
												// Crystal Hunter
												case 7:
													if (isTargetClose)
														player.changeWeaponSlot(9);
													player.press(Control.Shoot);
													doJump(0.75f);
													break;
												// SilkShot
												case 8:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(11);
													megamanX.player.press(Control.Shoot);
													break;
												// Sonic Slicer
												case 9:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(13);
													megamanX.player.press(Control.Shoot);
													break;
												// Electric Spark
												case 10:
													if (isTargetClose)
														megamanX.player.changeWeaponSlot(6);
													megamanX.player.press(Control.Shoot);
													break;
											}
										}
										break;
									case 3:
										if (target is MegamanX or Axl or Vile or NeoSigma) {
											if (megamanX.hasHadoukenEquipped() && megamanX.canUseFgMove() && isTargetSuperClose) {
												megamanX.player.currency -= 3;
												megamanX.player.fgMoveAmmo = 0;
												megamanX.changeState(new Hadouken(), true);
											}
										}
										break;
									case 4:
										if (target is Character && (player.isWolfSigma() || player.isViralSigma() || target is KaiserSigma && isTargetInAir == true && isTargetSuperClose)
										|| target is MegamanX or Zero or Axl or Vile or CmdSigma or NeoSigma or Doppma) {
											if (megamanX.hasShoryukenEquipped() && megamanX.canUseFgMove()) {
												megamanX.player.currency -= 3;
												megamanX.player.fgMoveAmmo = 0;
												megamanX.changeState(new Shoryuken(megamanX.isUnderwater()), true);
											}
										}
										break;
									case 5:
										if (!player.hasArmor() && isTargetSSC)
											megamanX.changeState(new X6SaberState(megamanX.grounded), true);
										break;
									case 6:
										int novaStrikeSlot = player.weapons.FindIndex(w => w is NovaStrike);
										if (player.hasUltimateArmor()) {
											megamanX.player.changeWeaponSlot(novaStrikeSlot);
											if (megamanX.player.weapon.ammo >= 16) {
												megamanX.player.press(Control.Shoot);
											} else { megamanX.player.changeWeaponSlot(getRandomWeaponIndex()); }
										}
										break;
									case 7:
										int gCrushSlot = player.weapons.FindIndex(w => w is GigaCrush);
										if (player.hasBodyArmor(2)) {
											megamanX.player.changeWeaponSlot(gCrushSlot);
											if (megamanX.player.weapon.ammo == 32)
												megamanX.player.press(Control.Shoot);
											else {
												megamanX.player.changeWeaponSlot(getRandomWeaponIndex());
											}
										}
										break;
									case 8:
										int hyperbuster = player.weapons.FindIndex(w => w is HyperBuster);
										if (player.hasArmArmor(3)) {
											player.changeWeaponSlot(hyperbuster);
											if (megamanX.player.weapon.ammo >= 16) {
												megamanX.player.press(Control.Shoot);
												megamanX.player.release(Control.Shoot);
											} else { megamanX.player.changeWeaponSlot(getRandomWeaponIndex()); }
										}
										break;
									case 9:
										if (Helpers.randomRange(0, 30) < 5) {
											megamanX.player.changeWeaponSlot(0);
											megamanX.shoot(true);
										}
										break;
									case 10:
										if (megamanX.stockedXSaber) {
											megamanX.player.press(Control.Shoot);
										}
										break;
									case 11:
										if (megamanX.stockedCharge) {
											megamanX.player.press(Control.Shoot);
										}
										break;
									case 12:
										if (Helpers.randomRange(0, 30) < 5) {
											megamanX.player.release(Control.Shoot);
										}
										break;
								}
							}
							int UPXAttack = Helpers.randomRange(0, 5);
							//UP X section
							if (megamanX?.isHyperX == true) {
								switch (UPXAttack) {
									case 0:
										megamanX.player.press(Control.Special1);
										break;
									case 1:
										megamanX.player.press(Control.Shoot);
										megamanX.player.release(Control.Shoot);
										break;
									case 2:
										megamanX.player.release(Control.Special1);
										break;
									case 3:
										megamanX.player.press(Control.WeaponRight);
										megamanX.player.release(Control.WeaponRight);
										break;
									case 4:
										if (megamanX.charState is Dash or AirDash or Run) {
											megamanX.charState.isGrabbing = true;
											megamanX.changeSpriteFromName("unpo_grab_dash", true);
										}
										break;
									case 5:
										if (isTargetClose)
											megamanX.changeState(new X6SaberState(megamanX.grounded), true);
										break;
								}
							}
						}
					}
				}
				shootTime += Global.spf;
				if (shootTime > 0.1) {
					shootTime = 0;
				}
			}
		}

		//The AI should dodge if a projectile is close to him
		if (aiState.shouldDodge && target != null) {
			foreach (GameObject gameObject in Global.level.gameObjects) {
				if (gameObject is Projectile proj) {
					if (proj.damager.owner.alliance != player.alliance) {
						//Start of Zero
						//Putting Zero here
						if (player.character is Zero zero) {
							//Projectile is not 
							if (gameObject is not RollingShieldProjCharged || gameObject is not RollingShieldProj
							 || gameObject is not FrostShieldProj || gameObject is not FrostShieldProjAir || gameObject is not FrostShieldProjCharged || gameObject is not FrostShieldProjGround || gameObject is not FrostShieldProjPlatform //HOW MANY OF U EXIST
							 || gameObject is not MagnetMineProj) {
								// If a projectile is close to Zero
								if (character != null && proj.isFacing(character) &&
									character.withinX(proj, 100) && character.withinY(proj, 30) && !player.isDead && zero.charState.canAttack() && zero.sprite.name != null &&
									(zero.charState is not HyperZeroStart or LadderClimb or DarkHoldState or Hurt or Frozen or Crystalized or Die or WarpIn or WarpOut or WallSlide or WallKick or SwordBlock) 
								) {
									// If the player is Z-Saber or Knuckle
									// and has giga attack ammo available do "I hate the ground" Or "Block/Parry"
									if (player.isZSaber() || player.hasKnuckle()) {
										//Do i have giga attack ammo available?
										if (zero.zeroGigaAttackWeapon.ammo >= 8f && zero.grounded) {
											//RAKUHOUHA!
											player.press(Control.Special1);
											 player.press(Control.Down);
										}
										//If he hasn't do "Block/Parry"
										else if (gameObject is not SwordBlock) {
											if(gameObject is not GenericMeleeProj && player.isZSaber() && !player.hasKnuckle()) {
												zero.turnToInput(player.input, player);
												zero.changeState(new SwordBlock());
											}
											if(!player.isZSaber() && player.hasKnuckle()) {
												zero.turnToInput(player.input, player);
												zero.changeState(new KKnuckleParryStartState());
											}
										}
									}
									//If player is Buster Zero do Saber Swing
									if (player.isZBusterZero())
										player.press(Control.Special1);
								}
							}
							//A magnet mine?
						/*	else if (gameObject is MagnetMineProj) {
								//if the projectile is super close to Zero
								if (character != null && proj.isFacing(character) &&
									character.withinX(proj, 15) && character.withinY(proj, 1)
								) {
									//If the character is on the ground (and is not knuckle or Buster Zero)
									if (player.isZSaber() && zero.grounded &&
										!(player.hasKnuckle() || player.isZBusterZero())
									) {
										//CrouchSlash
										player.press(Control.Down); player.press(Control.Shoot);
									}
									//If the character is on air
									else if (!zero.grounded) {
										//Air Dash
										player.press(Control.Dash);
									}
									//If the character is on the air (and is not knuckle or Buster Zero)
									else if (player.isZSaber() && zero.charState is Jump &&
										!(player.hasKnuckle() || player.isZBusterZero())
									) {
										//Kuuenzan
										player.press(Control.Special1);
									}
								}
							}
						*/
						}
						//End of Zero

						//Start Of Sigma
						//Putting Sigma here
						if (player.character is BaseSigma baseSigma) {
							//If a projectile is close to Sigma
							if (character != null && proj.isFacing(character) &&
								character.withinX(proj, 150) && character.withinY(proj, 30)
							) {
								//Commander Sigma
								if (character is CmdSigma cmdSigma) {
									if (gameObject is not GenericMeleeProj) {
										//Do Block
										player.press(Control.Down);
									}
								}
								//Neo Sigma
								if (character is NeoSigma neoSigma) {
									// If Neo Sigma giga attack ammo is the same and higher than 16 but less than 24
									if (player.sigmaAmmo >= 16 && player.sigmaAmmo <= 24) {
										if (Global.time > 0.3f) {
											//Do "Better C-Flasher" 
											//Original name: 5 Bullet Shot (弾5発射 Dan 5 Hassha)
											player.press(Control.Special1);
										}
									}
									// If Neo Sigma giga attack ammo is 32
									else if (player.sigmaAmmo == 32) {
										if (Global.time > 0.3f) {
											//Do "I-Frames E-Spark move"
											//Original name: Electromagnetic Wave (電磁波 Denjiha) - Nightshade Electric Spark
											player.press(Control.Special1);
										}
									}
									// If Neo Sigma has giga attack ammo less than 16
									else if (player.sigmaAmmo < 16) {
										if (gameObject is not GenericMeleeProj) {
											//Do "Block"
											player.press(Control.Down);
										}
									}
								}
							}
						}
						//Doppma shouldn't get an AI to block something
						//dude literally holds the best shield.
						//End of Sigma

						//Start of Axl
						//Putting Axl
						if (player.character is Axl axl) {
							if (character != null && proj.isFacing(character) &&
								character.withinX(proj, 150) && character.withinY(proj, 30)) {
								//Dodge Roll if your DodgeRollCooldown is on 0 and you are not in Dodgeroll State, also if you are on ground and can dash.
								//and we count that you are not on hurt, die, frozen, crystalized, stunned, or grabbed state.. :skull:
								if (axl.grounded && axl.canDash() && !(axl.charState is DodgeRoll) && axl.dodgeRollCooldown == 0 && (axl.player.axlWeapon.isTwoHanded(true) || axl.isZooming())
									 && !(axl.charState is Hurt or Die or Frozen or Crystalized or Stunned or HyperAxlStart or WarpIn or Hover) && axl?.charState?.isGrabbedState == false) {
									axl.changeState(new DodgeRoll());
								}
								//Use Airblast If Axl has flameburner as his weapon and has air blast as its alt, as the projectile is not Melee and is reflectable by airblast (why this exists?), which its ammo should be higher than 0
								else if (axl?.player.weapon is FlameBurner && axl.player.axlLoadout.flameBurnerAlt == 1 && (gameObject is not GenericMeleeProj || (proj.reflectableFBurner == true)) && axl.player.weapon.ammo > 0) {
									player.press(Control.Special1);
								} else { doJump(0.75f); }
								//Or just Jump
							}
						}
						//End of Axl

						//Start of X
						//Putting X 
						if (player.character is MegamanX X) {
							if (character != null && proj.isFacing(character) &&
								character.withinX(proj, 60) && character.withinY(proj, 30)) {

								if (player.hasArmor() || !player.hasArmor() && !X.isHyperX) {
									if (X.player.weapon.ammo > 0) {
										player.changeWeaponSlot(3);
										player.press(Control.Shoot);
									}
								}
								int novaStrikeSlot = player.weapons.FindIndex(w => w is NovaStrike);
								if (player.hasUltimateArmor()) {
									X.player.changeWeaponSlot(novaStrikeSlot);
									if (X.player.weapon.ammo >= 16) {
										X.player.press(Control.Shoot);
									} else { X.player.changeWeaponSlot(getRandomWeaponIndex()); }
								}
								if (X.isHyperX) {
									if (X.unpoAbsorbedProj != null) {
										X.changeState(new XUPParryProjState(X.unpoAbsorbedProj, true, false), true);
										player.press(Control.WeaponLeft); player.release(Control.WeaponLeft);
										X.unpoAbsorbedProj = null;
										return;
									} else { X.changeState(new XUPParryStartState(), true); player.press(Control.WeaponLeft); player.release(Control.WeaponLeft); }
								}
							}
						}
					}
				}
			}
		}
		//End of The AI Dodging

		//The AI should randomly charge weapon?
		//I truly wonder why GM19 made only X charge weapons	
		if (aiState.randomlyChargeWeapon && (player.isX || player.isZBusterZero() || player.isZSaber() || player.isAxl) && framesChargeHeld == 0 && player.character.canCharge()) {
			if (Helpers.randomRange(0, 20) < 1) {
				if (player.isZBusterZero()) {
					maxChargeTime = 5f;
				}
				if (player.isZSaber()) {
					if (Helpers.randomRange(0, 50) < 1) {
						maxChargeTime = 4.25f;
					}
				}
				if (player.isAxl) {
					if (player.weapon is AxlBullet || player.weapon is DoubleBullet) {
						character?.increaseCharge();
					}
				} else {
					maxChargeTime = 4.25f;
				}
				framesChargeHeld = 1;
				player.press(Control.Shoot);
			}
		}
		//End of Randomly Charge Weapon

		if (aiState.randomlyChangeState && character != null) {
			if (Helpers.randomRange(0, 100) < 1) {
				var randAmount = Helpers.randomRange(-100, 100);
				changeState(new MoveToPos(character, character.pos.addxy(randAmount, 0)));
				return;
			}
		}
		if (aiState.randomlyDash && character?.charState is not WallKick && !inNodeTransition && stuckTime == 0) {
			if (Helpers.randomRange(0, 75) < 5) {
				dashTime = Helpers.randomRange(0.3f, 0.5f);
			}
			if (dashTime > 0) {
				player.press(Control.Dash);
				dashTime -= Global.spf;
				if (dashTime < 0) dashTime = 0;
			}
		}
		if (aiState.randomlyJump && !inNodeTransition && stuckTime == 0) {
			int max = player.isX ? 150 : 600;
			if (Helpers.randomRange(0, max) < 5) {
				jumpTime = Helpers.randomRange(0.25f, 0.75f);
			}
		}
		if (aiState.randomlyChangeWeapon &&
			(player.isX || player.isAxl || player.isVile) &&
			!player.lockWeapon && character?.isInvisibleBS.getValue() != true &&
			(character as MegamanX)?.chargedRollingShieldProj == null
		) {
			weaponTime += Global.spf;
			if (weaponTime > 5) {
				weaponTime = 0;
				var wasBuster = (player.weapon is Buster or AxlBullet);
				player.changeWeaponSlot(getRandomWeaponIndex());
				if (wasBuster && maxChargeTime > 0) {
					maxChargeTime = 4.25f;
				}
			}
		}
		if (player.weapon != null && player.weapon.ammo <= 0 && player.weapon is not Buster or AxlBullet) {
			player.changeWeaponSlot(getRandomWeaponIndex());
		}
		if (player.vileAmmo <= 0 && player.weapon is not VileCannon) { player.changeWeaponSlot(getRandomWeaponIndex()); }

		aiState.update();

		if (jumpTime > 0) {
			jumpTime -= Global.spf;
			if (jumpTime < 0) {
				jumpTime = 0;
			}
		}
	}

	public int getRandomWeaponIndex() {
		if (player.weapons.Count == 0) return 0;
		List<Weapon> weapons = player.weapons.FindAll(w => w is not DNACore or IceGattling or BlastLauncher).ToList();
		return weapons.IndexOf(weapons.GetRandomItem());                                         // removing IceGattling until know the bug
	}

	public void changeState(AIState newState, bool forceChange = false) {
		if (aiState is FindPlayer && newState is not FindPlayer && character.flag != null) {
			return;
		}
		if (flagger && aiState is FindPlayer && newState is not FindPlayer && Global.level.gameMode is CTF) {
			return;
		}
		if (aiState is FindPlayer && newState is not FindPlayer && Global.level.gameMode is Race) {
			return;
		}
		if (forceChange || newState.canChangeTo()) {
			aiState = newState;
		}
	}

	public float getMaxDist() {
		var maxDist = Global.screenW / 2;
		if (player.isZero || player.isSigma) return 100;
		int? raNum = player.character?.rideArmor?.raNum;
		if (raNum != null && raNum != 2) maxDist = 35;
		return maxDist;
	}
}

public class AIState {
	public bool facePlayer;
	public Character character;
	public bool shouldAttack;
	public bool shouldDodge;
	public bool randomlyChangeState;
	public bool randomlyDash;
	public bool randomlyJump;
	public bool randomlyChangeWeapon;
	public bool randomlyChargeWeapon;

	public Player player {
		get {
			return character.player;
		}
	}

	public AI ai {
		get {
			if (player.character != null) {
				return player.character.ai;
			} else if (player.limboChar != null) {
				return player.limboChar.ai;
			} else {
				return new AI(character);
			}
		}
	}

	public Actor? target {
		get {
			return ai?.target;
		}
	}

	public string getPrevNodeName() {
		if (this is FindPlayer findPlayer) {
			return findPlayer.prevNode?.name ?? "";
		}
		return "";
	}

	public string getNextNodeName() {
		if (this is FindPlayer findPlayer) {
			return findPlayer.nextNode?.name ?? "";
		}
		return "";
	}

	public string getDestNodeName() {
		if (this is FindPlayer findPlayer) {
			return findPlayer.destNode?.name ?? "";
		}
		return "";
	}

	public bool canChangeTo() {
		return character.charState is not LadderClimb && character.charState is not LadderEnd;
	}

	public AIState(Character character) {
		this.character = character;
		shouldAttack = true;
		facePlayer = true;
		shouldDodge = true;
		randomlyChangeState = true;
		randomlyDash = true;
		randomlyJump = true;
		randomlyChangeWeapon = true;
		randomlyChargeWeapon = true;
	}

	public virtual void update() {
		if (character.charState is LadderClimb && this is not FindPlayer) {
			player.press(Control.Down);
			player.press(Control.Jump);
		}
	}
}

public class MoveTowardsTarget : AIState {
	public MoveTowardsTarget(Character character) : base(character) {
		facePlayer = true;
		shouldAttack = true;
		shouldDodge = true;
		randomlyChangeState = false;
		randomlyDash = true;
		randomlyJump = true;
		randomlyChangeWeapon = false;
		randomlyChargeWeapon = true;
	}

	public override void update() {
		base.update();
		if (ai.target == null) {
			ai.changeState(new FindPlayer(character));
			return;
		}

		if (character.pos.x - ai.target.pos.x > ai.getMaxDist()) {
			player.press(Control.Left);
		} else if (character.pos.x - ai.target.pos.x < -ai.getMaxDist()) {
			player.press(Control.Right);
		} else {
			ai.changeState(new AimAtPlayer(character));
		}
	}
}

public class FindPlayer : AIState {
	public NavMeshNode? destNode;
	public NavMeshNode? nextNode;
	public NavMeshNode? prevNode;
	public NavMeshNeighbor? neighbor;
	public NodeTransition? nodeTransition;
	public List<NavMeshNode> nodePath = new();
	public float stuckTime;
	public float lastX;
	public float runIntoWallTime;
	public FindPlayer(Character character) : base(character) {
		facePlayer = true;
		shouldAttack = true;
		shouldDodge = true;
		randomlyChangeState = false;
		randomlyDash = true;
		randomlyJump = true;
		randomlyChangeWeapon = false;
		randomlyChargeWeapon = true;

		setDestNodePos();
	}

	public override void update() {
		base.update();

		if (nextNode == null) {
			ai.changeState(new FindPlayer(character));
			return;
		}

		if (nodeTransition != null) {
			nodeTransition.update();
			if (nodeTransition.failed) {
				ai.changeState(new FindPlayer(character));
				return;
			} else if (!nodeTransition.completed) {
				return;
			}
		}

		float xDist = character.pos.x - nextNode.pos.x;
		if (MathF.Abs(xDist) > 2.5f) {
			if (xDist < 0) {
				player.press(Control.Right);
			} else if (xDist > 0) {
				player.press(Control.Left);
			}
			if (character.pos.x == lastX && character.grounded) {
				runIntoWallTime += Global.spf;
				if (runIntoWallTime > 2) {
					setDestNodePos();
				}
			}
			lastX = character.pos.x;
		} else {
			// States where it's possible to move to the next node. As more special situations are added this may need to grow
			bool isValidTransitionState = character.grounded || neighbor?.isDestNodeInAir == true || character.charState is LadderClimb;

			if (Math.Abs(character.abstractedActor().pos.y - nextNode.pos.y) < 30 && isValidTransitionState) {
				goToNextNode();
			} else {
				stuckTime += Global.spf;
				if (stuckTime > 2) {
					setDestNodePos();
				}
			}
		}
	}
	public void goToNextNode() {
		if (nextNode == destNode) {
			setDestNodePos();
		} else {
			prevNode = nextNode;
			nextNode = nodePath.PopFirst();
		}
		if (nextNode != null) {
			neighbor = prevNode?.getNeighbor(nextNode);
		}
		if (neighbor != null) {
			var phases = neighbor.getNodeTransitionPhases(this);
			if (phases.Count > 0) {
				nodeTransition = new NodeTransition(phases);
			} else {
				nodeTransition = null;
			}
		}
	}

	public void setDestNodePos() {
		runIntoWallTime = 0;
		stuckTime = 0;
		if (Global.level.gameMode is Race) {
			destNode = Global.level.goalNode;
		} else if (Global.level.gameMode is CTF && player.alliance < 1) {
			if (character.flag == null) {
				Flag targetFlag = Global.level.blueFlag;
				if (player.alliance == GameMode.redAlliance) targetFlag = Global.level.blueFlag;
				else if (player.alliance == GameMode.blueAlliance) targetFlag = Global.level.redFlag;
				if (targetFlag != null) {
					destNode = Global.level.getClosestNodeInSight(targetFlag.pos);
				}
				destNode ??= Global.level.getRandomNode();
			} else {
				if (player.alliance == GameMode.blueAlliance) destNode = Global.level.blueFlagNode;
				else if (player.alliance == GameMode.redAlliance) destNode = Global.level.redFlagNode;
			}
		} else if (Global.level.gameMode is ControlPoints) {
			var cp = Global.level.getCurrentControlPoint();
			if (cp == null) {
				destNode = Global.level.getRandomNode();
			} else {
				destNode = cp.navMeshNode;
			}
		} else if (Global.level.gameMode is KingOfTheHill) {
			var cp = Global.level.hill;
			destNode = cp.navMeshNode;
		} else {
			destNode = Global.level.getRandomNode();
		}
		if (Global.level.navMeshNodes.Count == 2) {
			nextNode = destNode;
		} else {
			nextNode = Global.level.getClosestNodeInSight(character.getCenterPos());
		}
		prevNode = null;

		if (nextNode != null) {
			if (destNode != null) {
				nodePath = nextNode.getNodePath(destNode);
			}
			nodePath.Remove(nextNode);
		}
	}
}

public class MoveToPos : AIState {
	public Point dest;
	public MoveToPos(Character character, Point dest) : base(character) {
		this.dest = dest;
		facePlayer = false;
		randomlyChangeState = false;
		randomlyChargeWeapon = true;
	}

	public override void update() {
		base.update();
		var dir = 0;
		if (character.pos.x - dest.x > 5) {
			dir = -1;
			player.press(Control.Left);
		} else if (character.pos.x - dest.x < -5) {
			dir = 1;
			player.press(Control.Right);
		} else {
			ai.changeState(new AimAtPlayer(character));
		}

		if (character.sweepTest(new Point(dir * 5, 0)) != null) {
			ai.changeState(new AimAtPlayer(character));
		}
	}
}

public class AimAtPlayer : AIState {
	public float jumpDelay = 0;
	public AimAtPlayer(Character character) : base(character) {
	}

	public override void update() {
		base.update();
		if (character.grounded && jumpDelay > 0.3) {
			jumpDelay = 0;
		}

		if (target != null && character.pos.y > target.pos.y && character.pos.y < target.pos.y + 80) {
			jumpDelay += Global.spf;
			if (jumpDelay > 0.3) {
				ai.doJump();
			}
		} else {
			//this.changeState(new JumpToWall());
		}
	}
}

public class InJumpZone : AIState {
	public JumpZone jumpZone;
	public int jumpZoneDir;
	public float time = 0.25f;

	public InJumpZone(Character character, JumpZone jumpZone, int jumpZoneDir) : base(character) {
		this.jumpZone = jumpZone;
		this.jumpZoneDir = jumpZoneDir;
		facePlayer = false;
		shouldAttack = false;
		shouldDodge = false;
		randomlyChangeState = false;
		randomlyDash = true;
		randomlyJump = false;
		randomlyChangeWeapon = false;
		randomlyChargeWeapon = true;
	}

	public override void update() {
		base.update();
		time += Global.spf;
		ai.doJump();
		ai.jumpZoneTime += Global.spf;

		if (jumpZoneDir == -1) {
			player.press(Control.Left);
		} else if (jumpZoneDir == 1) {
			player.press(Control.Right);
		}

		//Check if out of zone
		if (character != null && character.abstractedActor().collider != null) {
			if (!character.abstractedActor().collider.isCollidingWith(jumpZone.collider)) {
				ai.changeState(new FindPlayer(character));
			}
		}
	}
}
