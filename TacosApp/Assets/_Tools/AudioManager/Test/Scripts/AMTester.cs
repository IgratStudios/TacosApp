using UnityEngine;
using System.Collections;

public class AMTester : MonoBehaviour 
{
	public string[] musicIds;
	public int musicIndex = 0;
	public string[] sfxIds;
	public int sfxIndex = 0;
	public string playingMusicId = string.Empty;
	public string loopSFXId = "Wolf";

	public void StopPlaylist()
	{
		AudioManager.GetInstance().UnregisterToItemStopped(playingMusicId,OnAudioStopped);
		AudioManager.GetInstance().StopAllAudiosInCategory("MUSIC");

	}

	public void PlayPlaylist()
	{
		AudioManager.GetInstance().RegisterToItemStart(musicIds[musicIndex],OnAudioStarted);
		AudioManager.GetInstance().RegisterToItemStopped(musicIds[musicIndex],OnAudioStopped);
		AudioManager.GetInstance().Play( musicIds[musicIndex] );
		AdvanceMusicIndex();
	}

	public void AdvanceMusicIndex()
	{
		musicIndex++;
		if(musicIndex >= musicIds.Length)//loop
		{
			musicIndex = 0;
		}
	}

	private void OnAudioStopped(string id)
	{
		Debug.Log("OnAudioStoppedEvent from["+id+"]");
		AudioManager.GetInstance().UnregisterToItemStopped(id,OnAudioStopped);
		PlayPlaylist();
	}

	private void OnAudioStarted(string id)
	{
		Debug.Log("OnAudioStarteddEvent from["+id+"]");
		AudioManager.GetInstance().UnregisterToItemStart(id,OnAudioStarted);
		playingMusicId = id;
	}

	public void PlayLoopedMusic()
	{
		AudioManager.GetInstance().Play( "L_"+musicIds[musicIndex] );
	}

	public void StopLoopedMusic()
	{
		AudioManager.GetInstance().StopAllAudiosInCategory("LOOPEDMUSIC");
	}

	public void ChangeLoopedMusic()
	{
		AdvanceMusicIndex();
		PlayLoopedMusic();
	}

	public void PlaySFX()
	{
		AudioManager.GetInstance().Play(sfxIds[sfxIndex]);
	}

	public void AdvanceSFXIndex()
	{
		sfxIndex++;
		if(sfxIndex >= sfxIds.Length)//loop
		{
			sfxIndex = 0;
		}
	}

	public void PlayLoopedSFX()
	{
		AudioManager.GetInstance().Play(loopSFXId);
	}

	public void StopLoopedSFX()
	{
		AudioManager.GetInstance().Stop(loopSFXId);
	}
}
