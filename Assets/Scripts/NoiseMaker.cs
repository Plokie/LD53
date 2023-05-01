using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseSound {
    public enum Type {
        None,
        SneakFootstep,
        WalkFootstep,
        RunFootstep,
        Explosion,
        Collision,
        Fall,
        PackageFall,
        Hit,
        Gunshot,
        LightEngine,
        HeavyEngine
    }
    public Type type = Type.None;
    public float radius = 2;
    public float volume = 0.5f;
    public Vector2 variance = Vector2.zero;
    public NoiseSound(Type type, float radius, float volume, Vector2 variance) {
        this.type = type;
        this.radius = radius;
        this.volume = volume;
        this.variance = variance;
    }
    public NoiseSound(Type type, float radius, float volume) {
        this.type = type;
        this.radius = radius;
        this.volume = volume;
    }

    public static Dictionary<Type, NoiseSound> defaultNoiseSoundType = new Dictionary<Type, NoiseSound>()
    {
        {Type.SneakFootstep, new NoiseSound(Type.SneakFootstep, 4, 0.8f, new Vector2(-0.5f,0.5f))},
        {Type.WalkFootstep, new NoiseSound(Type.WalkFootstep, 10, 1f, new Vector2(-0.5f,0.5f))},
        {Type.RunFootstep, new NoiseSound(Type.RunFootstep, 15, 1f,  new Vector2(-0.5f,0.5f))},
        {Type.Explosion, new NoiseSound(Type.Explosion, 55, 1f)},
        {Type.Collision, new NoiseSound(Type.Collision, 17.5f, 1f)},
        {Type.Fall, new NoiseSound(Type.Fall, 10, 0.9f)},
        {Type.PackageFall, new NoiseSound(Type.PackageFall, 9, 0.8f)},
        {Type.Hit, new NoiseSound(Type.Hit, 10, 1f)},
        {Type.Gunshot, new NoiseSound(Type.Gunshot, 30, 1f)},
        {Type.LightEngine, new NoiseSound(Type.LightEngine, 22, 0.4f)},
        {Type.HeavyEngine, new NoiseSound(Type.HeavyEngine, 30, 1f)},
    };
}

[RequireComponent(typeof(AudioSource))]
public class NoiseMaker : MonoBehaviour
{
    [SerializeField] bool drawNoiseRing;
    [SerializeField] GameObject ringPrefab;
    [SerializeField] AudioSource audioSource;
    public bool looping = false;
    public NoiseSound loopingSound;
    void Start() {
        if(!audioSource) audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
    }

    public void MakeNoise(NoiseSound.Type soundType) {
        MakeNoise(NoiseSound.defaultNoiseSoundType[soundType]);
    }
    public void MakeNoise(NoiseSound sound) {
        looping = false;
        audioSource.loop = false;
        if(drawNoiseRing) {
            Ring ring = Instantiate(ringPrefab, transform.position, Quaternion.identity).GetComponent<Ring>();
            ring.transform.SetParent(transform);
            ring.radius = 0;
            if(gameObject.activeSelf)
            StartCoroutine(AnimateRing(ring, sound));
        }

        audioSource.clip = NoiseAudioClips.Instance.GetClip(sound.type);
        audioSource.volume = sound.volume;
        audioSource.pitch = 1 + Random.Range(sound.variance.x, sound.variance.y);
        if(audioSource.enabled && gameObject.activeSelf)
        audioSource.Play();

        GameObject[] allZombies = GameObject.FindGameObjectsWithTag("Zombie");
        // List<Zombie> zombiesWithin = new List<Zombie>();
        foreach(GameObject zombieObj in allZombies) {
            if(Vector3.Distance(zombieObj.transform.position, transform.position) < sound.radius) {
                // zombiesWithin.Add(zombieObj.GetComponent<Zombie>());
                Zombie zombie = zombieObj.GetComponent<Zombie>();
                zombie.SetTarget(transform);
            }
        }
    }
    public void SetLoopingNoise(NoiseSound sound) {
        looping = true;
        loopingSound = sound;

        audioSource.clip = NoiseAudioClips.Instance.GetClip(sound.type);
        audioSource.volume = sound.volume;
        audioSource.pitch = 1 + Random.Range(sound.variance.x, sound.variance.y);
        audioSource.loop = true;
        audioSource.dopplerLevel = 0;
        // audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, )
        if(audioSource.enabled && gameObject.activeSelf)
        audioSource.Play();
    }
    public void StopLooping() {
        looping = false;
        audioSource.loop = false;
        loopingSound = new NoiseSound(NoiseSound.Type.None, 0, 0);
    }
    void Update() {
        if(looping) {
            GameObject[] allZombies = GameObject.FindGameObjectsWithTag("Zombie");
            foreach(GameObject zombieObj in allZombies) {
                if(Vector3.Distance(zombieObj.transform.position, transform.position) < loopingSound.radius) {
                    Zombie zombie = zombieObj.GetComponent<Zombie>();
                    zombie.SetTarget(transform);
                }
            }
        }
    }
    IEnumerator AnimateRing(Ring ring, NoiseSound sound) {
        // ring.radius = sound.radius;
        // ring.axisOffset = 0.25f;
        // ring.OnValidate();

        // yield return new WaitForSeconds(1);
        // Destroy(ring.gameObject);

        ring.radius = 0;
        ring.draw = true;
        ring.axisOffset = 0.25f;
        ring.color = new Color(1, 1, 1, 0.1f);
        ring.thickness = 0.1f;
        ring.UpdateVars();

        float timeElapsed = 0;
        float totalDuration = 0.2f;
        Destroy(ring.gameObject, totalDuration);
        while(timeElapsed < totalDuration) {
            ring.radius = Mathf.Lerp(0, sound.radius, timeElapsed / totalDuration);
            ring.color = new Color(1, 1, 1, Mathf.Lerp(0, 0.1f, timeElapsed / totalDuration));
            ring.UpdateVars();

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        ring.radius = sound.radius;
    }
}
