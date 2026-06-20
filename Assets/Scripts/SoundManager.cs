using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BGMType
{
    Title_BGM,
    InGame_BGM
}

public enum SFXType
{
    BossAppear_SFX,
    HitMonster_SFX,
    KillMonster_SFX,
    DeepSeaBase_SFX,
    GameClear_SFX,
    GetItem_SFX,
    BatteryHeal_SFX,
    BatteryWarning_SFX,
    HitPlayer_SFX,
    PlayerGameOver_SFX,
    Click_SFX,
    OpenMenu_SFX,
    UICancel_SFX,
    UIExit_SFX,
    WeaponShot_SFX
}

[Serializable]
public class BGMClipEntry
{
    public BGMType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

[Serializable]
public class SFXClipEntry
{
    public SFXType type;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    [Header("Clips")]
    [SerializeField] private BGMClipEntry[] bgmClips = CreateDefaultBGMEntries();
    [SerializeField] private SFXClipEntry[] sfxClips = CreateDefaultSFXEntries();

    private readonly Dictionary<BGMType, BGMClipEntry> bgmLookup = new Dictionary<BGMType, BGMClipEntry>();
    private readonly Dictionary<SFXType, SFXClipEntry> sfxLookup = new Dictionary<SFXType, SFXClipEntry>();
    private readonly Dictionary<SFXType, AudioSource> loopingSfxSources = new Dictionary<SFXType, AudioSource>();

    private void Reset()
    {
        EnsureAudioSources(true);
        EnsureDefaultEntries();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources(true);
        BuildLookups();
        ApplySourceVolumes();
    }

    private void OnValidate()
    {
        EnsureAudioSources(false);
        EnsureDefaultEntries();
        ApplySourceVolumes();

#if UNITY_EDITOR
        if (AutoAssignClipsInEditor())
        {
            EditorUtility.SetDirty(this);
        }
#endif
    }

    public void PlayBGM(BGMType type, bool restartIfSame = false)
    {
        if (!TryGetBGM(type, out BGMClipEntry entry))
        {
            return;
        }

        if (!restartIfSame && bgmSource.clip == entry.clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = entry.clip;
        bgmSource.loop = true;
        bgmSource.volume = masterVolume * bgmVolume * entry.volume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlaySFX(SFXType type)
    {
        if (!TryGetSFX(type, out SFXClipEntry entry))
        {
            return;
        }

        sfxSource.PlayOneShot(entry.clip, masterVolume * sfxVolume * entry.volume);
    }

    public void PlaySFXAtPoint(SFXType type, Vector3 position)
    {
        if (!TryGetSFX(type, out SFXClipEntry entry))
        {
            return;
        }

        AudioSource.PlayClipAtPoint(entry.clip, position, masterVolume * sfxVolume * entry.volume);
    }

    public void PlayLoopSFX(SFXType type)
    {
        if (!TryGetSFX(type, out SFXClipEntry entry))
        {
            return;
        }

        if (!loopingSfxSources.TryGetValue(type, out AudioSource source) || source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            loopingSfxSources[type] = source;
        }

        source.clip = entry.clip;
        source.volume = masterVolume * sfxVolume * entry.volume;

        if (!source.isPlaying)
        {
            source.Play();
        }
    }

    public void StopLoopSFX(SFXType type)
    {
        if (loopingSfxSources.TryGetValue(type, out AudioSource source) && source != null)
        {
            source.Stop();
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplySourceVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplySourceVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplySourceVolumes();
    }

    private bool TryGetBGM(BGMType type, out BGMClipEntry entry)
    {
        if (bgmLookup.Count == 0)
        {
            BuildLookups();
        }

        if (bgmLookup.TryGetValue(type, out entry) && entry.clip != null)
        {
            return true;
        }

        Debug.LogWarning($"BGM clip is missing: {type}");
        return false;
    }

    private bool TryGetSFX(SFXType type, out SFXClipEntry entry)
    {
        if (sfxLookup.Count == 0)
        {
            BuildLookups();
        }

        if (sfxLookup.TryGetValue(type, out entry) && entry.clip != null)
        {
            return true;
        }

        Debug.LogWarning($"SFX clip is missing: {type}");
        return false;
    }

    private void BuildLookups()
    {
        bgmLookup.Clear();
        sfxLookup.Clear();

        foreach (BGMClipEntry entry in bgmClips)
        {
            bgmLookup[entry.type] = entry;
        }

        foreach (SFXClipEntry entry in sfxClips)
        {
            sfxLookup[entry.type] = entry;
        }
    }

    private void EnsureAudioSources(bool allowAddComponent)
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (bgmSource == null)
        {
            bgmSource = sources.Length > 0 ? sources[0] : null;
        }

        if (sfxSource == null)
        {
            sfxSource = sources.Length > 1 ? sources[1] : null;
        }

        if (allowAddComponent && bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        if (allowAddComponent && sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (bgmSource == null || sfxSource == null)
        {
            return;
        }

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
    }

    private void ApplySourceVolumes()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }

        foreach (KeyValuePair<SFXType, AudioSource> pair in loopingSfxSources)
        {
            if (pair.Value != null)
            {
                pair.Value.volume = masterVolume * sfxVolume;
            }
        }
    }

    private void EnsureDefaultEntries()
    {
        if (bgmClips == null || bgmClips.Length != Enum.GetValues(typeof(BGMType)).Length)
        {
            bgmClips = CreateDefaultBGMEntries();
        }

        if (sfxClips == null || sfxClips.Length != Enum.GetValues(typeof(SFXType)).Length)
        {
            sfxClips = CreateDefaultSFXEntries();
        }
    }

    private static BGMClipEntry[] CreateDefaultBGMEntries()
    {
        BGMType[] values = (BGMType[])Enum.GetValues(typeof(BGMType));
        BGMClipEntry[] entries = new BGMClipEntry[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            entries[i] = new BGMClipEntry { type = values[i], volume = 1f };
        }

        return entries;
    }

    private static SFXClipEntry[] CreateDefaultSFXEntries()
    {
        SFXType[] values = (SFXType[])Enum.GetValues(typeof(SFXType));
        SFXClipEntry[] entries = new SFXClipEntry[values.Length];

        for (int i = 0; i < values.Length; i++)
        {
            entries[i] = new SFXClipEntry { type = values[i], volume = 1f };
        }

        return entries;
    }

#if UNITY_EDITOR
    private bool AutoAssignClipsInEditor()
    {
        bool changed = false;

        changed |= AssignClip(bgmClips, BGMType.Title_BGM, "Assets/Sounds/BGM/Title_BGM.mp3");
        changed |= AssignClip(bgmClips, BGMType.InGame_BGM, "Assets/Sounds/BGM/InGame_BGM.mp3");

        changed |= AssignClip(sfxClips, SFXType.BossAppear_SFX, "Assets/Sounds/SFX/Enemy/BossAppear_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.HitMonster_SFX, "Assets/Sounds/SFX/Enemy/HitMonster_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.KillMonster_SFX, "Assets/Sounds/SFX/Enemy/KillMonster_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.DeepSeaBase_SFX, "Assets/Sounds/SFX/Environment/DeepSeaBase_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.GameClear_SFX, "Assets/Sounds/SFX/Environment/GameClear_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.GetItem_SFX, "Assets/Sounds/SFX/item/GetItem_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.BatteryHeal_SFX, "Assets/Sounds/SFX/Player/BatteryHeal_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.BatteryWarning_SFX, "Assets/Sounds/SFX/Player/BatteryWarning_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.HitPlayer_SFX, "Assets/Sounds/SFX/Player/HitPlayer_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.PlayerGameOver_SFX, "Assets/Sounds/SFX/Player/PlayerGameOver_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.Click_SFX, "Assets/Sounds/SFX/UI/Click_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.OpenMenu_SFX, "Assets/Sounds/SFX/UI/OpenMenu_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.UICancel_SFX, "Assets/Sounds/SFX/UI/UICancel_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.UIExit_SFX, "Assets/Sounds/SFX/UI/UIExit_SFX.mp3");
        changed |= AssignClip(sfxClips, SFXType.WeaponShot_SFX, "Assets/Sounds/SFX/Weapon/WeaponShot_SFX.mp3");

        return changed;
    }

    private static bool AssignClip(BGMClipEntry[] entries, BGMType type, string path)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

        if (clip == null)
        {
            return false;
        }

        foreach (BGMClipEntry entry in entries)
        {
            if (entry.type == type && entry.clip == null)
            {
                entry.clip = clip;
                return true;
            }
        }

        return false;
    }

    private static bool AssignClip(SFXClipEntry[] entries, SFXType type, string path)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

        if (clip == null)
        {
            return false;
        }

        foreach (SFXClipEntry entry in entries)
        {
            if (entry.type == type && entry.clip == null)
            {
                entry.clip = clip;
                return true;
            }
        }

        return false;
    }
#endif
}
