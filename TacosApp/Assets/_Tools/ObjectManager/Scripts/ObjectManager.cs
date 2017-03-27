using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/******************************************************************/
/* PoolableObject                                                 */
/* This class is the class Type every poolable class should       */
/* inherit in order to be managed by the Object Manager.          */
/******************************************************************/
public abstract class PoolableObject : CachedMonoBehaviour
{
    /*String that cotains the pool Id that spawns this object*/
	private string originPoolName = string.Empty;
    /*Should be true if on spawn should be called during the preloading of this object*/
    public bool callOnSpawnWhenPreloadedByPool = false;

    /*
    *  Function: Acces to this instance pool id
    *  Parameters: None
    *  Return: Spawning Pool Id
    */
    public string OriginObjectPool
    {
        get
        {
            return originPoolName;
        }
        set
        {
            originPoolName = value;
        }
    }

    /*
    *  Function: Called when this object is spawned by Object manager. Must override.
    *            used for initialize the gameobject and class particularities.
    *  Parameters: isFirstTime only is tru if OnSpawn is Called during preloading
    *  Return: None
    */
    abstract public void OnSpawn(bool isFirstTime);
    /*
    *  Function: Called when this object is despawned by Object manager.Must override.
    *            used for shutting down the object and class particularities.  
    *  Parameters: None
    *  Return: None
    */
    abstract public void OnDespawn();
}


/******************************************************************/
/* ObjectPool                                                     */
/* This class is the class Type that will hold the information    */
/* of every pool in the Object Manager and is the responsible     */
/* for spawning and despqwning the objects in the pool            */
/******************************************************************/
[System.Serializable]
public class ObjectPool
{ 
    /*Pool Unique Id*/
    public string poolId;
    /*Reference to the object this pool will control*/
    public PoolableObject poolableObjectPrefab;
    /*Number of instances to preload of the controlled object*/
    public int numberOfPreloadedObjects = 0;
    /*True if the instancing should be limited by number of instances*/
    public bool limitInstances = false;
    /*Instancing limit for this controlled object*/
    public int spawnLimit = 100;
    /*True to activate console printing capabilities*/
    public bool showDebugInfo = false;
    /*List containing a reference to every GameObject controlled by the pool.*/
	private List<PoolableObject> allPoolableObjectsList;
    /*List containing a reference to every SPAWNED GameObject controlled by the pool.*/
	private List<PoolableObject> spawnedPoolableObjectsList;
    /*List containing a reference to every DESPAWNED GameObject controlled by the pool.*/
	private List<PoolableObject> despawnedPoolableObjectsList;

    /*
    *  Function: Register the pool in the ObjectManager and preload instances
    *  Parameters: None
    *  Return: None
    */
    public void InitPool()
    {
        if (showDebugInfo)
        {
            Debug.Log("Init ObjectPool[" + poolId + "]");
        }
        allPoolableObjectsList = new List<PoolableObject>();
        spawnedPoolableObjectsList = new List<PoolableObject>();
        despawnedPoolableObjectsList = new List<PoolableObject>();

        RegisterPool();
        PreloadSpawns();
    }

    /*
    *  Function: Register the pool in the Object Manager
    *  Parameters: None
    *  Return: None
    */
    public void RegisterPool()
    {
        if (showDebugInfo)
        {
            Debug.Log("Registering ObjectPool[" + poolId + "]");
        }
        ObjectManager.AddPool(this);
    }

    /*
    *  Function: Preload(instance) all the instances setted in the editor or by code
    *  Parameters: None
    *  Return: None
    */
    public void PreloadSpawns()
    {
        if (showDebugInfo)
        {
            Debug.Log("Preload ObjectPool[" + poolId + "]");
        }

        if (limitInstances)
        {
            numberOfPreloadedObjects = (numberOfPreloadedObjects > spawnLimit ? spawnLimit : numberOfPreloadedObjects);
        }

        string appendedName = "_(Instance_0";
        for (int i = 0; i < numberOfPreloadedObjects; i++)
        {
            GameObject obj = GameObject.Instantiate(poolableObjectPrefab.CachedGameObject,
                                                    Vector3.zero, Quaternion.identity) as GameObject;
            if (obj != null)
            {
                PoolableObject spwnObj = obj.GetComponent<PoolableObject>();

                if (spwnObj != null)
                {
                    obj.name += appendedName + allPoolableObjectsList.Count + ")";
                    //set pool name on this object
                    spwnObj.OriginObjectPool = poolId;
                    //add to the main list
                    allPoolableObjectsList.Add(spwnObj);
                    if (spwnObj.callOnSpawnWhenPreloadedByPool)
                    {
                        spwnObj.OnSpawn(true);
                    }
                    Despawn(spwnObj);
                }
            }
        }
    }

    /*
    *  Function: 
    *  Parameters: None
    *  Return: None
    */
    public PoolableObject Spawn(Vector3 position = default(Vector3),
                                 Quaternion rotation = default(Quaternion),
                                 Transform newParent = null)
    {
        PoolableObject spawnObj = null;
        if (despawnedPoolableObjectsList.Count > 0)
        {
            spawnObj = despawnedPoolableObjectsList[0];
            despawnedPoolableObjectsList.RemoveAt(0);
        }
        else if (!limitInstances || allPoolableObjectsList.Count < spawnLimit)
        {
            GameObject obj = GameObject.Instantiate(poolableObjectPrefab.CachedGameObject,
                                                    Vector3.zero, Quaternion.identity) as GameObject;
            if (obj != null)
            {
                spawnObj = obj.GetComponent<PoolableObject>();
                if (spawnObj != null)
                {
                    obj.name += "_(Instance_0" + allPoolableObjectsList.Count + ")";
                    //set pool name on this object
                    spawnObj.OriginObjectPool = poolId;
                    //add to the main list
                    allPoolableObjectsList.Add(spawnObj);
                }
            }
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("Can't Spawn [" + poolableObjectPrefab.CachedGameObject.name +
                          "] from pool [" + poolId + "]");

            return null;
        }
        if (showDebugInfo)
            Debug.Log("Spawning [" + spawnObj.CachedGameObject.name +
                      "] from pool [" + spawnObj.OriginObjectPool + "]");
		if(spawnedPoolableObjectsList.Contains(spawnObj))
		{
			if(showDebugInfo)
			{
				Debug.Log("Already Spawned");
			}
		}
		else
		{
        	spawnedPoolableObjectsList.Add(spawnObj);
		}
		spawnObj.CachedTransform.SetParent(newParent);
        spawnObj.CachedTransform.localPosition = position;
        spawnObj.CachedTransform.localRotation = rotation;
        spawnObj.OnSpawn(false);
        return spawnObj;
    }

    /*
    *  Function: Despawn the poolable object passed, this method should never be called directly by other different than the Object Manager.
    *  Parameters: None
    *  Return: None
    */
    public void Despawn(PoolableObject objectToDespawn)
    {
        if (objectToDespawn != null)
        {
            if(objectToDespawn.OriginObjectPool == poolId)
            {
                if (showDebugInfo)
                    Debug.Log("Despawning [" + objectToDespawn.CachedGameObject.name +
                              "] from pool [" + objectToDespawn.OriginObjectPool);
                objectToDespawn.OnDespawn();
				objectToDespawn.CachedTransform.SetParent(ObjectManager.GetInstance().CachedTransform);
                spawnedPoolableObjectsList.Remove(objectToDespawn);
				if(!despawnedPoolableObjectsList.Contains(objectToDespawn))
				{
                	despawnedPoolableObjectsList.Add(objectToDespawn);
				}
				else
				{
					if(showDebugInfo)
					{
						Debug.Log("Already Despawned");
					}
				}
            }
        }
    }

    /*
    *  Function: Despawns all the SPAWNED instances of this pool.
    *  Parameters: None
    *  Return: None
    */
    public void DespawnAll()
    {
        if (showDebugInfo)
            Debug.Log("Despawn ALL from ["+poolId+"]");

        while (spawnedPoolableObjectsList.Count > 0)
        {
            Despawn(spawnedPoolableObjectsList[0]);
        }
    }

    /*
    *  Function: Object Pool Constructor.
    *  Parameter: poolId is the id of this pool and wont be accepted if it is not unique
    *  Parameter: prefab is the object that this pool will be controlling
    *  Parameter: preloadedInstances is the number of instances that should be instanced when creating this pool.
    *  Parameter: limitInstances marks if this pool should limit instancing.
    *  Parameter: limitCounter is the max number of instances allowed by this pool if limitInstances is True.
    *  Return: ObjectPool created
    */
    public ObjectPool(string poolID,PoolableObject prefab,int preLoadedInstances,bool mustLimitInstances,int limitCounter)
    {
        poolId = poolID;
        poolableObjectPrefab = prefab;
		limitInstances = mustLimitInstances;
        numberOfPreloadedObjects = preLoadedInstances;
        spawnLimit = limitCounter;
    }

    /*
    *  Function: Despawn and destroy every pooled object controlled by this pool.
    *  Parameters: None
    *  Return: None
    */
    public void DestroyPool()
    {
        DespawnAll();

        despawnedPoolableObjectsList.Clear();
        spawnedPoolableObjectsList.Clear();

        if (showDebugInfo)
            Debug.Log("Destroy ALL from [" + poolId + "]");
        while(allPoolableObjectsList.Count > 0)
        {
            PoolableObject obj = allPoolableObjectsList[0];
            allPoolableObjectsList.RemoveAt(0);
            MonoBehaviour.Destroy(obj);
        }
        allPoolableObjectsList.Clear();
    }

}

/******************************************************************/
/* ObjectManager                                                  */
/* This class is the responsible for organizing every pool        */
/* it contains.                                                   */
/******************************************************************/
public class ObjectManager : Manager<ObjectManager> 
{
    /*Is initialized alone, false if some other instance is going to initialize it*/
    public bool autoInitialize = true;
    /*Marks if the pool manager has been initialized*/
    public bool poolManagerInitialized = false;
    /*List of editor setted pools*/
    public List<ObjectPool> poolsList;
    /*Dictionary tha contains every managed pool in the ObjectManager*/
	private Dictionary<string, ObjectPool> poolsMap = new Dictionary<string,ObjectPool>();

    /*
    *  Function: Initialize all currently setted pools
    *  Parameters: None
    *  Return: None
    */
	protected override void Awake ()
	{
		base.Awake ();
		if(isThisManagerValid)
		{
			if (autoInitialize)
			{
				InitPools();
			}
		}
	}

	public override void StartManager()
	{
		if(alreadystarted)
			return;
		base.StartManager();
		InitPools();
	}
    /*
    *  Function: indexer overload to acces pools by id
    *  Parameters: None
    *  Return: Object pool if founded, null otherwise
    */
    public ObjectPool this[string poolId]
    {
        get
        {
            ObjectPool op;
            if (poolsMap.TryGetValue(poolId, out op))
            {
                return op;
            }
            else
            {
				if (_mustShowDebugInfo)
                    Debug.LogWarning("A pool with the id [" + poolId + "] doesn't exist!");
                return null; 
            }
        }
    }

    /*
    *  Function: GetPool static method to access pools by Id
    *  Parameters: None
    *  Return: Object Pool if founded, null otherwise.
    */
    public static ObjectPool GetPool(string poolId)
    {
       return GetInstance()[poolId];
    }

    /*
    *  Function: Add a pool to the dictionaries register in the Object Manager
    *  Parameters: Object pool to add if there are no previously one with that Id
    *  Return: None
    */
    public static void AddPool(ObjectPool objectPool)
    {
        if (objectPool != null)
        {
            if (!GetInstance().poolsMap.ContainsKey(objectPool.poolId))
            {
                GetInstance().poolsMap.Add(objectPool.poolId, objectPool);
                if(GetInstance().poolManagerInitialized)
				{
                    GetInstance().poolsList.Add(objectPool);
					objectPool.InitPool();
				}
            }
            else if (GetInstance()._mustShowDebugInfo)
            {
                Debug.LogWarning("A pool with the name [" + objectPool.poolId + "] already exist!");
            }
        }
    }

    /*
    *  Function: Creates a pool with the passed parameters, this is used when pools are needed on the run.
    *  Parameter: poolId is the id of this pool and wont be accepted if it is not unique
    *  Parameter: spawnable is the object that this pool will be controlling
    *  Parameter: preloadedInstances is the number of instances that should be instanced when creating this pool.
    *  Parameter: limitInstances marks if this pool should limit instancing.
    *  Parameter: limitCounter is the max number of instances allowed by this pool if limitInstances is True.
    *  Return: True if the pool was created correctly, false otherwise.
    */
    public static bool CreatePool(string poolId, PoolableObject spawnable, int preLoadedInstances = 0, bool limitInstances = false, int limitCounter = 0)
    {
        ObjectPool op = GetPool(poolId);
        if (op == null)
        {
            if (GetInstance()._mustShowDebugInfo)
                Debug.Log("As this pool doesnt exist we will create it.");
            op = new ObjectPool(poolId, spawnable, preLoadedInstances, limitInstances, limitCounter);
            if (GetInstance()._mustShowDebugInfo)
                op.showDebugInfo = true;
            op.InitPool();
            return true;
        }
        else if (GetInstance()._mustShowDebugInfo)
        {
            Debug.LogWarning("A pool with the name [" + poolId + "] already exist!");
        }
        return false;
    }

    /*
    *  Function: Destroy a pool and all its pooled objects
    *  Parameters: Pool id of the pool that is wanted to be destroyed
    *  Return: None
    */
    public static void DestroyPool(string poolId)
    {
        ObjectPool op = GetPool(poolId);
        if (op == null)
        {
            op.DestroyPool();
            GetInstance().poolsList.Remove(op);
            GetInstance().poolsMap.Remove(poolId);
        }
        else if (GetInstance()._mustShowDebugInfo)
        {
            Debug.LogWarning("A pool with the name [" + poolId + "] doesnt exist!");
        }
    }

    /*
    *  Function:  Spawn an object of the pool with the id passed.
    *  Parameter: poolId is the id of the pool that will spawn an object
    *  Parameter: position is the localposition where the object should be spawned
    *  Parameter: rotation is the localrotation that the object should have when spawned
    *  Parameter: newParent is the transform to where the new spawned object should be parented
    *  Return: PoolableObject spawned by the function.
    */
    public static PoolableObject Spawn(     string poolId,
                                            Vector3 position = default(Vector3),
                                            Quaternion rotation = default(Quaternion),
                                            Transform newParent = null)
    {
		if(_cachedInstance == null)
		{
			return null;
		}
        if (!GetInstance().poolManagerInitialized)
        {
            if (GetInstance()._mustShowDebugInfo)
                Debug.LogWarning("The Object Manager Is not initialized yet!");

            return null;
        }
        
        ObjectPool sp = ObjectManager.GetPool(poolId);
        if (sp != null)
        {
            return sp.Spawn(position, rotation, newParent);
        }
        else
        {
            if (GetInstance()._mustShowDebugInfo)
               Debug.LogWarning("A pool with the name [" + poolId + "] doesn't exist!. Can't spawn from there.");
            return null;
        }
    }

	public static R SpawnLike<R>( 	string poolId,
									Vector3 position = default(Vector3),
									Quaternion rotation = default(Quaternion),
									Transform newParent = null) where R:PoolableObject
	{
		PoolableObject obj = Spawn(poolId,position,rotation,newParent);
		if(obj != null)
		{
			return (R)obj;
		}
		return null;
	}

    /*
    *  Function: Despawn the object passed to the function, the object will be parented to the object manager.
    *  Parameters: objectToDespawn is the poolable object that should be despawned
    *  Return: None
    */
    public static void Despawn(PoolableObject objectToDespawn)
    {
		if(_cachedInstance == null || objectToDespawn == null)
			return;
        if (!GetInstance().poolManagerInitialized)
            return;
        if (objectToDespawn != null)
        {
            ObjectPool sp = ObjectManager.GetPool(objectToDespawn.OriginObjectPool);
            if (sp != null)
            {
                sp.Despawn(objectToDespawn);
            }
            else if(GetInstance()._mustShowDebugInfo)
            {
                Debug.LogWarning("A pool with the name [" + objectToDespawn.OriginObjectPool + "] doesn't exist!. Can't despawn from there.");
            }
        }
    }

    /*
    *  Function: Despawns all objects from a pool
    *  Parameters: poolId of the pool that will despawn all its objects
    *  Return: None
    */
    public static void DespawnAllObjectsFromPool(string poolId)
    {
        ObjectPool sp = ObjectManager.GetPool(poolId);
        if (sp != null)
        {
            sp.DespawnAll();
        }
    }

    /*
    *  Function: Init all the setted in editor pools
    *  Parameters: None
    *  Return: None
    */
    private void InitPools()
    {
		if(!poolManagerInitialized)
		{
	        if (_mustShowDebugInfo)
			{
	            Debug.Log("Initializing pools");
			}
	        for (int i = 0; i < poolsList.Count; i++)
	        {
	            poolsList[i].InitPool();
	        }
		}
        poolManagerInitialized = true;
    }

}
