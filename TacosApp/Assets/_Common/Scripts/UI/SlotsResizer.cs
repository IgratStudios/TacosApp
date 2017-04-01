using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotsResizer : MonoBehaviour 
{
	public bool _mustShowDebugInfo = false;
	public bool alreadyRecalculated = false;
	public GridLayoutGroup grid;
	public RectTransform rectReference;
	public bool useReferenceWidth = true;
	public float slotPercentSize = 0.4f;
	public float slotAspectRatioOneTo = 1;
	public float maxVisibleElementsAlongConstrainedAxis = 3;
	public float minimumXSpacing = 20;
	public float minimumYSpacing = 20;

	void OnEnable()
	{
		if(alreadyRecalculated)
			return;
		if(grid != null && rectReference != null)
		{
			float w = rectReference.rect.size.x;
			float h = rectReference.rect.size.y;
			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("ON ENABLE CALLED! Rect["+w+"]["+h+"] of Screen["+Screen.width+"]["+Screen.height+"]" +
				"SlotsPercent["+slotPercentSize+"] SlotsConstrain["+grid.constraintCount+"]");
			}
			float slotWidth = w;
			float slotHeight = h;
			float spacingX = 1;
			float spacingY = 1;
			float maxSpacingX = w;
			float maxSpacingY = h;

			if(useReferenceWidth)
			{
				slotWidth *= slotPercentSize;
				slotHeight = slotWidth*slotAspectRatioOneTo;
				if(grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
				{
					spacingX = w - slotWidth*grid.constraintCount;
				}
				else
				{
					spacingX = w - slotWidth*maxVisibleElementsAlongConstrainedAxis;
				}
				spacingY = spacingX;

				maxSpacingX = w - slotWidth;
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Height after slot percent["+slotWidth+"] SpacingX["+spacingX+"]");
				}
			}
			else
			{
				slotHeight *= slotPercentSize;
				slotWidth = slotHeight*slotAspectRatioOneTo;

				if(grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
				{
					spacingY = h - slotHeight*grid.constraintCount;
				}
				else
				{
					spacingY = h - slotHeight*maxVisibleElementsAlongConstrainedAxis;
				}

				spacingX = spacingY;

				maxSpacingY = h - slotHeight;
				if(_mustShowDebugInfo)
				{
					Debug.LogWarning("Height after slot percent["+slotHeight+"] SpacingY["+spacingY+"]");
				}

			}
			if(spacingX < minimumXSpacing)
			{
				spacingX = minimumXSpacing;
			}
			if(spacingX > maxSpacingX)
			{
				spacingX = maxSpacingX;
			}
			if(spacingY < minimumYSpacing)
			{
				spacingY = minimumYSpacing;
			}
			if(spacingY > maxSpacingY)
			{
				spacingY = maxSpacingY;
			}

			if(_mustShowDebugInfo)
			{
				Debug.LogWarning("Slot Resizer CellSize["+slotWidth+"]["+slotHeight+"] Spacing["+spacingX+"]["+spacingY+"]");
			}
			grid.cellSize = new Vector2(slotWidth,slotHeight);
			grid.spacing = new Vector2(spacingX,spacingY);
			Canvas.ForceUpdateCanvases();
			alreadyRecalculated = true;
		}
	}

}
