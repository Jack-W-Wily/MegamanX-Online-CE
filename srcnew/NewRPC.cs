using System;
using System.Collections.Generic;
using Lidgren.Network;
using System.Linq;

namespace MMXOnline;

public class RPC_CreditKillOther : RPC {
	public RPC_CreditKillOther() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public override void invoke(params byte[] arguments) {
		int i = 0;
		byte type = arguments[i++];
		byte extraId = arguments[i++];
		byte scrapReward = arguments[i++];
		byte victimId = arguments[i++];
		byte killerId = arguments[i++];
		byte assisterId = arguments[i++];
		int? weaponIndex = null;
		if (arguments.Length - 1 == i) {
			weaponIndex = arguments[i++];
		}

		
	}

	public void sendRpc(
		byte type, byte extraId, byte scrapReward,
		Player victim, Player killer, Player assister, int? weaponIndex
	) {
		if (killer != null && victim != null) {
			byte assisterId = ((assister == null) ? byte.MaxValue : ((byte)assister.id));

			List<byte> bytesToAdd = new List<byte> {
				type,
				extraId,
				scrapReward,
				(byte)victim.id,
				(byte)killer.id,
				assisterId
			};
			if (weaponIndex.HasValue) {
				bytesToAdd.Add((byte)weaponIndex.Value);
			}

			if (Global.serverClient != null) {
				RPC.hdmCustomRPC.sendRpc(1, bytesToAdd.ToArray());
			}
		}
	}
}

public class RPC_GiveMaverickAmmo : RPC {
	public RPC_GiveMaverickAmmo() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public override void invoke(params byte[] arguments) {
		ushort maverickId = BitConverter.ToUInt16(new byte[2] { arguments[0], arguments[1] }, 0 );
		byte byteAmmo = arguments[2];

		Actor target = Global.level.getActorByNetId(maverickId);
		if (target != null && target is Maverick maverick) {
			maverick.ammo += byteAmmo;
			if (maverick.ammo > maverick.maxAmmo) {
				maverick.ammo = maverick.maxAmmo;
			}
		}
	}

	public void sendRpc(float ammoHeal, ushort gatorId) {
		byte byteAmmo = 1;
		if (ammoHeal > 200) {
			byteAmmo = 200;
		} else if (ammoHeal >= 1) {
			byteAmmo = (byte)MathF.Ceiling(ammoHeal);
		}

		byte[] gatorObjId = BitConverter.GetBytes(gatorId);
		byte[] gatorRpcData = {
			gatorObjId[0],
			gatorObjId[1],
			byteAmmo
		};
		RPC.hdmCustomRPC.sendRpc(2, gatorRpcData);
	}
}

public class HdmCustomRPC : RPC {
	public HdmCustomRPC() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public override void invoke(params byte[] arguments) {
		byte type = arguments[0];
		byte[] finalArguments = arguments[1..];

		switch (type) {
			case 0:
				RPC.feedWheelGator.invoke(finalArguments);
				break;
			case 1:
				RPC.creditKillOther.invoke(finalArguments);
				break;
			case 2:
				RPC.giveMaverickAmmo.invoke(finalArguments);
				break;
			case 3:
				RPC.updateHdmPlayer.invoke(finalArguments);
				break;
			case 4:
				RPC.grabMaverick.invoke(finalArguments);
				break;	
		}
	}

	public void sendRpc(byte type, byte[] arguments) {
		byte[] sendValues = new byte[arguments.Length + 1];
		sendValues[0] = type;
		Array.Copy(arguments, 0, sendValues, 1, arguments.Length);

		if (Global.serverClient != null) {
			Global.serverClient.rpc(RPC.hdmCustomRPC, sendValues);
		}
	}
}

public class RPC_UpdateHdmPlayer : RPC {
	public RPC_UpdateHdmPlayer() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public override void invoke(params byte[] arguments) {
		byte id = arguments[0];
		byte assists = arguments[1];
		bool nonSpectator = (arguments[2] == 1 ? true : false);

		Player player = Global.level.getPlayerById(id);

		if (player == null) {
			return;
		}
		player.assists = assists;
	}

	public void sendRpc(int id, int assists, bool isNonSpectatorOverride) {
		byte[] playerRpcData = {
			(byte)id,
			(byte)assists,
			isNonSpectatorOverride ? (byte)1 : (byte)0,
		};
		RPC.hdmCustomRPC.sendRpc(3, playerRpcData);
	}
}

public class RPC_maverickGrabMaverick : RPC {
	public RPC_maverickGrabMaverick() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public void sendRpc(
		ushort? grabberNetId, ushort? victimCharNetId,
		CommandGrabScenario hookScenario, bool isDefenderFavored, int projId
	) {
		if (victimCharNetId.HasValue) {
			byte[] grabberNetIdBytes = BitConverter.GetBytes(grabberNetId.Value);
			byte[] victimNetIdBytes = BitConverter.GetBytes(victimCharNetId.Value);
			byte[] projIdIdBytes = BitConverter.GetBytes(projId);
			byte[] grabRpcData = {
				grabberNetIdBytes[0],
				grabberNetIdBytes[1],
				victimNetIdBytes[0],
				victimNetIdBytes[1],
				(byte)hookScenario,
				Helpers.boolToByte(isDefenderFavored),
				projIdIdBytes[0],
				projIdIdBytes[1]
			};
			RPC.hdmCustomRPC.sendRpc(4, grabRpcData);
		}
	}

	public override void invoke(params byte[] arguments) {
		ushort grabberNetId = BitConverter.ToUInt16(
			new byte[2] {
				arguments[0],
				arguments[1]
			}, 0
		);
		ushort victimNetId = BitConverter.ToUInt16(
			new byte[2] {
				arguments[2],
				arguments[3]
			}, 0
		);
		CommandGrabScenario hookScenario = (CommandGrabScenario)arguments[4];
		bool isDefenderFavored = Helpers.byteToBool(arguments[5]);
		int projId = BitConverter.ToUInt16(new byte[] {arguments[6], arguments[7]});
		Actor grabber = Global.level.getActorByNetId(grabberNetId);
		if (grabber == null) {
			return;
		}
		Actor victim = Global.level.getActorByNetId(victimNetId);
		if (victim == null) {
			return;
		}
		Character grabberChar = grabber as Character;
		Maverick grabberMaverick = grabber as Maverick;
		Maverick victimChar = victim as Maverick;
		switch (hookScenario) {
			case CommandGrabScenario.StrikeChain:
				break;
	/*		case CommandGrabScenario.MK2Grab:
				if (grabberChar == null || victimChar == null ||
					!victimChar.canBeGrabbed(grabberChar.player.id, projId)
				) {
					break;
				}
				if (!isDefenderFavored) {
					if (victim.ownedByLocalPlayer && !(victimChar.state is MvrkVileMK2Grabbed)) {
						victimChar.changeState(new MvrkVileMK2Grabbed(grabberChar, projId));
					}
				} else if (grabberChar.ownedByLocalPlayer) {
					grabberChar.changeState(new VileMK2GrabState(victimChar));
				}
				break;
			case CommandGrabScenario.UPGrab:
				if (grabberChar == null || victimChar == null ||
					!victimChar.canBeGrabbed(grabberChar.player.id, projId)
				) {
					break;
				}
				if (!isDefenderFavored) {
					if (victimChar.ownedByLocalPlayer && !(victimChar.state is MvrkUPXGrabbed)) {
						victimChar.changeState(new MvrkUPXGrabbed(grabberChar, projId));
					}
				} else if (grabberChar.ownedByLocalPlayer) {
					grabberChar.changeState(new XUPGrabState(victimChar));
				}
				break;
			case CommandGrabScenario.WhirlpoolGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkWhirlpoolGrabbed(grabber as LaunchOctopus, projId),
					isDefenderFavored, projId
				);
				break;
			case CommandGrabScenario.DeadLiftGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkDeadLiftGrabbed(grabber as BoomerKuwanger, projId),
					isDefenderFavored, projId
				);
				break;
			case CommandGrabScenario.WheelGGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkWheelGGrabbed(grabber as WheelGator, projId),
					isDefenderFavored, projId
				);
				break;
			case CommandGrabScenario.FStagGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkFStagGrabbed(grabber as FlameStag, projId),
					isDefenderFavored, projId,
					new FStagUppercutState(victimChar)
				);
				break;
			case CommandGrabScenario.MagnaCGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkMagnaCDrainGrabbed(grabber as MagnaCentipede, projId),
					isDefenderFavored, projId
				);
				break;
			case CommandGrabScenario.BeetleLiftGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkBeetleGrabbedState(grabber as GravityBeetle, projId),
					isDefenderFavored, projId
				);
				break;
			case CommandGrabScenario.CrushCGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkCrushCGrabbed(grabber as CrushCrawfish, projId),
					isDefenderFavored, projId,
					new CrushCGrabState(victimChar)
				);
				break;
			case CommandGrabScenario.BBuffaloGrab:
				maverickGrabCode(
					grabberMaverick, victimChar,
					new MvrkBBuffaloDragged(grabber as BlizzardBuffalo, projId),
					isDefenderFavored, projId
				);
				break;
			case CommandGrabScenario.Release:
				victimChar?.releaseGrab(null);
				break;
		*/
		}
	}


	
public class RPC_ShootTriad : RPC {
	public RPC_ShootTriad() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public override void invoke(params byte[] arguments) {
		byte id = arguments[0];
		byte type = arguments[1];
		ushort netProjId = BitConverter.ToUInt16(
			new byte[2] {
				arguments[2],
				arguments[3]
			}, 0
		);
		Player player = Global.level.getPlayerById(id);

	}

	public void sendRpc(int playerId, int type, ushort netProjId) {
		byte[] projIdBytes = BitConverter.GetBytes(netProjId);
		byte[] shootData = {
			(byte)playerId,
			(byte)type,
			projIdBytes[0],
			projIdBytes[1]
		};
		RPC.hdmCustomRPC.sendRpc(5, shootData);
	}
}

public class RPC_CreateEffect : RPC {
	public RPC_CreateEffect() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public override void invoke(params byte[] arguments) {
		int effectId = arguments[0];
		Player player = Global.level.getPlayerById(arguments[1]);
		byte[] extraArgs;
		if (arguments.Length > 2) {
			extraArgs = arguments[2..];
		} else {
			extraArgs = null;
		}

		if (effectId == (int)EffectId.GigaCrushStart) {
			if (player.character != null) {
				new GigaCrushEffect(player.character);
			}
		}
		else if (effectId == (int)EffectId.MegaCrushStart) {
			if (player.character != null) {
				new MegaCrushEffect(player.character);
			}
		}
	}

	public void sendRpc(EffectId effectId, Player player, byte[] extraData = null) {
		byte[] data = {
			(byte)effectId,
			(byte)player.id
		};
		if (extraData != null) {
			data.Concat(extraData).ToArray();
		}
		RPC.hdmCustomRPC.sendRpc(6, data);
	}
}

public enum EffectId {
	GigaCrushStart,
	MegaCrushStart
}


public class RPC_XSteal : RPC {
	public RPC_XSteal() {
		netDeliveryMethod = NetDeliveryMethod.ReliableOrdered;
	}

	public override void invoke(params byte[] arguments) {
		byte playerId = arguments[0];
		byte type = arguments[1];
		Character stealerChara = Global.level.getPlayerById(playerId)?.character;
		if (stealerChara == null || !stealerChara.player.isX) {
			return;
		}
		ushort targetNetId = BitConverter.ToUInt16(
			new byte[2] {
				arguments[2],
				arguments[3]
			}, 0
		);
		Actor target = Global.level.getActorByNetId(targetNetId);
		if (target == null) {
			return;
		}
		//XUPGrabState.stealWeapon(stealerChara, target);
	}
	}


	public void sendRpc(Player player, Actor target) {
		byte[] actorId = BitConverter.GetBytes(target.netId.Value);
		byte[] data = {
			(byte)player.id,
			(byte)actorId[0],
			(byte)actorId[1]
		};
		RPC.hdmCustomRPC.sendRpc(20, data);
	
	}
}