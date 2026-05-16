using System.Collections.Generic;
using System.IO;
using ClockworkSurvivor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClockworkSurvivor.EditorTools
{
    public static class ClockworkBootstrap
    {
        private const string ScenePath = "Assets/Scenes/ClockworkSurvivor.unity";
        private const string DataRoot = "Assets/ClockworkSurvivor/Data";
        private const string PrefabRoot = "Assets/ClockworkSurvivor/Prefabs";
        private const string MaterialRoot = "Assets/ClockworkSurvivor/Materials";

        [InitializeOnLoadMethod]
        private static void SetupProjectOnFirstLoad()
        {
            EditorApplication.delayCall += delegate
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && !File.Exists(ScenePath))
                {
                    SetupProject(false);
                }
            };
        }

        [MenuItem("Clockwork Survivor/Setup Project")]
        public static void SetupProjectFromMenu()
        {
            SetupProject(true);
        }

        private static void SetupProject(bool overwriteScene)
        {
            EnsureFolders();
            ConfigurePlayerSettings();

            Material playerMaterial = CreateMaterial(MaterialRoot + "/Player_Copper.mat", new Color(0.95f, 0.5f, 0.18f));
            Material walkerMaterial = CreateMaterial(MaterialRoot + "/Enemy_Walker.mat", new Color(0.95f, 0.22f, 0.2f));
            Material dasherMaterial = CreateMaterial(MaterialRoot + "/Enemy_Dasher.mat", new Color(0.95f, 0.78f, 0.18f));
            Material shooterMaterial = CreateMaterial(MaterialRoot + "/Enemy_Shooter.mat", new Color(0.25f, 0.55f, 1f));
            Material groundMaterial = CreateMaterial(MaterialRoot + "/Arena_Floor.mat", new Color(0.24f, 0.25f, 0.23f));
            Material wallMaterial = CreateMaterial(MaterialRoot + "/Arena_Wall.mat", new Color(0.36f, 0.31f, 0.26f));
            Material projectileMaterial = CreateMaterial(MaterialRoot + "/Player_Projectile.mat", new Color(0.36f, 0.95f, 1f));
            Material enemyProjectileMaterial = CreateMaterial(MaterialRoot + "/Enemy_Projectile.mat", new Color(1f, 0.25f, 0.35f));
            Material gemMaterial = CreateMaterial(MaterialRoot + "/Experience_Gem.mat", new Color(0.25f, 1f, 0.55f));

            GameObject playerProjectilePrefab = CreateProjectilePrefab(PrefabRoot + "/ClockBoltProjectile.prefab", projectileMaterial, 0.28f);
            GameObject enemyProjectilePrefab = CreateProjectilePrefab(PrefabRoot + "/EnemyProjectile.prefab", enemyProjectileMaterial, 0.22f);
            ExperienceGem gemPrefab = CreateGemPrefab(PrefabRoot + "/ExperienceGem.prefab", gemMaterial);
            WeaponConfig weapon = CreateWeapon(playerProjectilePrefab);
            GameObject walkerPrefab = CreateEnemyPrefab(PrefabRoot + "/Walker.prefab", walkerMaterial, new Vector3(1f, 1f, 1f));
            GameObject dasherPrefab = CreateEnemyPrefab(PrefabRoot + "/Dasher.prefab", dasherMaterial, new Vector3(0.8f, 0.8f, 1.4f));
            GameObject shooterPrefab = CreateEnemyPrefab(PrefabRoot + "/Shooter.prefab", shooterMaterial, new Vector3(1.05f, 1.05f, 1.05f));

            EnemyConfig walker = CreateEnemyConfig("Walker", EnemyBehavior.Walker, walkerPrefab, null, gemPrefab, new Color(0.95f, 0.22f, 0.2f), 12f, 3.2f, 8f, 1, 0f, 1f);
            EnemyConfig dasher = CreateEnemyConfig("Dasher", EnemyBehavior.Dasher, dasherPrefab, null, gemPrefab, new Color(0.95f, 0.78f, 0.18f), 10f, 3.8f, 12f, 2, 28f, 0.5f);
            EnemyConfig shooter = CreateEnemyConfig("Shooter", EnemyBehavior.Shooter, shooterPrefab, enemyProjectilePrefab, gemPrefab, new Color(0.25f, 0.55f, 1f), 16f, 2.5f, 5f, 3, 55f, 0.35f);
            List<UpgradeDefinition> upgrades = CreateUpgrades();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (overwriteScene || !File.Exists(ScenePath))
            {
                CreateScene(weapon, new List<EnemyConfig> { walker, dasher, shooter }, upgrades, playerMaterial, groundMaterial, wallMaterial);
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            }

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Clockwork Survivor project setup complete.");
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                DataRoot,
                PrefabRoot,
                MaterialRoot,
                "Assets/Scenes"
            };

            for (int i = 0; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(folders[i]))
                {
                    Directory.CreateDirectory(folders[i]);
                }
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "Portfolio";
            PlayerSettings.productName = "Clockwork Survivor";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        }

        private static Material CreateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                if (shader == null)
                {
                    shader = Shader.Find("Unlit/Color");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreateProjectilePrefab(string path, Material material, float scale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab;
            }

            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = Path.GetFileNameWithoutExtension(path);
            projectile.transform.localScale = Vector3.one * scale;
            projectile.GetComponent<Renderer>().sharedMaterial = material;
            SphereCollider collider = projectile.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            Rigidbody body = projectile.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            projectile.AddComponent<Projectile>();

            prefab = PrefabUtility.SaveAsPrefabAsset(projectile, path);
            Object.DestroyImmediate(projectile);
            return prefab;
        }

        private static ExperienceGem CreateGemPrefab(string path, Material material)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab.GetComponent<ExperienceGem>();
            }

            GameObject gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gem.name = "ExperienceGem";
            gem.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            gem.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = gem.GetComponent<Collider>();
            collider.isTrigger = true;
            Rigidbody body = gem.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            gem.AddComponent<ExperienceGem>();

            GameObject saved = PrefabUtility.SaveAsPrefabAsset(gem, path);
            Object.DestroyImmediate(gem);
            return saved.GetComponent<ExperienceGem>();
        }

        private static WeaponConfig CreateWeapon(GameObject projectilePrefab)
        {
            string path = DataRoot + "/ClockBolt.asset";
            WeaponConfig weapon = AssetDatabase.LoadAssetAtPath<WeaponConfig>(path);
            if (weapon == null)
            {
                weapon = ScriptableObject.CreateInstance<WeaponConfig>();
                AssetDatabase.CreateAsset(weapon, path);
            }

            weapon.weaponName = "Clock Bolt";
            weapon.projectilePrefab = projectilePrefab;
            weapon.damage = 8f;
            weapon.fireRate = 2.2f;
            weapon.range = 13f;
            weapon.projectileSpeed = 15f;
            weapon.projectileLifetime = 1.15f;
            weapon.projectileCount = 1;
            weapon.spreadAngle = 12f;
            EditorUtility.SetDirty(weapon);
            return weapon;
        }

        private static GameObject CreateEnemyPrefab(string path, Material material, Vector3 scale)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                return prefab;
            }

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = Path.GetFileNameWithoutExtension(path);
            enemy.transform.localScale = scale;
            enemy.GetComponent<Renderer>().sharedMaterial = material;
            Rigidbody body = enemy.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            Health health = enemy.AddComponent<Health>();
            health.Configure(12f);
            enemy.AddComponent<EnemyController>();

            prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
            Object.DestroyImmediate(enemy);
            return prefab;
        }

        private static EnemyConfig CreateEnemyConfig(string name, EnemyBehavior behavior, GameObject prefab, GameObject projectilePrefab, ExperienceGem gemPrefab, Color tint, float health, float speed, float damage, int xp, float unlock, float weight)
        {
            string path = DataRoot + "/" + name + ".asset";
            EnemyConfig config = AssetDatabase.LoadAssetAtPath<EnemyConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<EnemyConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.enemyName = name;
            config.behavior = behavior;
            config.prefab = prefab;
            config.projectilePrefab = projectilePrefab;
            config.experiencePrefab = gemPrefab;
            config.tint = tint;
            config.maxHealth = health;
            config.moveSpeed = speed;
            config.contactDamage = damage;
            config.experienceValue = xp;
            config.unlockAfterSeconds = unlock;
            config.spawnWeight = weight;
            config.dashRange = 6f;
            config.dashSpeed = 10f;
            config.dashDuration = 0.32f;
            config.dashCooldown = 2.4f;
            config.preferredDistance = 7f;
            config.projectileDamage = 7f;
            config.projectileSpeed = 8f;
            config.shootCooldown = 1.7f;
            EditorUtility.SetDirty(config);
            return config;
        }

        private static List<UpgradeDefinition> CreateUpgrades()
        {
            List<UpgradeDefinition> upgrades = new List<UpgradeDefinition>();
            upgrades.Add(CreateUpgrade("Sharper Gears", "攻撃ダメージ +18%", UpgradeKind.DamageMultiplier, 0.18f, 5));
            upgrades.Add(CreateUpgrade("Rapid Spring", "攻撃速度 +18%", UpgradeKind.FireRateMultiplier, 0.18f, 5));
            upgrades.Add(CreateUpgrade("Twin Barrel", "同時発射数 +1", UpgradeKind.ProjectileCount, 1f, 3));
            upgrades.Add(CreateUpgrade("Overwound Boots", "移動速度 +0.55", UpgradeKind.MoveSpeed, 0.55f, 4));
            upgrades.Add(CreateUpgrade("Reinforced Core", "最大HP +12", UpgradeKind.MaxHealth, 12f, 4));
            upgrades.Add(CreateUpgrade("Magnet Coil", "経験値取得範囲 +1.2", UpgradeKind.PickupRadius, 1.2f, 4));
            upgrades.Add(CreateUpgrade("Burst Gear", "敵撃破時に小爆発", UpgradeKind.ExplosionOnKill, 8f, 4));
            return upgrades;
        }

        private static UpgradeDefinition CreateUpgrade(string name, string description, UpgradeKind kind, float amount, int maxStacks)
        {
            string path = DataRoot + "/Upgrade_" + name.Replace(" ", "") + ".asset";
            UpgradeDefinition upgrade = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
            if (upgrade == null)
            {
                upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
                AssetDatabase.CreateAsset(upgrade, path);
            }

            upgrade.upgradeName = name;
            upgrade.description = description;
            upgrade.kind = kind;
            upgrade.amount = amount;
            upgrade.maxStacks = maxStacks;
            EditorUtility.SetDirty(upgrade);
            return upgrade;
        }

        private static void CreateScene(WeaponConfig weapon, List<EnemyConfig> enemies, List<UpgradeDefinition> upgrades, Material playerMaterial, Material groundMaterial, Material wallMaterial)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Arena Floor";
            ground.transform.localScale = new Vector3(3.6f, 1f, 3.6f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            CreateArenaWalls(wallMaterial);
            CreateDecorativeGears(wallMaterial);

            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Player";
            playerObject.transform.position = new Vector3(0f, 1f, 0f);
            playerObject.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            playerObject.GetComponent<Renderer>().sharedMaterial = playerMaterial;
            Rigidbody playerBody = playerObject.AddComponent<Rigidbody>();
            playerBody.useGravity = false;
            playerBody.constraints = RigidbodyConstraints.FreezeRotation;
            Health playerHealth = playerObject.AddComponent<Health>();
            playerHealth.Configure(42f);
            PlayerController player = playerObject.AddComponent<PlayerController>();
            PlayerCombat playerCombat = playerObject.AddComponent<PlayerCombat>();
            PickupCollector pickupCollector = playerObject.AddComponent<PickupCollector>();

            GameObject firePoint = new GameObject("Fire Point");
            firePoint.transform.SetParent(playerObject.transform);
            firePoint.transform.localPosition = new Vector3(0f, 0.65f, 0.65f);
            playerCombat.firePoint = firePoint.transform;
            playerCombat.baseWeapon = weapon;

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 11f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.1f, 0.11f);
            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
            follow.target = playerObject.transform;
            cameraObject.transform.position = playerObject.transform.position + follow.offset;
            cameraObject.transform.rotation = Quaternion.Euler(58f, 0f, 0f);

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            GameObject managerObject = new GameObject("Game Manager");
            GameManager manager = managerObject.AddComponent<GameManager>();
            EnemySpawner spawner = managerObject.AddComponent<EnemySpawner>();
            UpgradeSystem upgradeSystem = managerObject.AddComponent<UpgradeSystem>();

            spawner.player = playerObject.transform;
            spawner.enemyConfigs = enemies;
            upgradeSystem.player = player;
            upgradeSystem.playerCombat = playerCombat;
            upgradeSystem.playerHealth = playerHealth;
            upgradeSystem.pickupCollector = pickupCollector;
            upgradeSystem.upgradePool = upgrades;

            UIController ui = CreateUi();
            manager.player = player;
            manager.playerHealth = playerHealth;
            manager.enemySpawner = spawner;
            manager.upgradeSystem = upgradeSystem;
            manager.uiController = ui;

            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void CreateArenaWalls(Material material)
        {
            CreateWall("North Wall", new Vector3(0f, 0.5f, 18f), new Vector3(36f, 1f, 0.6f), material);
            CreateWall("South Wall", new Vector3(0f, 0.5f, -18f), new Vector3(36f, 1f, 0.6f), material);
            CreateWall("East Wall", new Vector3(18f, 0.5f, 0f), new Vector3(0.6f, 1f, 36f), material);
            CreateWall("West Wall", new Vector3(-18f, 0.5f, 0f), new Vector3(0.6f, 1f, 36f), material);
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateDecorativeGears(Material material)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 position = new Vector3(Mathf.Cos(angle), 0.08f, Mathf.Sin(angle)) * 12.5f;
                GameObject gear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                gear.name = "Floor Gear " + (i + 1);
                gear.transform.position = position;
                gear.transform.localScale = new Vector3(1.8f, 0.06f, 1.8f);
                gear.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static UIController CreateUi()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            UIController ui = canvasObject.AddComponent<UIController>();

            ui.hudPanel = CreatePanel(canvasObject.transform, "HUD", new Color(0f, 0f, 0f, 0f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ui.timerText = CreateText(ui.hudPanel.transform, "Timer", font, "03:00", 34, TextAnchor.UpperCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(220f, 48f));
            ui.healthText = CreateText(ui.hudPanel.transform, "Health", font, "HP 42 / 42", 20, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(240f, 34f));
            ui.levelText = CreateText(ui.hudPanel.transform, "Level", font, "LV 1", 20, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -58f), new Vector2(140f, 34f));
            ui.killsText = CreateText(ui.hudPanel.transform, "Kills", font, "DEFEATED 0", 20, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(220f, 34f));
            ui.experienceSlider = CreateSlider(ui.hudPanel.transform, "Experience", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(420f, 18f));

            ui.titlePanel = CreatePanel(canvasObject.transform, "Title Panel", new Color(0.02f, 0.025f, 0.03f, 0.82f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateText(ui.titlePanel.transform, "Title", font, "CLOCKWORK SURVIVOR", 48, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(760f, 72f));
            CreateText(ui.titlePanel.transform, "Subtitle", font, "3分間、歯車仕掛けの闘技場で生き残る", 22, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), Vector2.zero, new Vector2(560f, 42f));
            ui.startButton = CreateButton(ui.titlePanel.transform, "Start Button", font, "START", new Vector2(0.5f, 0.39f), new Vector2(0.5f, 0.39f), Vector2.zero, new Vector2(220f, 56f));

            ui.upgradePanel = CreatePanel(canvasObject.transform, "Upgrade Panel", new Color(0.02f, 0.025f, 0.03f, 0.78f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateText(ui.upgradePanel.transform, "Upgrade Title", font, "SELECT UPGRADE", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), Vector2.zero, new Vector2(520f, 54f));
            ui.upgradeButtons = new Button[3];
            ui.upgradeTexts = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                float x = 0.27f + 0.23f * i;
                Button button = CreateButton(ui.upgradePanel.transform, "Upgrade Button " + (i + 1), font, "Upgrade", new Vector2(x, 0.48f), new Vector2(x, 0.48f), Vector2.zero, new Vector2(250f, 148f));
                Text text = button.GetComponentInChildren<Text>();
                text.fontSize = 20;
                text.supportRichText = true;
                ui.upgradeButtons[i] = button;
                ui.upgradeTexts[i] = text;
            }

            ui.resultPanel = CreatePanel(canvasObject.transform, "Result Panel", new Color(0.02f, 0.025f, 0.03f, 0.82f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            ui.resultTitleText = CreateText(ui.resultPanel.transform, "Result Title", font, "SURVIVED", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), Vector2.zero, new Vector2(540f, 72f));
            ui.resultStatsText = CreateText(ui.resultPanel.transform, "Result Stats", font, "Time  00:00\nLevel  1\nDefeated  0", 24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 108f));
            ui.restartButton = CreateButton(ui.resultPanel.transform, "Restart Button", font, "RESTART", new Vector2(0.5f, 0.33f), new Vector2(0.5f, 0.33f), Vector2.zero, new Vector2(220f, 56f));

            ui.titlePanel.SetActive(true);
            ui.hudPanel.SetActive(false);
            ui.upgradePanel.SetActive(false);
            ui.resultPanel.SetActive(false);
            return ui;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, Font font, string content, int size, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color(0.95f, 0.93f, 0.86f);
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, Font font, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.75f, 0.44f, 0.18f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.95f, 0.62f, 0.24f, 1f);
            colors.pressedColor = new Color(0.55f, 0.28f, 0.12f, 1f);
            button.colors = colors;

            Text text = CreateText(buttonObject.transform, "Label", font, label, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            text.color = Color.white;
            return button;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(parent, false);
            RectTransform rect = sliderObject.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 10f;
            slider.value = 0f;

            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObject.transform, false);
            RectTransform backgroundRect = background.AddComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.25f, 1f, 0.55f, 1f);
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            return slider;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
