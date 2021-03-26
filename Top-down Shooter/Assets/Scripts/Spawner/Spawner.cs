using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemyPrefab;
    public Color spawningColor = Color.red;

    private LivingEntity _playerEntity;
    private Transform _playerTransform;
    private MapGenerator _mapGenerator;

    private Wave _currentWave;
    private int _currentWaveNumber;

    private int _enemiesRemainingToSpawn;
    private int _enemiesRemainingAlive;
    private float _nextSpawnTime;

    private float _timeBetweenCampingChecks = 2f;
    private float _campingThresholdDistance = 1.5f; // distance required to move once camping to stop camping
    private float _nextCampingCheckTime;
    private Vector3 _campingPositionOld;
    private bool _isPlayerCamping;

    private bool _isDisabled = false;

    public event System.Action<int> OnNewWave;

    private void Start()
    {
        _playerEntity = FindObjectOfType<Player>();
        _playerTransform = _playerEntity.transform;
        _playerEntity.OnDeath += OnPlayerDeath;

        _nextCampingCheckTime = _timeBetweenCampingChecks + Time.time;
        _campingPositionOld = _playerTransform.position;

        _mapGenerator = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    private void Update()
    {
        if (!_isDisabled)
        {
            if (Time.time > _nextCampingCheckTime)
            {
                _nextCampingCheckTime = Time.time + _timeBetweenCampingChecks;

                _isPlayerCamping = (Vector3.Distance(_playerTransform.position, _campingPositionOld) < _campingThresholdDistance);
                _campingPositionOld = _playerTransform.position;
            }

            if (_enemiesRemainingToSpawn > 0 && Time.time > _nextSpawnTime)
            {
                _enemiesRemainingToSpawn--;
                _nextSpawnTime = Time.time + _currentWave.timeBetweenSpawns;

                StartCoroutine(SpawnEnemy());
            }
        }
    }

    private void OnPlayerDeath()
    {
        _isDisabled = true;
    }

    private void OnEnemyDeath()
    {
        _enemiesRemainingAlive--;

        if (_enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    private void ResetPlayerPosition()
    {
        _playerTransform.position = _mapGenerator.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    private void NextWave()
    {
        _currentWaveNumber++;
        if (_currentWaveNumber - 1 < waves.Length)
        {
            _currentWave = waves[_currentWaveNumber - 1];

            _enemiesRemainingToSpawn = _currentWave.enemyCount;
            _enemiesRemainingAlive = _enemiesRemainingToSpawn;

            if (OnNewWave != null)
            {
                OnNewWave(_currentWaveNumber);
            }
            ResetPlayerPosition();
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1f;
        float tileFlashSpeed = 4f;

        Transform spawnTile = _mapGenerator.GetRandomOpenTile();
        if (_isPlayerCamping)
        {
            spawnTile = _mapGenerator.GetTileFromPosition(_playerTransform.position);
        }

        Material tileMaterial = spawnTile.GetComponent<Renderer>().material;
        Color originalColour = tileMaterial.color;

        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMaterial.color = Color.Lerp(originalColour, spawningColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1f));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = (Enemy) Instantiate(enemyPrefab, spawnTile.position + Vector3.up, Quaternion.identity);
        spawnedEnemy.OnDeath += OnEnemyDeath;
    }
}
