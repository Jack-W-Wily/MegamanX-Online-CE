﻿namespace MMXOnline;

public class CTF : GameMode {
	int neutralKillLimit;

	public CTF(
		Level level, int playingTo,
		int? timeLimit, int neutralKillLimit = 100
	) : base(level, timeLimit, level.teamNum) {
		this.playingTo = playingTo;
		this.neutralKillLimit = neutralKillLimit;
		isTeamMode = true;
	}

	public override void render() {
		base.render();
		drawObjectiveNavpoint("Capture", level.mainPlayer.alliance == redAlliance ? level.blueFlag.pos : level.redFlag.pos);
		if (level.mainPlayer.character?.flag != null) {
			drawObjectiveNavpoint("Return", level.mainPlayer.alliance == redAlliance ? level.redFlag.pedestal.pos : level.blueFlag.pedestal.pos);
		} else {
			drawObjectiveNavpoint("Defend", level.mainPlayer.alliance == redAlliance ? level.redFlag.pos : level.blueFlag.pos);
		}
	}

	public override void drawTopHUD() {
		drawTeamTopHUD();
	}

	public override void checkIfWinLogic() {
		checkIfWinLogicTeams();
	}

	public override void drawScoreboard() {
		drawTeamScoreboard();
	}
}
