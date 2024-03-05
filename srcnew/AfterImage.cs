using System;
using System.Collections.Generic;
using System.Linq;

namespace MMXOnline;

public class AfterImageRenderer {
	public List<afterImage> afterImageList = new();

	public class afterImage {
		public string spriteName;
		public int frameNum;
		public float x;
		public float y;
		public int xDir;
		public int yDir;
		public float time;

		public afterImage(string spriteName, int frameNum, float x, float y, int xDir, int yDir) {
			this.spriteName = spriteName;
			this.frameNum = frameNum;
			this.x = x;
			this.y = y;
			this.xDir = xDir;
			this.yDir = yDir;
			this.time = 0;
		}
	}

	public int imageGap;

	public double maxImages;

	public float alpha;

	public int removeWait;

	public List<ShaderWrapper> shaderList;

	public int id;

	// Creation code.
	public AfterImageRenderer(int id, int imageGap, int maxImages, ShaderWrapper shader) {
		this.id = id;
		this.imageGap = imageGap;
		this.maxImages = maxImages;
		shaderList = new List<ShaderWrapper> { shader };
		alpha = 0;
	}

	public void updateParams(int id, int imageGap, int maxImages, ShaderWrapper shader) {
		this.id = id;
		this.imageGap = imageGap;
		this.maxImages = maxImages;
		shaderList = new List<ShaderWrapper> { shader };
		alpha = 0;
	}

	public void drawAfterImage(Actor actor, float x, float y) {
		// Add the current position for future rendering if needed.
		Point alignOffset = actor.sprite.getAlignOffset(actor.frameIndex, actor.xDir, actor.yDir);
		afterImage tempAfImg = new(
			actor.sprite.name, actor.sprite.frameIndex,
			actor.pos.x + x + (float)actor.xDir * actor.currentFrame.offset.x,
			actor.pos.y + y + (float)actor.yDir * actor.currentFrame.offset.y,
			actor.xDir, actor.yDir
		);
		if (tempAfImg.yDir == -1) {
			tempAfImg.y -= actor.reversedGravityOffset + 1;
		}
		afterImageList.Add(tempAfImg);

		if (afterImageList.Count > (maxImages * imageGap) + 1) {
			afterImageList.RemoveAt(0);
		}

		// Draw sprites
		for (int i = afterImageList.Count() - 1; i >= 0 ; i -= imageGap) {
			Global.sprites[afterImageList[i].spriteName].draw(
				afterImageList[i].frameNum,
				afterImageList[i].x, afterImageList[i].y,
				afterImageList[i].xDir, afterImageList[i].yDir,
				null, alpha,
				actor.xScale,
				actor.yScale,
				actor.zIndex - 1,
				shaderList
			);
		}
	}

	public void removeAfterImage(Actor actor, float x, float y) {
		removeWait++;
		if (removeWait >= 2) {
			afterImageList.RemoveAt(0);
			removeWait = 0;
		}
		// Draw sprites
		for (int i = afterImageList.Count() - 1; i >= 0 ; i -= imageGap) {
			Global.sprites[afterImageList[i].spriteName].draw(
				afterImageList[i].frameNum,
				afterImageList[i].x, afterImageList[i].y,
				afterImageList[i].xDir, afterImageList[i].yDir,
				null, alpha,
				actor.xScale,
				actor.yScale,
				actor.zIndex - 1,
				shaderList
			);
		}
	}

	public enum IDs {
		SpeedDevil,
		ShiningSpark,
		BlackZero,
		TwinPhantasm
	}
}