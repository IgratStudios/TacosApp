using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyDataChart : MonoBehaviour 
{

	public string pieChartGraphicElementPoolId = "PieChartElement";
	public string pieChartDataLabelPoolId = "ChartInfoElement2Label";

	public Transform labelElementsHolder;

	public List<ChartData> allDatas = new List<ChartData>();
	public List<ChartInfoLabel> allDataLabels = new List<ChartInfoLabel>();


	public void UpdateGraph(float[] dataSamples,string[] dataNames,Color[] dataColors)
	{
		if(dataSamples.Length != dataNames.Length || dataSamples.Length > dataColors.Length)
		{
			return;
		}

		float totalSamples = 0;
		for(int i = 0; i < dataSamples.Length; i++)
		{
			totalSamples += dataSamples[i];
		}

		for(int i = 0; i < dataSamples.Length; i++ )
		{
			ChartData data = ObjectManager.SpawnLike<ChartData>(pieChartGraphicElementPoolId,
				Vector3.zero,Quaternion.identity,labelElementsHolder);
			if(data != null)
			{
				ChartInfoLabel label = ObjectManager.SpawnLike<ChartInfoLabel>(pieChartDataLabelPoolId,
					Vector3.zero,Quaternion.identity,labelElementsHolder);

				data.CachedRectTransform.ResetToFillParent();
				data.CachedGameObject.SetActive(false);

				data.InitWithData(CHART_DATA_TYPE.ONLY_LABELS,
					dataNames[i],
					dataColors[i],
					dataSamples[i],
					totalSamples,
					label);

				allDatas.Add( data );

				if(label != null)
				{
					label.CachedRectTransform.ResetToFillParent();
					allDataLabels.Add(label);
				}
			}
		}

		//sort datas by percent(from less to more)
		allDatas.Sort(delegate(ChartData x, ChartData y) 
			{
				return x.dataPercent.CompareTo(y.dataPercent);
			});

		float accumulatedData = 0.0f;
		for(int i = 0; i < allDatas.Count; i++)
		{
			allDatas[i].UpdateGraph(accumulatedData);
			accumulatedData += allDatas[i].dataPercent;
		}

	}

	public void Clear()
	{
		for(int i = 0; i < allDatas.Count; i++)
		{
			allDatas[i].Despawn();
		}
		allDatas.Clear();

		for(int i = 0; i < allDataLabels.Count; i++)
		{
			allDataLabels[i].Despawn();
		}
		allDataLabels.Clear();
	}

}
